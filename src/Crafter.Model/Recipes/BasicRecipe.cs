using Auturge.Quantity;
using Auturge.Stores;
using Crafter.Model.Identifiers;

namespace Crafter.Model.Recipes;

public class BasicRecipe : StoredEntity, IRecipe, IEquatable<BasicRecipe>
{
    /// <inheritdoc/>
    public string DisplayName { get; set; }
    
    /// <inheritdoc/>
    public List<IngredientAmount> Ingredients { get; set; }

    public Product Product1 { get; set; }
    public IProduct Product2 { get; set; }
    public URNReference Product3 { get; set; }

    /// <inheritdoc/>
    public URNReference Product { get; set; }
    
    /// <inheritdoc/>
    public Quantity Yield { get; set; }
    
    /// <inheritdoc/>
    public List<IRequirement> Requirements { get; set; }
    
    /// <inheritdoc/>
    public bool Equals(BasicRecipe? other) => throw new NotImplementedException();
}
