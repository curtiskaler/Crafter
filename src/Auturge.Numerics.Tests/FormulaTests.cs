namespace Auturge.Numerics.Tests;

/// <summary>
/// Covers <see cref="Formula{TValue,T1}"/>: a formula is an ordered pipeline of
/// <c>(value, arg) =&gt; value</c> steps, and <see cref="Formula{TValue,T1}.Apply"/> folds an
/// input through every step in turn.
/// </summary>
[TestFixture]
public class FormulaTests
{
    private sealed class LinearFormula() : Formula<double, double>(
        (value, slope) => value * slope,
        (value, _) => value + 1);

    private sealed class IdentityFormula() : Formula<int, int>();

    [Test]
    public void Apply_FoldsEveryOperationInOrder()
    {
        var formula = new LinearFormula();

        // ((3 * 10) + 1)
        double result = formula.Apply(3, 10);

        Assert.That(result, Is.EqualTo(31));
    }

    [Test]
    public void Apply_WithNoOperations_ReturnsInputUnchanged()
    {
        var formula = new IdentityFormula();

        Assert.That(formula.Apply(42, 99), Is.EqualTo(42));
    }

    [Test]
    public void Constructor_RecordsEveryOperation()
    {
        var formula = new LinearFormula();

        Assert.That(formula.Operations, Has.Count.EqualTo(2));
    }

    [Test]
    public void Formula_IsAnIFormula()
    {
        Assert.That(new LinearFormula(), Is.InstanceOf<IFormula>());
    }
}
