using System.ComponentModel.DataAnnotations;
using Auturge.Stores;

namespace Crafter.Model.Recipes;

public class Product(long? id = null, string? displayName = null) : StoredEntity(id), IDisplayEntity
{
    public string DisplayName { get; set; } = displayName ?? string.Empty;
    
    public static Product None => new(-1, null);
    
    public ValidationResult? ValidateBeforeSave(IState? state) => null;
}
