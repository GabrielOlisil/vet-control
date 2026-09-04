using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public sealed class NonEmptyGuidAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
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
        return value is DateOnly date && date != default && date <= DateOnly.FromDateTime(DateTime.Today);
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be a valid date that is not in the future.";
    }
}