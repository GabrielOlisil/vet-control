using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public sealed class NonEmptyGuidAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        return value is Guid guid && guid != Guid.Empty;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must not be empty.";
    }
}

public sealed class PastOrPresentDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        // Optional dates may be omitted in PATCH. Required/non-nullable fields
        // remain responsible for presence; a supplied default date is still invalid.
        if (value is null) return true;
        var maxDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        return value is DateOnly date && date != default && date <= maxDate;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be a valid date that is not in the future.";
    }
}
