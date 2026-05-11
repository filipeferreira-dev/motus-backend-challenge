using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using AutoMapper;
using AppCommon = Ambev.DeveloperEvaluation.Application.Sales.Common;
using ApiCommon = Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;

public class GetSaleProfile : Profile
{
    public GetSaleProfile()
    {
        CreateMap<Guid, GetSaleCommand>().ConstructUsing(id => new GetSaleCommand(id));
        CreateMap<GetSaleResult, GetSaleResponse>();
        CreateMap<AppCommon.SaleItemDto, ApiCommon.SaleItemDto>();
    }
}
