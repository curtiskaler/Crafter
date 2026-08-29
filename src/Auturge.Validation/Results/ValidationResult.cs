// ReSharper disable MemberCanBePrivate.Global

using System.Diagnostics;

namespace Auturge.Validation;

/// <summary>
/// The result of running a validator.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ValidationResult : IEquatable<ValidationResult>
{
    /// <summary> A successful validation: no errors. </summary>
    public static readonly ValidationResult Success = new();

    /// <summary> Whether validation succeeded. </summary>
    public virtual bool IsValid => Errors.Count == 0;

    /// <summary> A collection of errors. </summary>
    public List<ValidationFailure> Errors { get; }

    /// <summary> Creates a new ValidationResult. </summary>
    public ValidationResult()
    {
        Errors = [];
    }

    /// <summary> Creates a new ValidationResult from a collection of failures. </summary>
    /// <param name="failures">Collection of <see cref="ValidationFailure"/> instances which is later available through the <see cref="Errors"/> property.</param>
    /// <remarks>
    /// Any nulls will be excluded.
    /// The list is copied.
    /// </remarks>
    public ValidationResult(IEnumerable<ValidationFailure> failures)
    {
        Errors = [.. failures];
    }

    /// <summary> Creates a new ValidationResult by combining several other ValidationResults. </summary>
    /// <param name="results"></param>
    public ValidationResult(IEnumerable<ValidationResult> results)
    {
        Errors = [.. results.SelectMany(x => x.Errors)];
    }

    /// <summary>
    /// Generates a string representation of the error messages separated by new lines.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => ToString(Environment.NewLine);

    /// <summary>
    /// Generates a string representation of the error messages separated by the specified character.
    /// </summary>
    /// <param name="separator">The character to separate the error messages.</param>
    /// <returns></returns>
    public string ToString(string separator)
        => string.Join(separator, Errors.Select(failure => failure.ErrorMessage));

    /// <summary>
    /// Converts the ValidationResult's errors collection into a simple dictionary representation.
    /// </summary>
    /// <returns>A dictionary keyed by property name
    /// where each value is an array of error messages associated with that property.
    /// </returns>
    public IDictionary<string, string[]> ToDictionary()
        => Errors.GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
            );

    #region Equality

    public bool Equals(ValidationResult? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Errors.Equals(other.Errors);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ValidationResult)obj);
    }

    public override int GetHashCode() => Errors.GetHashCode();

    public static bool operator ==(ValidationResult? lhs, ValidationResult? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(ValidationResult? lhs, ValidationResult? rhs) => !(lhs == rhs);

    #endregion Equality

    private string DebuggerDisplay
    {
        // TODO: Make this an expandable list?
        get
        {
            return Errors.Count switch
            {
                0 => "SUCCESS",
                1 => $"FAILURE: {Errors.First().ErrorMessage}",
                _ => $"FAILURES: {Errors.Count}"
            };
        }
    }
}
