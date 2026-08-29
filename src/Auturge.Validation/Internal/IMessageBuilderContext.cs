namespace Auturge.Validation;

public interface IMessageBuilderContext<TObj, out TProperty>
{
    IRuleComponent<TObj, TProperty> Component { get; }

    // string DisplayName { get; }
    TObj? InstanceToValidate { get; }
    MessageFormatter MessageFormatter { get; }
    ValidationContext<TObj> ParentContext { get; }
    string PropertyName { get; }
    IPropertyValidator PropertyValidator { get; }
    TProperty PropertyValue { get; }
    string GetDefaultMessage();
}

public class MessageBuilderContext<T, TProperty> : IMessageBuilderContext<T, TProperty>
{
    private readonly ValidationContext<T> _innerContext;

    public RuleComponent<T, TProperty> Component { get; }
    IRuleComponent<T, TProperty> IMessageBuilderContext<T, TProperty>.Component => Component;

    // public string DisplayName => _innerContext.DisplayName;
    public T? InstanceToValidate => _innerContext.InstanceToValidate;
    public MessageFormatter MessageFormatter => _innerContext.MessageFormatter;
    public ValidationContext<T> ParentContext => _innerContext;
    public string PropertyName => _innerContext.PropertyPath;
    public IPropertyValidator PropertyValidator => Component.Validator;
    public TProperty? PropertyValue { get; }

    public MessageBuilderContext(ValidationContext<T> innerContext, TProperty? value,
        RuleComponent<T, TProperty> component)
    {
        _innerContext = innerContext;
        PropertyValue = value;
        Component = component;
    }

    public string GetDefaultMessage() => Component.GetErrorMessage(_innerContext, PropertyValue);
}
