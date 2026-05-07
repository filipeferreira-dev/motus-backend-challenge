using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using AutoMapper;
using AppCommon = Ambev.DeveloperEvaluation.Application.Sales.Common;
using ApiCommon = Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

public class CreateSaleProfile : Profile
{
    public CreateSaleProfile()
    {
        CreateMap<CreateSaleRequest, CreateSaleCommand>();
        CreateMap<ApiCommon.SaleItemRequestDto, AppCommon.SaleItemInput>();
        CreateMap<CreateSaleResult, CreateSaleResponse>();
        CreateMap<AppCommon.SaleItemDto, ApiCommon.SaleItemDto>();
    }
}
