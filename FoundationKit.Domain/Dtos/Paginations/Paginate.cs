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
    /// Order by
    /// </summary>
    [FromQuery]
    public string? OrderBy { get; set; } = null;

    /// <summary>
    /// flag order decs or asc
    /// </summary>
    /// 
    [FromQuery]
    public bool OrderByDesc { get; set; }

    /// <summary>
    /// param to filter
    /// </summary>
    [FromQuery]
    public string? Query { get; set; }
}
