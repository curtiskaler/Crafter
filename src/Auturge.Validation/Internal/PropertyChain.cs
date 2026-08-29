using System.Linq.Expressions;
using System.Reflection;

namespace Auturge.Validation;

/// <summary>
/// A chain of nested properties.
/// </summary>
public class PropertyChain
{
    private readonly List<string> _memberNames = new(2);
    
    /// <summary> Number of member names in the chain. </summary>
    public int Count => _memberNames.Count;
    
    /// <summary> Creates a new PropertyChain. </summary>
    public PropertyChain()
    {
    }

    /// <summary> Creates a new PropertyChain based on another. </summary>
    public PropertyChain(PropertyChain? parent) : this(parent?._memberNames ?? [])
    {
    }

    /// <summary> Creates a new PropertyChain. </summary>
    /// <param name="memberNames"></param>
    public PropertyChain(IEnumerable<string> memberNames)
    {
        _memberNames.AddRange(memberNames);
    }

    /// <summary> Adds a MemberInfo instance to the chain. </summary>
    /// <param name="member">Member to add</param>
    public void Add(MemberInfo member) => _memberNames.Add(member.Name);

    /// <summary> Adds a property name to the chain. </summary>
    /// <param name="propertyName">Name of the property to add</param>
    public void Add(string propertyName) => _memberNames.Add(propertyName);

    
    /// <summary>
    /// Adds an indexer to the property chain. For example, if the following chain has been constructed:
    /// Parent.Child
    /// then calling AddIndexer(0) would convert this to:
    /// Parent.Child[0]
    /// </summary>
    /// <param name="indexer"></param>
    /// <param name="surroundWithBrackets">Whether square brackets should be applied before and after the indexer. Default true.</param>
    public void AddIndexer(object indexer, bool surroundWithBrackets)
    {
        if(_memberNames.Count == 0) {
            throw new InvalidOperationException("Could not apply an Indexer because the property chain is empty.");
        }

        string last = _memberNames[^1];
        last += surroundWithBrackets ? "[" + indexer + "]" : indexer;

        _memberNames[^1] = last;
    }
    
    /// <summary> Builds a property path. </summary>
    public string BuildPropertyPath(string? propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        
        if (_memberNames.Count == 0)
        {
            return propertyName;
        }

        var chain = new PropertyChain(this);
        chain.Add(propertyName);
        return chain.ToString();
    }

    /// <summary> A string that represents the current property chain. </summary>
    public override string ToString() => _memberNames.Count switch
    {
        0 => string.Empty,
        1 => _memberNames[0],
        _ => string.Join('.', _memberNames)
    };

    /// <summary> Creates a PropertyChain from a lambda expression. </summary>
    public static PropertyChain FromExpression(LambdaExpression expression)
    {
        var memberNames = new Stack<string>();
        MemberExpression? memberExp = expression.Body.GetMemberExpression();
        while(memberExp != null) {
            memberNames.Push(memberExp.Member.Name);
            memberExp = memberExp.Expression?.GetMemberExpression();
        }
        return new PropertyChain(memberNames);
    }

}
