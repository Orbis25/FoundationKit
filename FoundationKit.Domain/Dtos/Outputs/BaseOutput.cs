namespace FoundationKit.Domain.Dtos.Outputs;

public class BaseOutput
{
    public virtual Guid Id { get; set; }
    public virtual string? CreatedAtStr { get; set; }
    public virtual DateTime CreatedAt { get; set; }
    public virtual string? CreatedBy { get; set; }
}
