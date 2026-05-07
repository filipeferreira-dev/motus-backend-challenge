namespace Ambev.DeveloperEvaluation.Domain.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int Size { get; }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int size)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        Size = size;
    }
}
