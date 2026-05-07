using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using AutoMapper;
using AppCommon = Ambev.DeveloperEvaluation.Application.Sales.Common;
using ApiCommon = Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

public class UpdateSaleProfile : Profile
{
    public UpdateSaleProfile()
    {
        CreateMap<UpdateSaleRequest, UpdateSaleCommand>();
        CreateMap<ApiCommon.SaleItemRequestDto, AppCommon.SaleItemInput>();
    }
}
