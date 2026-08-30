using Auturge.Quantity;

namespace Crafter.Model.Recipes;

// NOTE: IngredientAmount is NOT a linking object.


public class IngredientAmount(Ingredient ingredient, Quantity amount)
{
    public Ingredient Ingredient { get; } = ingredient;
    public Quantity Amount { get; } = amount;
}
