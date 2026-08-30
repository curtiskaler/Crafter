using System.Globalization;
using System.Numerics;

namespace Auturge.Numerics.Tests;

/// <summary>
/// Covers <see cref="Number"/> string parsing: signs, decimal/group separators, currency,
/// culture sensitivity, and the failure modes for <c>Parse</c> (throws) versus <c>TryParse</c>
/// (returns <see langword="false"/>). Scientific-notation parsing lives in
/// <see cref="NumberScientificNotationTests"/>.
/// </summary>
[TestFixture]
public class NumberParsingTests
{
    private static readonly CultureInfo _invariant = CultureInfo.InvariantCulture;

    [Test]
    public void Parse_PlainInteger_ProducesIntegralNumber()
    {
        Number number = Number.Parse("12345", _invariant);

        Assert.Multiple(() =>
        {
            Assert.That(number, Is.EqualTo(new Number(12345L)));
            Assert.That(number.IsIntegral, Is.True);
        });
    }

    [Test]
    public void Parse_DecimalValue_KeepsFractionalDigits()
    {
        Assert.That(Number.Parse("3.14159", _invariant), Is.EqualTo(new Number(new BigInteger(314159), 5)));
    }

    [Test]
    public void Parse_LeadingZerosAndTrailingFractionalZeros_AreNormalizedAway()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.Parse("007", _invariant), Is.EqualTo(new Number(7L)));
            Assert.That(Number.Parse("2.500", _invariant), Is.EqualTo(new Number(new BigInteger(25), 1)));
        });
    }

    [Test]
    public void Parse_NegativeAndExplicitlyPositiveSigns_AreHonored()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.Parse("-42.5", _invariant), Is.EqualTo(new Number(new BigInteger(-425), 1)));
            Assert.That(Number.Parse("+42.5", _invariant), Is.EqualTo(new Number(new BigInteger(425), 1)));
        });
    }

    [Test]
    public void Parse_WithGroupSeparators_StripsThemWhenAllowThousandsIsSet()
    {
        Assert.That(Number.Parse("1,234,567", _invariant), Is.EqualTo(new Number(1_234_567L)));
    }

    [Test]
    public void Parse_RespectsCultureSpecificSeparators()
    {
        CultureInfo german = CultureInfo.GetCultureInfo("de-DE"); // ',' decimal, '.' group

        Assert.That(Number.Parse("1.234,56", german), Is.EqualTo(new Number(new BigInteger(123456), 2)));
    }

    [Test]
    public void Parse_CurrencyFormattedValue_IsAccepted()
    {
        Number number = Number.Parse("$1,234.50", CultureInfo.GetCultureInfo("en-US"));

        Assert.That(number, Is.EqualTo(new Number(new BigInteger(12345), 1)));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Parse_NullEmptyOrWhitespace_ThrowsArgumentException(string? value)
    {
        Assert.That(() => Number.Parse(value!, _invariant), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase("abc")]
    [TestCase("1.2.3")]
    [TestCase("12x")]
    public void Parse_NonNumericText_ThrowsArgumentException(string value)
    {
        Assert.That(() => Number.Parse(value, _invariant), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public void TryParse_OnValidInput_ReturnsTrueAndValue()
    {
        bool ok = Number.TryParse("-0.75", _invariant, out Number result);

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.True);
            Assert.That(result, Is.EqualTo(new Number(new BigInteger(-75), 2)));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("not-a-number")]
    public void TryParse_OnInvalidInput_ReturnsFalse(string? value)
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.TryParse(value, _invariant, out Number result), Is.False);
            Assert.That(result, Is.EqualTo(default(Number)));
        });
    }

    [Test]
    public void Parse_SpanOverload_MatchesStringOverload()
    {
        ReadOnlySpan<char> span = "987.6".AsSpan();

        Assert.That(Number.Parse(span, _invariant), Is.EqualTo(Number.Parse("987.6", _invariant)));
    }

    [Test]
    [SetCulture("en-US")]
    public void Parse_RoundTripsThroughToString()
    {
        foreach (string text in new[] { "0", "1", "-1", "12345", "0.001", "-9999.9999" })
        {
            Number parsed = Number.Parse(text, _invariant);
            Assert.That(Number.Parse(parsed.ToString(), _invariant), Is.EqualTo(parsed), $"round-trip of {text}");
        }
    }
}
