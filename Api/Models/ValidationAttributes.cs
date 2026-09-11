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
        var maxDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        return value is DateOnly date && date != default && date <= maxDate;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be a valid date that is not in the future.";
    }
}