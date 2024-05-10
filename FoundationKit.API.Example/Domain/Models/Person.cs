namespace FoundationKit.API.Example.Domain.Models;

public class Person : BaseModel
{
    public string? Name { get; set; }

    [NotMapped]
    public override string? CreatedBy { get; set; }
}