namespace FoundationKit.Domain.Dtos.Paginations;

public class Paginate
{
    /// <summary>
    /// Actual page
    /// </summary>
    [FromQuery]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Quantity by page
    /// </summary>
    [FromQuery]
    public int Qyt { get; set; } = 10;

    /// <summary>
    /// flag order decs or asc
    /// </summary>
    /// 
    [FromQuery]
    public bool OrderByDesc { get; set; }

    /// <summary>
    /// indicate if the results is paginated
    /// </summary>

    [FromQuery]
    public bool NoPaginate { get; set; }

    /// <summary>
    /// Query for search you need implement this in your code
    /// </summary>
    [FromQuery]
    public string? Query { get; set; }
}
