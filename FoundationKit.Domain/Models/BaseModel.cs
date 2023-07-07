namespace FoundationKit.Domain.Models;
public abstract class BaseModel
{
    public virtual Guid Id { get; set; }

    public virtual DateTime CreatedAt { get; set; }

    public virtual DateTime? UpdateAt { get; set; }

    [NotMapped]
    public string CreatedAtStr => CreatedAt.ToString("dd/MM/yyyy hh:mm:ss");

    [NotMapped]
    public string UpdateAtStr => CreatedAt.ToString("dd/MM/yyyy hh:mm:ss");

    public virtual bool IsDeleted { get; set; }

    public virtual string? CreatedBy { get; set; }

    public virtual string? UpdatedBy { get; set; }
}