using Auturge.Numerics;

namespace Auturge.Quantity.Tests;

/// <summary>
/// Covers <see cref="SIPrefix{T}"/> and the <see cref="Number"/>-backed <see cref="SIPrefix"/>
/// shortcut: the two constructors (with and without an explicit divisor), the null guards, and
/// the way a bare factor implies a unit divisor.
/// </summary>
[TestFixture]
public class SIPrefixTests
{
    [Test]
    public void Constructor_WithFactorAndDivisor_StoresBoth()
    {
        var centi = new SIPrefix<Rational>("centi", "c", factor: 1, divisor: 100);

        Assert.Multiple(() =>
        {
            Assert.That(centi.DisplayName, Is.EqualTo("centi"));
            Assert.That(centi.Symbol, Is.EqualTo("c"));
            Assert.That(centi.Factor, Is.EqualTo(Rational.One));
            Assert.That(centi.Divisor, Is.EqualTo(new Rational(100, 1)));
        });
    }

    [Test]
    public void Constructor_WithBareFactor_DefaultsDivisorToTheMultiplicativeIdentity()
    {
        var kilo = new SIPrefix<Rational>("kilo", "k", factor: 1000);

        Assert.Multiple(() =>
        {
            Assert.That(kilo.Factor, Is.EqualTo(new Rational(1000, 1)));
            Assert.That(kilo.Divisor, Is.EqualTo(Rational.MultiplicativeIdentity));
        });
    }

    [Test]
    public void Constructor_WithNullDisplayNameOrSymbol_Throws()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => new SIPrefix<Rational>(null!, "k", 1000), Throws.InstanceOf<ArgumentNullException>());
            Assert.That(() => new SIPrefix<Rational>("kilo", null!, 1000), Throws.InstanceOf<ArgumentNullException>());
        });
    }

    [Test]
    public void NumberBackedShortcut_DefaultsDivisorToOne()
    {
        var milli = new SIPrefix("milli", "m", factor: new Number(1L), divisor: new Number(1000L));
        var deca = new SIPrefix("deca", "da", factor: new Number(10L));

        Assert.Multiple(() =>
        {
            Assert.That((decimal)milli.Divisor, Is.EqualTo(1000m));
            Assert.That((decimal)deca.Divisor, Is.EqualTo(1m));
            Assert.That((decimal)deca.Factor, Is.EqualTo(10m));
        });
    }

    [Test]
    public void LibraryPrefixes_HaveTheExpectedRatios()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SIPrefixes.Kilo.Factor, Is.EqualTo(new Rational(1000, 1)));
            Assert.That(SIPrefixes.Centi.Divisor, Is.EqualTo(new Rational(100, 1)));
            Assert.That(SIPrefixes.Milli.Divisor, Is.EqualTo(new Rational(1000, 1)));
        });
    }
}
