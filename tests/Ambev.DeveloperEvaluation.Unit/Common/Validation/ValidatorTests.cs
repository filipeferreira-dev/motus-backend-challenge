using Ambev.DeveloperEvaluation.Common.Validation;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common.Validation;

public class ValidatorTests
{
    private class Unvalidated
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact(DisplayName = "Calling the static helper throws because it tries to instantiate IValidator<T> directly")]
    public async Task ValidateAsync_ThrowsBecauseActivatorCannotInstantiateInterface()
    {
        var act = () => Validator.ValidateAsync(new Unvalidated { Name = "x" });

        await act.Should().ThrowAsync<MissingMethodException>();
    }
}
