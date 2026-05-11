using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Users;

public class GetUserHandlerTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly GetUserHandler _handler;

    public GetUserHandlerTests()
    {
        _handler = new GetUserHandler(_repo, _mapper);
    }

    [Fact(DisplayName = "Existing user returns mapped result")]
    public async Task Handle_Existing_ReturnsResult()
    {
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();
        _repo.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<GetUserResult>(user).Returns(new GetUserResult { Id = user.Id, Email = user.Email });

        var result = await _handler.Handle(new GetUserCommand(user.Id), CancellationToken.None);

        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
    }

    [Fact(DisplayName = "Missing user throws KeyNotFoundException")]
    public async Task Handle_Missing_Throws()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(new GetUserCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Empty Guid fails validation")]
    public async Task Handle_EmptyId_Throws()
    {
        var act = () => _handler.Handle(new GetUserCommand(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        await _repo.DidNotReceiveWithAnyArgs().GetByIdAsync(default, default);
    }
}
