namespace FoundationKit.API.Example.Domain.Persistence;

public class ApplicationDbContext : FoundationKitDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Person> Persons { get; set; }
}