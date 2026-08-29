using System.Text.RegularExpressions;

namespace Auturge.Validation;

public partial class MessageFormatter
{
    [GeneratedRegex("{([^{}:]+)(?::([^{}]+))?}")]
    private static partial Regex KeyRegex();
    
    /// <summary> Default Property Name placeholder. </summary>
    public const string PropertyName = "PropertyName";

    /// <summary> Default Property Value placeholder. </summary>
    public const string PropertyValue = "PropertyValue";

    /// <summary> Additional placeholder values. </summary>
    public Dictionary<string, object?> PlaceholderValues { get; } = new(2);

    /// <summary> Adds a value for a validation message placeholder. </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public MessageFormatter AppendArgument(string name, object? value) {
        PlaceholderValues[name] = value;
        return this;
    }

    /// <summary> Appends a property name to the message. </summary>
    /// <param name="name">The name of the property</param>
    /// <returns></returns>
    public MessageFormatter AppendPropertyName(string name) 
        => AppendArgument(PropertyName, name);

    /// <summary> Appends a property value to the message. </summary>
    /// <param name="value">The value of the property</param>
    /// <returns></returns>
    public MessageFormatter AppendPropertyValue(object? value) 
        => AppendArgument(PropertyValue, value);

    /// <summary> Constructs the final message from the specified template. </summary>
    /// <param name="messageTemplate">Message template</param>
    /// <returns>The message with placeholders replaced with their appropriate values</returns>
    public virtual string BuildMessage(string messageTemplate) =>
        KeyRegex().Replace(messageTemplate, m =>	{
            string key = m.Groups[1].Value;

            if (!PlaceholderValues.TryGetValue(key, out object? value))
                return m.Value; // No placeholder / value

            string? format = m.Groups[2].Success // Format specified?
                ? $"{{0:{m.Groups[2].Value}}}"
                : null;

            return format == null
                ? value?.ToString() ?? string.Empty
                : string.Format(format, value);
        });
    
    internal void Reset() => PlaceholderValues.Clear();
}
