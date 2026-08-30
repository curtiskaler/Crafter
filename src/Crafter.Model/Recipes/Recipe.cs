// ReSharper disable UnusedType.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

using System.ComponentModel.DataAnnotations;
using Auturge.Quantity;
using Auturge.Stores;
using Crafter.Model.Identifiers;

namespace Crafter.Model.Recipes;

public class Recipe : StoredEntity, IDisplayEntity, IEquatable<Recipe>
{
    /// <summary> The name of the recipe. </summary>
    public string DisplayName => Product?.DisplayName ?? string.Empty;

    /// <summary> The ingredients and amounts. </summary>
    public List<IngredientAmount> Ingredients { get; internal set; }

    /// <summary> The item being crafted. </summary>
    public Product? Product { get; protected set; }

    /// <summary> The total number of portions, servings, items or total weight/volume produced. </summary>
    public Quantity Yield { get; protected set; } = 1;

    // // this might be better as a hierarchical... thing. That can be decomposed into step-by-step.
    // public string Instructions { get; } = "";

    /// <summary>
    /// Make parameters include Time, Temperature, etc.
    /// </summary>
    public Dictionary<string, IParameter> MakeParameters { get; set; } = [];

    /// <summary>
    /// Equipment used in the creation of this product.
    /// </summary>
    public List<Equipment> Equipment { get; set; } = [];

    /// <summary>
    /// Internal or external references.
    /// </summary>
    public Dictionary<string, URNReference> References { get; set; } = [];

    public ValidationResult? ValidateBeforeSave(IState? state) => null;

    #region ctors

    public Recipe() : this(null)
    {
    }

    public Recipe(Product? product, IEnumerable<IngredientAmount>? ingredients = null) : base(product?.Id)
    {
        Ingredients = ingredients?.ToList() ?? [];
        Product = product;
    }

    public static Recipe None => new(Product.None);

    #endregion ctors

    #region Equality

    public bool Equals(Recipe? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && Product?.Id == other.Product?.Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Recipe)obj);
    }

    public override int GetHashCode() => HashCode.Combine(DisplayName, Id);

    public static bool operator ==(Recipe? lhs, Recipe? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Recipe? lhs, Recipe? rhs) => !(lhs == rhs);

    #endregion Equality
}

// TODO:
//  crafted (High quality) bonuses
//  crafted (Low quality) detriments
//  MATH formula handler
// public List<Adjustment> Adjustments { get; } = [];
// TODO: step-by-step instructions
// TODO: equipment
