using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleProfile : Profile
{
    public GetSaleProfile()
    {
        CreateMap<Guid, GetSaleCommand>().ConstructUsing(id => new GetSaleCommand(id));
        CreateMap<Sale, GetSaleResult>();
        CreateMap<SaleItem, SaleItemDto>();
    }
}
