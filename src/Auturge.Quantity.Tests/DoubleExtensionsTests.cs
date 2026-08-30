namespace Auturge.Quantity.Tests;

/// <summary>
/// Covers <see cref="DoubleExtensions"/>: the epsilon-tolerant floating-point equality check and
/// the <c>ExpectOne</c> guard used by the reciprocal-unit operators (e.g. <c>1.0 / Seconds</c>).
/// </summary>
[TestFixture]
public class DoubleExtensionsTests
{
    [Test]
    public void ApproxEqual_WithinTolerance_IsTrue()
    {
        Assert.Multiple(() =>
        {
            Assert.That(1.0.ApproxEqual(1.0), Is.True);
            Assert.That(0.1.ApproxEqual(0.1 + 1e-12, epsilon: 1e-9), Is.True);
        });
    }

    [Test]
    public void ApproxEqual_OutsideTolerance_IsFalse()
    {
        Assert.Multiple(() =>
        {
            Assert.That(1.0.ApproxEqual(1.01), Is.False);
            Assert.That(1.0.ApproxEqual(1.0001, epsilon: 1e-6), Is.False);
        });
    }

    [Test]
    public void ExpectOne_WhenValueIsOne_DoesNotThrow()
    {
        Assert.That(() => 1.0.ExpectOne(), Throws.Nothing);
    }

    [Test]
    public void ExpectOne_WhenValueIsNotOne_ThrowsArgumentOutOfRange()
    {
        Assert.That(() => 2.0.ExpectOne(), Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ReciprocalUnitOperator_RejectsANumeratorThatIsNotOne()
    {
        Assert.That(() => { Unit _ = 2.0 / Units.Seconds; }, Throws.InstanceOf<ArgumentOutOfRangeException>());
    }
}
