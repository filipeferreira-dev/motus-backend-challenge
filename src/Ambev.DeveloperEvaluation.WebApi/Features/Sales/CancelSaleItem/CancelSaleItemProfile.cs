using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;

public class CancelSaleItemProfile : Profile
{
    public CancelSaleItemProfile()
    {
        CreateMap<(Guid SaleId, Guid ItemId), CancelSaleItemCommand>()
            .ConstructUsing(t => new CancelSaleItemCommand(t.SaleId, t.ItemId));
    }
}
