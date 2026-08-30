using System.Globalization;
using System.Numerics;

namespace Auturge.Numerics.Tests;

/// <summary>
/// Covers <see cref="Number.ToString(string?)"/>, <see cref="Number.ToString(string?, IFormatProvider?)"/>
/// and <see cref="Number.TryFormat"/> against the .NET standard numeric format strings.
/// The parameterless <see cref="Number.ToString()"/> is exercised by <c>NumberConstructionTests</c>.
/// </summary>
[TestFixture]
public class NumberFormattingTests
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private static Number Parse(string s) => Number.Parse(s, Invariant);

    private static NumberFormatInfo GermanStyle() => new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
        NumberGroupSizes = [3],
        NegativeSign = "-",
        CurrencySymbol = "€",
        CurrencyDecimalSeparator = ",",
        CurrencyGroupSeparator = ".",
        CurrencyGroupSizes = [3],
        CurrencyDecimalDigits = 2,
        CurrencyNegativePattern = 8,
    };

    [Test]
    public void ToString_GeneralWithoutPrecision_Should_MatchRoundTrip_When_ValueHasFraction()
    {
        Number value = Parse("-1234567.8901");

        Assert.Multiple(() =>
        {
            Assert.That(value.ToString("G", Invariant), Is.EqualTo("-1234567.8901"));
            Assert.That(value.ToString("R", Invariant), Is.EqualTo("-1234567.8901"));
            Assert.That(value.ToString((string?)null, Invariant), Is.EqualTo("-1234567.8901"));
            Assert.That(value.ToString("G", Invariant), Is.EqualTo(value.ToString()));
        });
    }

    [Test]
    public void ToString_RoundTrip_Should_PreserveEveryDigit_When_ValueIsArbitraryPrecision()
    {
        Number value = Parse("123456789012345678901234567890.123456789012345678901234567891");

        Assert.That(value.ToString("R", Invariant),
            Is.EqualTo("123456789012345678901234567890.123456789012345678901234567891"));
    }

    [Test]
    public void ToString_General_Should_RoundToSignificantDigits_When_PrecisionSupplied()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Parse("123.4546").ToString("G4", Invariant), Is.EqualTo("123.5"));
            Assert.That(Parse("0.00012345").ToString("G3", Invariant), Is.EqualTo("0.000123"));
            Assert.That(Parse("123456").ToString("G4", Invariant), Is.EqualTo("123500"));
        });
    }

    [Test]
    public void ToString_Fixed_Should_ApplyPrecisionAndPadWithZeros()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Parse("1234.5").ToString("F2", Invariant), Is.EqualTo("1234.50"));
            Assert.That(Parse("1234.56789").ToString("F3", Invariant), Is.EqualTo("1234.568"));
            Assert.That(Parse("7").ToString("F0", Invariant), Is.EqualTo("7"));
        });
    }

    [Test]
    public void ToString_Fixed_Should_RoundHalfAwayFromZero()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Parse("0.125").ToString("F2", Invariant), Is.EqualTo("0.13"));
            Assert.That(Parse("2.5").ToString("F0", Invariant), Is.EqualTo("3"));
            Assert.That(Parse("0.999999").ToString("F2", Invariant), Is.EqualTo("1.00"));
        });
    }

    [Test]
    public void ToString_Fixed_Should_KeepNegativeSign_When_ValueIsNegative()
    {
        Assert.That(Parse("-1234.56").ToString("F2", Invariant), Is.EqualTo("-1234.56"));
    }

    [Test]
    public void ToString_Fixed_Should_DropNegativeSign_When_ValueRoundsToZero()
    {
        Assert.That(Parse("-0.001").ToString("F2", Invariant), Is.EqualTo("0.00"));
    }

    [Test]
    public void ToString_Fixed_Should_UseNumberFormatInfoDecimalSeparator()
    {
        Assert.That(Parse("-1234.5").ToString("F2", GermanStyle()), Is.EqualTo("-1234,50"));
    }

    [Test]
    public void ToString_Number_Should_InsertGroupSeparators()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Parse("1234567.891").ToString("N2", Invariant), Is.EqualTo("1,234,567.89"));
            Assert.That(Parse("12").ToString("N0", Invariant), Is.EqualTo("12"));
            Assert.That(Parse("0.5").ToString("N2", Invariant), Is.EqualTo("0.50"));
        });
    }

    [Test]
    public void ToString_Number_Should_UseCultureGroupingAndSign()
    {
        Assert.That(Parse("-1234567.89").ToString("N3", GermanStyle()), Is.EqualTo("-1.234.567,890"));
    }

    [Test]
    public void ToString_Number_Should_HonorNegativePattern_When_ParenthesesRequested()
    {
        NumberFormatInfo info = (NumberFormatInfo)Invariant.NumberFormat.Clone();
        info.NumberNegativePattern = 0;

        Assert.That(Parse("-12.5").ToString("N1", info), Is.EqualTo("(12.5)"));
    }

    [Test]
    public void ToString_Currency_Should_UseSymbolGroupingAndDecimalDigits()
    {
        NumberFormatInfo info = (NumberFormatInfo)Invariant.NumberFormat.Clone();
        info.CurrencySymbol = "$";
        info.CurrencyNegativePattern = 1;
        info.CurrencyPositivePattern = 0;

        Assert.Multiple(() =>
        {
            Assert.That(Parse("1234.5").ToString("C", info), Is.EqualTo("$1,234.50"));
            Assert.That(Parse("-1234.5").ToString("C2", info), Is.EqualTo("-$1,234.50"));
        });
    }

    [Test]
    public void ToString_Currency_Should_FollowCultureNegativePattern()
    {
        Assert.That(Parse("-1234.5").ToString("C", GermanStyle()), Is.EqualTo("-1.234,50 €"));
    }

    [Test]
    public void ToString_Percent_Should_MultiplyByOneHundredAndAppendSymbol()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Parse("0.5").ToString("P2", Invariant), Is.EqualTo("50.00 %"));
            Assert.That(Parse("-0.39678").ToString("P1", Invariant), Is.EqualTo("-39.7 %"));
        });
    }

    [Test]
    public void ToString_Exponential_Should_ProduceScientificNotation()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Parse("0.5").ToString("E2", Invariant), Is.EqualTo("5.00E-001"));
            Assert.That(Parse("1234.5").ToString("E2", Invariant), Is.EqualTo("1.23E+003"));
            Assert.That(Parse("9.999").ToString("E2", Invariant), Is.EqualTo("1.00E+001"));
            Assert.That(Parse("0").ToString("E3", Invariant), Is.EqualTo("0.000E+000"));
        });
    }

    [Test]
    public void ToString_Exponential_Should_LowercaseTheExponentMarker_When_SpecifierIsLowercase()
    {
        Assert.That(Parse("-1234.5").ToString("e2", Invariant), Is.EqualTo("-1.23e+003"));
    }

    [Test]
    public void ToString_Decimal_Should_PadWithLeadingZerosAndKeepSign()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Parse("1234").ToString("D6", Invariant), Is.EqualTo("001234"));
            Assert.That(Parse("-1234").ToString("D6", Invariant), Is.EqualTo("-001234"));
            Assert.That(Parse("42").ToString("D", Invariant), Is.EqualTo("42"));
        });
    }

    [Test]
    public void ToString_Hex_Should_ProduceHexadecimalDigits()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Parse("255").ToString("X4", Invariant), Is.EqualTo("00FF"));
            Assert.That(Parse("255").ToString("x", Invariant), Is.EqualTo("0ff"));
        });
    }

    [Test]
    public void ToString_Binary_Should_ProduceBinaryDigits()
    {
        Assert.That(Parse("10").ToString("B8", Invariant), Is.EqualTo("00001010"));
    }

    [Test]
    public void ToString_IntegralOnlyFormats_Should_ThrowFormatException_When_ValueHasFraction()
    {
        Number value = Parse("1.5");

        Assert.Multiple(() =>
        {
            Assert.Throws<FormatException>(() => value.ToString("D", Invariant));
            Assert.Throws<FormatException>(() => value.ToString("X", Invariant));
            Assert.Throws<FormatException>(() => value.ToString("B", Invariant));
        });
    }

    [Test]
    public void ToString_UnknownSpecifier_Should_ThrowFormatException()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<FormatException>(() => Parse("1").ToString("Z", Invariant));
            Assert.Throws<FormatException>(() => Parse("1").ToString("F2F", Invariant));
        });
    }

    [Test]
    public void ToString_SingleArgumentOverload_Should_UseCurrentCulture()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
        try
        {
            Assert.That(Parse("1234.5").ToString("F2"), Is.EqualTo("1234,50"));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Test]
    public void TryFormat_Should_WriteFormattedValue_When_DestinationIsLargeEnough()
    {
        char[] buffer = new char[32];
        bool ok = Parse("-1234.56").TryFormat(buffer, out int written, "N2", Invariant);
        string formatted = new(buffer, 0, written);

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.True);
            Assert.That(formatted, Is.EqualTo("-1,234.56"));
        });
    }

    [Test]
    public void TryFormat_Should_ReturnFalse_When_DestinationIsTooSmall()
    {
        char[] buffer = new char[3];
        bool ok = Parse("1234.56").TryFormat(buffer, out int written, "N2", Invariant);

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.False);
            Assert.That(written, Is.EqualTo(0));
        });
    }

    [Test]
    public void TryFormat_Should_ThrowFormatException_When_IntegralFormatMeetsFraction()
    {
        Number value = Parse("1.5");
        Assert.Throws<FormatException>(() => value.TryFormat(new char[32], out _, "X", Invariant));
    }
}
