extern alias VetApi;
using Xunit;
using PastOrPresentDateAttribute = VetApi::Api.Models.PastOrPresentDateAttribute;

namespace Api.Tests;

public sealed class DateValidationTests
{
    [Fact]
    public void OptionalGuidMayBeOmittedButEmptyOrMalformedGuidsAreRejected()
    {
        var validator = new VetApi::Api.Models.NonEmptyGuidAttribute();
        Assert.True(validator.IsValid(null));
        Assert.False(validator.IsValid(Guid.Empty));
        Assert.False(validator.IsValid("invalid-guid"));
        Assert.True(validator.IsValid(Guid.NewGuid()));
    }
    [Fact]
    public void OptionalDateMayBeOmittedInPartialUpdates() =>
        Assert.True(new PastOrPresentDateAttribute().IsValid(null));

    [Fact]
    public void InvalidRequiredDatesAndWrongTypesAreStillRejected()
    {
        var validation = new PastOrPresentDateAttribute();
        Assert.False(validation.IsValid(default(DateOnly)));
        Assert.False(validation.IsValid(DateOnly.MaxValue));
        Assert.False(validation.IsValid("not-a-date"));
        Assert.True(validation.IsValid(new DateOnly(2000, 1, 1)));
    }
}
