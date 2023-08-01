namespace FoundationKit.Domain.Models;
public abstract class BaseModel
{
    public virtual Guid Id { get; set; }

    public virtual DateTime CreatedAt { get; set; }
    /// <summary>
    /// If you after used updateAt please override this prop and add anotation [Colunm("UpdateAt")]
    /// </summary>
    public virtual DateTime? UpdatedAt { get; set; }

    [NotMapped]
    public string CreatedAtStr => CreatedAt.ToString("dd/MM/yyyy hh:mm:ss");

    [NotMapped]
    public string UpdateAtStr => UpdatedAt != null ? UpdatedAt.Value.ToString("dd/MM/yyyy hh:mm:ss") : "";

    public virtual bool IsDeleted { get; set; }

    public virtual string? CreatedBy { get; set; }

    public virtual string? UpdatedBy { get; set; }
}