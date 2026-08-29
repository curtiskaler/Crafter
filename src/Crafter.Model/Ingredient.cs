using System.ComponentModel.DataAnnotations.Schema;
using Auturge.Quantity;
using Auturge.Stores;

namespace Crafter.Model;

public class Ingredient(Component component, Quantity amount) : StoredEntity, IDisplayEntity
{
    public Component Component { get; } = component;
    public Quantity Amount { get; } = amount;
    
    [NotMapped] // <- Transient / doesn't go into the database
    public string DisplayName => Component.DisplayName;
}
