using static Auturge.Quantity.Units;

namespace Auturge.Quantity.Tests;

/// <summary>
/// Covers <see cref="UnitDefinition"/> — the &lt;base unit, exponent&gt; map behind compound units
/// like m/s or kg·m·s⁻² — including its multiply / divide / reciprocal algebra, value equality,
/// and the <c>m s^-2</c> style rendering.
/// </summary>
[TestFixture]
public class UnitDefinitionTests
{
    private static UnitDefinition Def(params (Unit unit, short power)[] entries)
    {
        var definition = new UnitDefinition();
        foreach ((Unit unit, short power) in entries)
        {
            definition.Add(unit, power);
        }

        return definition;
    }

    [Test]
    public void ToString_RendersSymbolsWithExponentsOnlyWhenNotOne()
    {
        UnitDefinition definition = Def((Meters, 1), (Seconds, -2));

        Assert.That(definition.ToString(), Is.EqualTo("m s^-2"));
    }

    [Test]
    public void EmptyDefinition_RendersAsEmptyString()
    {
        Assert.That(new UnitDefinition().ToString(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void CopyConstructor_ClonesEntries()
    {
        UnitDefinition source = Def((Meters, 2));

        var copy = new UnitDefinition(source);
        copy.Add(Seconds, -1);

        Assert.Multiple(() =>
        {
            Assert.That(source, Has.Count.EqualTo(1), "the original is not mutated");
            Assert.That(copy, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Multiplication_AddsMatchingExponentsAndUnionsTheRest()
    {
        UnitDefinition left = Def((Meters, 1));
        UnitDefinition right = Def((Meters, 1), (Seconds, -1));

        UnitDefinition product = left * right;

        Assert.Multiple(() =>
        {
            Assert.That(product[Meters], Is.EqualTo(2));
            Assert.That(product[Seconds], Is.EqualTo(-1));
        });
    }

    [Test]
    public void Division_SubtractsMatchingExponents()
    {
        UnitDefinition left = Def((Meters, 1));
        UnitDefinition right = Def((Seconds, 1));

        UnitDefinition quotient = left / right;

        Assert.Multiple(() =>
        {
            Assert.That(quotient[Meters], Is.EqualTo(1));
            Assert.That(quotient[Seconds], Is.EqualTo(-1));
        });
    }

    [Test]
    public void Reciprocal_NegatesEveryExponent()
    {
        UnitDefinition definition = Def((Meters, 1), (Seconds, -2));

        UnitDefinition reciprocal = definition.Reciprocal();

        Assert.Multiple(() =>
        {
            Assert.That(reciprocal[Meters], Is.EqualTo(-1));
            Assert.That(reciprocal[Seconds], Is.EqualTo(2));
        });
    }

    [Test]
    public void Equality_IsByEntrySetRegardlessOfInsertionOrder()
    {
        UnitDefinition a = Def((Meters, 1), (Seconds, -1));
        UnitDefinition b = Def((Seconds, -1), (Meters, 1));
        UnitDefinition c = Def((Meters, 1), (Seconds, -2));

        Assert.Multiple(() =>
        {
            Assert.That(a == b, Is.True);
            Assert.That(a.Equals(b), Is.True);
            Assert.That(a != c, Is.True);
        });
    }

    [Test]
    public void GetHashCode_Should_BeEqual_When_DefinitionsAreEqualRegardlessOfInsertionOrder()
    {
        UnitDefinition a = Def((Meters, 1), (Seconds, -1));
        UnitDefinition b = Def((Seconds, -1), (Meters, 1));

        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void GetHashCode_Should_LetAnEqualDefinitionResolveTheSameKey_When_UsedInADictionary()
    {
        UnitDefinition a = Def((Meters, 1), (Seconds, -1));
        UnitDefinition b = Def((Seconds, -1), (Meters, 1));
        var lookup = new Dictionary<UnitDefinition, string> { [a] = "velocity" };

        Assert.That(lookup[b], Is.EqualTo("velocity"));
    }

    [Test]
    public void EqualityOperator_HandlesNullOperands()
    {
        Assert.Multiple(() =>
        {
            Assert.That((UnitDefinition?)null == null, Is.True);
            Assert.That(new UnitDefinition() == null, Is.False);
        });
    }

    [Test]
    public void GetUnitsWhere_FiltersByExponentPredicate()
    {
        UnitDefinition definition = Def((Meters, 1), (Seconds, -1), (Kilograms, 1));

        List<Unit> numerator = definition.GetUnitsWhere(entry => entry.Value > 0);

        Assert.That(numerator, Is.EquivalentTo(new[] { Meters, Kilograms }));
    }

    [Test]
    public void IncludeBaseUnits_ReturnsSelfAndLeavesAlreadyDefinedUnitsUntouched()
    {
        // Library units already carry a {self: 1} definition, so this is a fluent no-op for them —
        // the real work happens when composing units whose operand definitions are pre-populated.
        UnitDefinition definition = Def((Meters, 1));

        UnitDefinition result = definition.IncludeBaseUnits(Meters);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(definition));
            Assert.That(result[Meters], Is.EqualTo(1));
        });
    }
}
