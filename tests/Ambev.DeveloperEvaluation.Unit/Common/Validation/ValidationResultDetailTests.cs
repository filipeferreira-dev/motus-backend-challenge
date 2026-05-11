using Ambev.DeveloperEvaluation.Common.Validation;
using FluentAssertions;
using FluentValidation.Results;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common.Validation;

public class ValidationResultDetailTests
{
    [Fact(DisplayName = "Default constructor leaves IsValid=false and empty Errors")]
    public void DefaultCtor_Defaults()
    {
        var detail = new ValidationResultDetail();

        detail.IsValid.Should().BeFalse();
        detail.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Copies IsValid and projects Errors from a FluentValidation result")]
    public void FromValidationResult_CopiesFields()
    {
        var fv = new ValidationResult(new[]
        {
            new ValidationFailure("Name", "is required"),
            new ValidationFailure("Email", "must be a valid email"),
        });

        var detail = new ValidationResultDetail(fv);

        detail.IsValid.Should().Be(fv.IsValid);
        detail.Errors.Should().HaveCount(2);
        detail.Errors.Should().Contain(e => e.Error == "is required" || e.Detail == "is required");
    }

    [Fact(DisplayName = "Wraps a valid FluentValidation result")]
    public void FromValidValidationResult_IsValidTrue()
    {
        var fv = new ValidationResult();

        var detail = new ValidationResultDetail(fv);

        detail.IsValid.Should().BeTrue();
        detail.Errors.Should().BeEmpty();
    }
}
