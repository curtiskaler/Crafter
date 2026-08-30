namespace Auturge.Quantity.Tests;

/// <summary>
/// Covers <see cref="Conversion"/> / <see cref="Conversion{T}"/> and the <see cref="Bijection"/>
/// base: applying a conversion, inverting it, composing conversions with <c>*</c>, the chain
/// constructor, and the guard that a bijection always has at least one function each way.
/// </summary>
[TestFixture]
public class ConversionTests
{
    private static Conversion PlusOne() => new(x => (int)x + 1, x => (int)x - 1);

    private static Conversion TimesTen() => new(x => (int)x * 10, x => (int)x / 10);

    [Test]
    public void Identity_ReturnsItsInputUnchanged()
    {
        Assert.That(Conversion.Identity.Execute(42), Is.EqualTo(42));
    }

    [Test]
    public void Execute_AppliesTheForwardFunction()
    {
        Assert.That(PlusOne().Execute(10), Is.EqualTo(11));
    }

    [Test]
    public void Invert_UndoesTheConversion()
    {
        Conversion conversion = PlusOne();

        object round = conversion.Invert().Execute(conversion.Execute(10));

        Assert.That(round, Is.EqualTo(10));
    }

    [Test]
    public void Multiplication_ComposesConversionsLeftToRight()
    {
        // (x + 1) then (x * 10)
        Conversion composed = PlusOne() * TimesTen();

        Assert.Multiple(() =>
        {
            Assert.That(composed.Execute(4), Is.EqualTo(50));
            Assert.That(composed.Invert().Execute(50), Is.EqualTo(4));
        });
    }

    [Test]
    public void ChainConstructor_WithEmptyList_Throws()
    {
        Assert.That(() => new Conversion(new List<Conversion>()), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public void Bijection_WithNoForwardFunctions_Throws()
    {
        Assert.That(
            () => new Conversion(new List<Func<object, object>>(), [x => x]),
            Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public void GenericConversion_Identity_ReturnsInput()
    {
        Assert.That(Conversion<int>.Identity.Execute(7), Is.EqualTo(7));
    }

    [Test]
    public void GenericConversion_ExecuteAndInvert_AreTyped()
    {
        var conversion = new Conversion<double>(celsius => celsius * 9 / 5 + 32, f => (f - 32) * 5 / 9);

        Assert.Multiple(() =>
        {
            Assert.That(conversion.Execute(100), Is.EqualTo(212));
            Assert.That(((IBijection<double>)conversion).Invert().Execute(212), Is.EqualTo(100).Within(1e-9));
        });
    }

    [Test]
    public void FunctionExtensions_BoxAndUnbox_RoundTripADelegate()
    {
        Func<int, int> original = x => x * 3;

        Func<object, object> boxed = original.Box();
        Func<int, int> unboxed = boxed.Unbox<int>();

        Assert.That(unboxed(4), Is.EqualTo(12));
    }
}
