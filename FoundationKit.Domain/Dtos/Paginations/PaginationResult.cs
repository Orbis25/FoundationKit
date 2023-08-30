namespace FoundationKit.Domain.Dtos.Paginations;
public class PaginationResult<T>
{
    public int ActualPage { get; set; } = 1;
    public int Qyt { get; set; }
    public int PageTotal { get; set; }
    public int Total { get; set; }
    public string? Query { get; set; }
    public virtual IReadOnlyCollection<T>? Results { get; set; }
}