using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Mapping;

public class ApplicationMappingProfilesTests
{
    private static IMapper BuildMapper(params Profile[] profiles)
    {
        var config = new MapperConfiguration(c =>
        {
            foreach (var profile in profiles)
                c.AddProfile(profile);
        });
        return config.CreateMapper();
    }

    [Fact(DisplayName = "Sale -> CreateSaleResult maps scalars and items")]
    public void Map_Sale_To_CreateSaleResult_PopulatesAllMembers()
    {
        var mapper = BuildMapper(new CreateSaleProfile());
        var sale = SaleTestData.GenerateValidSale(itemCount: 2);

        var result = mapper.Map<CreateSaleResult>(sale);

        result.Id.Should().Be(sale.Id);
        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.SaleDate.Should().Be(sale.SaleDate);
        result.CustomerId.Should().Be(sale.CustomerId);
        result.CustomerName.Should().Be(sale.CustomerName);
        result.BranchId.Should().Be(sale.BranchId);
        result.BranchName.Should().Be(sale.BranchName);
        result.Status.Should().Be(sale.Status);
        result.TotalAmount.Should().Be(sale.TotalAmount);
        result.Items.Should().HaveCount(2);

        var firstSource = sale.Items.First();
        var firstMapped = result.Items[0];
        firstMapped.Id.Should().Be(firstSource.Id);
        firstMapped.ProductId.Should().Be(firstSource.ProductId);
        firstMapped.ProductName.Should().Be(firstSource.ProductName);
        firstMapped.Quantity.Should().Be(firstSource.Quantity);
        firstMapped.UnitPrice.Should().Be(firstSource.UnitPrice);
        firstMapped.DiscountPercent.Should().Be(firstSource.DiscountPercent);
        firstMapped.LineDiscount.Should().Be(firstSource.LineDiscount);
        firstMapped.LineTotal.Should().Be(firstSource.LineTotal);
        firstMapped.IsCancelled.Should().Be(firstSource.IsCancelled);
    }

    [Fact(DisplayName = "Sale -> GetSaleResult maps audit fields and items")]
    public void Map_Sale_To_GetSaleResult_PopulatesAllMembers()
    {
        var mapper = BuildMapper(new GetSaleProfile());
        var sale = SaleTestData.GenerateValidSale(itemCount: 3);

        var result = mapper.Map<GetSaleResult>(sale);

        result.Id.Should().Be(sale.Id);
        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.SaleDate.Should().Be(sale.SaleDate);
        result.CustomerId.Should().Be(sale.CustomerId);
        result.CustomerName.Should().Be(sale.CustomerName);
        result.BranchId.Should().Be(sale.BranchId);
        result.BranchName.Should().Be(sale.BranchName);
        result.Status.Should().Be(sale.Status);
        result.TotalAmount.Should().Be(sale.TotalAmount);
        result.CreatedAt.Should().Be(sale.CreatedAt);
        result.UpdatedAt.Should().Be(sale.UpdatedAt);
        result.Items.Should().HaveCount(3);
    }

    [Fact(DisplayName = "Guid -> GetSaleCommand uses ConstructUsing")]
    public void Map_Guid_To_GetSaleCommand_ConstructsCommand()
    {
        var mapper = BuildMapper(new GetSaleProfile());
        var id = Guid.NewGuid();

        var command = mapper.Map<GetSaleCommand>(id);

        command.Id.Should().Be(id);
    }

    [Fact(DisplayName = "Sale -> SaleSummaryDto computes ItemCount from Items collection")]
    public void Map_Sale_To_SaleSummaryDto_PopulatesItemCount()
    {
        var mapper = BuildMapper(new ListSalesProfile());
        var sale = SaleTestData.GenerateValidSale(itemCount: 4);

        var summary = mapper.Map<SaleSummaryDto>(sale);

        summary.Id.Should().Be(sale.Id);
        summary.SaleNumber.Should().Be(sale.SaleNumber);
        summary.SaleDate.Should().Be(sale.SaleDate);
        summary.CustomerId.Should().Be(sale.CustomerId);
        summary.CustomerName.Should().Be(sale.CustomerName);
        summary.BranchId.Should().Be(sale.BranchId);
        summary.BranchName.Should().Be(sale.BranchName);
        summary.Status.Should().Be(sale.Status);
        summary.TotalAmount.Should().Be(sale.TotalAmount);
        summary.ItemCount.Should().Be(4);
    }

    [Fact(DisplayName = "SaleItem -> SaleItemDto maps every property")]
    public void Map_SaleItem_To_SaleItemDto_PopulatesAllMembers()
    {
        var mapper = BuildMapper(new GetSaleProfile());
        var sale = SaleTestData.GenerateValidSale(itemCount: 1);
        var saleItem = sale.Items.Single();

        var dto = mapper.Map<SaleItemDto>(saleItem);

        dto.Id.Should().Be(saleItem.Id);
        dto.ProductId.Should().Be(saleItem.ProductId);
        dto.ProductName.Should().Be(saleItem.ProductName);
        dto.Quantity.Should().Be(saleItem.Quantity);
        dto.UnitPrice.Should().Be(saleItem.UnitPrice);
        dto.DiscountPercent.Should().Be(saleItem.DiscountPercent);
        dto.LineDiscount.Should().Be(saleItem.LineDiscount);
        dto.LineTotal.Should().Be(saleItem.LineTotal);
        dto.IsCancelled.Should().Be(saleItem.IsCancelled);
    }

    [Fact(DisplayName = "User -> CreateUserResult maps Id")]
    public void Map_User_To_CreateUserResult_PopulatesId()
    {
        var mapper = BuildMapper(new CreateUserProfile());
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();

        var result = mapper.Map<CreateUserResult>(user);

        result.Id.Should().Be(user.Id);
    }

    [Fact(DisplayName = "CreateUserCommand -> User maps scalars")]
    public void Map_CreateUserCommand_To_User_PopulatesScalars()
    {
        var mapper = BuildMapper(new CreateUserProfile());
        var command = new CreateUserCommand
        {
            Username = "demo-user",
            Password = "Pwd@1234",
            Email = "demo@example.com",
            Phone = "+5511999999999",
            Status = Ambev.DeveloperEvaluation.Domain.Enums.UserStatus.Active,
            Role = Ambev.DeveloperEvaluation.Domain.Enums.UserRole.Customer
        };

        var user = mapper.Map<User>(command);

        user.Username.Should().Be(command.Username);
        user.Email.Should().Be(command.Email);
        user.Phone.Should().Be(command.Phone);
        user.Status.Should().Be(command.Status);
        user.Role.Should().Be(command.Role);
    }

    [Fact(DisplayName = "User -> GetUserResult populates the convention-mapped members")]
    public void Map_User_To_GetUserResult_PopulatesConventionMembers()
    {
        var mapper = BuildMapper(new GetUserProfile());
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();

        var result = mapper.Map<GetUserResult>(user);

        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.Phone.Should().Be(user.Phone);
        result.Role.Should().Be(user.Role);
        result.Status.Should().Be(user.Status);
    }

    [Fact(DisplayName = "User -> AuthenticateUserResult ignores Token and stringifies Role")]
    public void Map_User_To_AuthenticateUserResult_AppliesProfileRules()
    {
        var mapper = BuildMapper(new AuthenticateUserProfile());
        var user = UserTestData.GenerateValidUser();
        user.Id = Guid.NewGuid();

        var result = mapper.Map<AuthenticateUserResult>(user);

        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.Phone.Should().Be(user.Phone);
        result.Role.Should().Be(user.Role.ToString());
        result.Token.Should().BeEmpty();
    }
}
