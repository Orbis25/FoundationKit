namespace FoundationKit.Domain.Models;

public abstract class BaseBasicModel : BaseModel
{
    [NotMapped]
    public override string? CreatedBy { get; set; }

    [NotMapped]
    public override string? UpdatedBy { get; set; }
}
