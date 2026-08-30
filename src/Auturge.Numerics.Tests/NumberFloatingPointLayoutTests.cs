using System.Numerics;

namespace Auturge.Numerics.Tests;

/// <summary>
/// Covers the <see cref="System.IFloatingPoint{TSelf}"/> introspection surface on
/// <see cref="Number"/>: the significand/exponent byte and bit counts, the big/little-endian
/// writers, and the mathematical constants.
/// </summary>
[TestFixture]
public class NumberFloatingPointLayoutTests
{
    [Test]
    public void MathematicalConstants_HaveExpectedLeadingDigits()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.E.ToString(), Does.StartWith("2.71828"));
            Assert.That(Number.Pi.ToString(), Does.StartWith("3.14159"));
            Assert.That(Number.Tau.ToString(), Does.StartWith("6.28318"));
            Assert.That(Number.Pi + Number.E, Is.EqualTo(Number.E + Number.Pi), "addition commutes");
        });
    }

    [Test]
    public void SignificandByteCount_MatchesTheRawValueEncoding()
    {
        Number value = new(new BigInteger(123456789), 2);

        Assert.That(value.GetSignificandByteCount(),
            Is.EqualTo(((BigInteger)123456789).GetByteCount(isUnsigned: true)));
        Assert.That(value.GetSignificandBitLength(), Is.GreaterThan(0));
    }

    [Test]
    public void ExponentCounts_AreDerivedFromDecimalOffset()
    {
        Number value = new(new BigInteger(5), 3); // offset 3

        Assert.Multiple(() =>
        {
            Assert.That(value.GetExponentByteCount(), Is.EqualTo(new BigInteger(3).GetByteCount()));
            Assert.That(value.GetExponentShortestBitLength(),
                Is.EqualTo((int)new BigInteger(3).GetBitLength()));
        });
    }

    [Test]
    public void TryWriteSignificand_RoundTripsThroughBigInteger_BothEndiannesses()
    {
        Number value = new(new BigInteger(987654321), 0);
        byte[] big = new byte[value.GetSignificandByteCount()];
        byte[] little = new byte[value.GetSignificandByteCount()];

        bool wroteBig = value.TryWriteSignificandBigEndian(big, out int bigWritten);
        bool wroteLittle = value.TryWriteSignificandLittleEndian(little, out int littleWritten);
        var fromBig = new BigInteger(big.AsSpan(0, bigWritten), isUnsigned: true, isBigEndian: true);
        var fromLittle = new BigInteger(little.AsSpan(0, littleWritten), isUnsigned: true, isBigEndian: false);

        Assert.Multiple(() =>
        {
            Assert.That(wroteBig, Is.True);
            Assert.That(wroteLittle, Is.True);
            Assert.That(fromBig, Is.EqualTo((BigInteger)987654321));
            Assert.That(fromLittle, Is.EqualTo((BigInteger)987654321));
        });
    }

    [Test]
    public void TryWriteSignificand_WhenDestinationTooSmall_ReturnsFalse()
    {
        Number value = new(new BigInteger(987654321), 0);

        bool wrote = value.TryWriteSignificandBigEndian(Span<byte>.Empty, out int written);

        Assert.Multiple(() =>
        {
            Assert.That(wrote, Is.False);
            Assert.That(written, Is.EqualTo(0));
        });
    }

    [Test]
    public void TryWriteExponent_WritesTheDecimalOffset()
    {
        Number value = new(new BigInteger(5), 7);
        byte[] buffer = new byte[value.GetExponentByteCount()];

        bool wrote = value.TryWriteExponentLittleEndian(buffer, out int written);
        var fromBuffer = new BigInteger(buffer.AsSpan(0, written), isUnsigned: false, isBigEndian: false);

        Assert.Multiple(() =>
        {
            Assert.That(wrote, Is.True);
            Assert.That(fromBuffer, Is.EqualTo((BigInteger)7));
        });
    }
}
