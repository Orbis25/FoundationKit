namespace Foundationkit.Extensions.Enumerations;

public static class QueryableExtensions
{
    public static async Task<PaginationResult<TSource?>> PaginateAsync<TSource>(this IQueryable<TSource> items, 
        Paginate paginate, 
        CancellationToken cancellationToken = default)
    {
        if (paginate.NoPaginate)
        {
            return new()
            {
                Results = await items.ToListAsync(cancellationToken),
            };
        }

        var total = items.Count();
        var pages = (int)Math.Ceiling((decimal)total / paginate.Qyt);

        items = items.Skip((paginate.Page - 1) * paginate.Qyt).Take(paginate.Qyt);

        return new()
        {
            ActualPage = paginate.Page,
            Qyt = paginate.Qyt,
            PageTotal = pages,
            Total = total,
            Results = await items.ToListAsync(cancellationToken)
        };
    }

    public static  PaginationResult<TSource?> Paginate<TSource>(this IQueryable<TSource> items,
       Paginate paginate)
    {
        if (paginate.NoPaginate)
        {
            return new()
            {
                Results = items.ToList()
            };
        }

        var total = items.Count();
        var pages = (int)Math.Ceiling((decimal)total / paginate.Qyt);

        items = items.Skip((paginate.Page - 1) * paginate.Qyt).Take(paginate.Qyt);

        return new()
        {
            ActualPage = paginate.Page,
            Qyt = paginate.Qyt,
            PageTotal = pages,
            Total = total,
            Results = items.ToList()
        };
    }
}
