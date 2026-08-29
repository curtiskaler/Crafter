namespace Auturge.Validation.Tests;

public class TestUser(string? name, string? email = null, int? bones = null)
{
    public string? Name { get; set; } = name;
    public string? Email { get; set; } = email;
    public int? Bones { get; set; } = bones;
    public string? Website { get; set; } = null;
}
