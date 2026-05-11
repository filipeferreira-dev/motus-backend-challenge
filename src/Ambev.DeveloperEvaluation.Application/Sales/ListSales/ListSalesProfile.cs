using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesProfile : Profile
{
    public ListSalesProfile()
    {
        CreateMap<Sale, SaleSummaryDto>()
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count));
    }
}
