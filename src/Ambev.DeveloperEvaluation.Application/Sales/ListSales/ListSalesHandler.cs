using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesCommand, ListSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<ListSalesResult> Handle(ListSalesCommand request, CancellationToken cancellationToken)
    {
        var validator = new ListSalesValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var filters = new SaleListFilters
        {
            Page = request.Page,
            Size = request.Size,
            Order = request.Order,
            CustomerId = request.CustomerId,
            BranchId = request.BranchId,
            Status = request.Status,
            SaleNumber = request.SaleNumber,
            MinSaleDate = request.MinSaleDate,
            MaxSaleDate = request.MaxSaleDate,
            MinTotalAmount = request.MinTotalAmount,
            MaxTotalAmount = request.MaxTotalAmount
        };

        var page = await _saleRepository.ListAsync(filters, cancellationToken);

        return new ListSalesResult
        {
            Items = page.Items.Select(_mapper.Map<SaleSummaryDto>).ToList(),
            TotalCount = page.TotalCount,
            Page = page.Page,
            Size = page.Size
        };
    }
}
