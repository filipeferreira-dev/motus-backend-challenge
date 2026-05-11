using Ambev.DeveloperEvaluation.Common.Validation;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common.Validation;

public class ValidationBehaviorTests
{
    public record DummyRequest(string Value) : IRequest<string>;

    public class DummyValidator : AbstractValidator<DummyRequest>
    {
        public DummyValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }

    [Fact(DisplayName = "No validators registered: next() runs and response is returned")]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<DummyRequest, string>(Array.Empty<IValidator<DummyRequest>>());
        var nextCalled = false;

        Task<string> Next() { nextCalled = true; return Task.FromResult("ok"); }

        var result = await behavior.Handle(new DummyRequest("x"), Next, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be("ok");
    }

    [Fact(DisplayName = "Validators pass: next() runs and response is returned")]
    public async Task Handle_ValidatorsPass_CallsNext()
    {
        var behavior = new ValidationBehavior<DummyRequest, string>(new IValidator<DummyRequest>[] { new DummyValidator() });
        var nextCalled = false;

        Task<string> Next() { nextCalled = true; return Task.FromResult("ok"); }

        var result = await behavior.Handle(new DummyRequest("non-empty"), Next, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be("ok");
    }

    [Fact(DisplayName = "Validator fails: throws ValidationException and skips next()")]
    public async Task Handle_ValidatorFails_ThrowsAndSkipsNext()
    {
        var behavior = new ValidationBehavior<DummyRequest, string>(new IValidator<DummyRequest>[] { new DummyValidator() });
        var nextCalled = false;

        Task<string> Next() { nextCalled = true; return Task.FromResult("ok"); }

        var act = () => behavior.Handle(new DummyRequest(""), Next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        nextCalled.Should().BeFalse();
    }
}
