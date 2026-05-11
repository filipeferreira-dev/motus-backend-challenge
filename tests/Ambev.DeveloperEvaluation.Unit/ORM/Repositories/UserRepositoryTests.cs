using Ambev.DeveloperEvaluation.ORM.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Ambev.DeveloperEvaluation.Unit.ORM.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM.Repositories;

public class UserRepositoryTests
{
    [Fact(DisplayName = "CreateAsync persists a user that can be reloaded from a new context")]
    public async Task CreateAsync_PersistsUser()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new UserRepository(fixture.Context);
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();

        await repo.CreateAsync(user);

        using var verify = fixture.NewContext();
        var loaded = await verify.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        loaded.Should().NotBeNull();
        loaded!.Email.Should().Be(user.Email);
    }

    [Fact(DisplayName = "GetByIdAsync returns the user when present and null when missing")]
    public async Task GetByIdAsync_Existing_ReturnsUser_Missing_ReturnsNull()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new UserRepository(fixture.Context);
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();
        await repo.CreateAsync(user);

        (await repo.GetByIdAsync(user.Id)).Should().NotBeNull();
        (await repo.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact(DisplayName = "GetByEmailAsync returns the user when present and null when missing")]
    public async Task GetByEmailAsync_Existing_ReturnsUser_Missing_ReturnsNull()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new UserRepository(fixture.Context);
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();
        await repo.CreateAsync(user);

        (await repo.GetByEmailAsync(user.Email))!.Id.Should().Be(user.Id);
        (await repo.GetByEmailAsync("not-a-real-email@example.com")).Should().BeNull();
    }

    [Fact(DisplayName = "DeleteAsync removes an existing user and returns true; false for missing")]
    public async Task DeleteAsync_Existing_Removes_Missing_ReturnsFalse()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new UserRepository(fixture.Context);
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();
        await repo.CreateAsync(user);

        var deleted = await repo.DeleteAsync(user.Id);
        deleted.Should().BeTrue();

        using var verify = fixture.NewContext();
        (await verify.Users.AnyAsync(u => u.Id == user.Id)).Should().BeFalse();

        (await repo.DeleteAsync(Guid.NewGuid())).Should().BeFalse();
    }
}
