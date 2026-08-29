namespace Auturge.Validation;

public static class ValidationResultExtensions
{
    public static ValidationResult Collate(this IEnumerable<ValidationResult> results) => new(results);

    public static void AddIfFailure(this List<ValidationResult> results, ValidationResult validationResult)
    {
        if (validationResult != ValidationResult.Success)
        {
            results.Add(validationResult);
        }
    }
}
