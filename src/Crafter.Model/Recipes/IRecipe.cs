using Auturge.Quantity;
using Crafter.Model.Identifiers;

namespace Crafter.Model.Recipes;

public interface IRecipe : IDisplayEntity
{
    /// <summary> The name of the recipe. </summary>
    new string DisplayName { get; set; }

    /// <summary> The ingredients and amounts. </summary>
    List<IngredientAmount> Ingredients { get; internal set; }

    /// <summary> The item being crafted. </summary>
    Product Product1 { get; protected set; }

    IProduct Product2 { get; protected set; }

    URNReference Product3 { get; protected set; }
    
    /// <summary> The total number of portions, servings, items or total weight/volume produced. </summary>
    Quantity Yield { get; protected set; }
    
    /// <summary> A list of things that must exist or occur before one may execute the recipe. </summary>
    List<IRequirement> Requirements { get; internal set; }
    
}

public interface IProduct: IDisplayEntity
{
}

/// <summary>
/// A dummy class, not intended to be used, but rather to display the various "types" of requirements.
/// </summary>
internal class RequirementsList
{
    /// <summary> Equipment and tools required to execute the recipe. </summary>
    /// <example>"Must have the sign-off of the lab director."</example>
    public List<IRequirement> Equipment { get; internal set; }
    
    /// <summary> Conditions required to execute the recipe. </summary>
    /// <example>"Must be facing north, under the full moon."</example>
    public List<IRequirement> Preconditions { get; internal set; }
    
    /// <summary> Credentials required to execute the recipe. </summary>
    /// <example>"Must have the Carpenter job/class."</example>
    public List<IRequirement> Credentials { get; internal set; }
    
    /// <summary> Signatures required to execute the recipe. </summary>
    /// <example>"Must have the sign-off of the lab director."</example>
    public List<IRequirement> Signatures { get; internal set; }
}
