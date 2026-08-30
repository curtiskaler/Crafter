namespace Auturge.Quantity.Tests;

/// <summary>
/// Covers <see cref="Synonym"/>: the alternate name/symbol record used by dimensions and units,
/// its constructors (including the <see cref="IHaveNameAndSymbol"/> copy), the null-name guard,
/// and its rendering.
/// </summary>
[TestFixture]
public class SynonymTests
{
    [Test]
    public void Constructor_WithNameAndSymbol_SetsBoth()
    {
        var synonym = new Synonym("weight", "W");

        Assert.Multiple(() =>
        {
            Assert.That(synonym.DisplayName, Is.EqualTo("weight"));
            Assert.That(synonym.Symbol, Is.EqualTo("W"));
        });
    }

    [Test]
    public void Constructor_WithNullDisplayName_ThrowsArgumentNullException()
    {
        Assert.That(() => new Synonym(null, "W"), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_WithNullSymbol_IsAllowed()
    {
        var synonym = new Synonym("weight", null);

        Assert.That(synonym.Symbol, Is.Null);
    }

    [Test]
    public void Constructor_FromIHaveNameAndSymbol_CopiesBothMembers()
    {
        var source = new Synonym("stress", "σ");

        var copy = new Synonym(source);

        Assert.Multiple(() =>
        {
            Assert.That(copy.DisplayName, Is.EqualTo("stress"));
            Assert.That(copy.Symbol, Is.EqualTo("σ"));
        });
    }

    [Test]
    public void ParameterlessConstructor_LeavesBothMembersNull()
    {
        var synonym = new Synonym();

        Assert.Multiple(() =>
        {
            Assert.That(synonym.DisplayName, Is.Null);
            Assert.That(synonym.Symbol, Is.Null);
        });
    }

    [Test]
    public void ToString_RendersNameAndSymbol()
    {
        Assert.That(new Synonym("weight", "W").ToString(), Is.EqualTo("weight (W)"));
    }
}
