using System.Text.Json.Serialization;

namespace FoundationKit.Domain.Dtos.Inputs;

public abstract class BaseEdit
{
    public virtual Guid Id { get; set; }
    [JsonIgnore]
    public virtual DateTime? UpdateAt { get; set; }
    [JsonIgnore]
    public virtual string? UpdatedBy { get; set; }
    [JsonIgnore]
    public virtual DateTime CreatedAt { get; set; }
    [JsonIgnore]
    public virtual string? CreatedBy { get; set; }
}
