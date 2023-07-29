namespace FoundationKit.API.Example.Domain.Persistence;

public class ApplicationDbContext : FoundationKitDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Person> Persons { get; set; }
}

public class ApplicationIdentityDbContext : FoundationKitIdentityDbContext<User>
{
    public ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options) : base(options)
    {
    }
    public DbSet<Person> Persons { get; set; }
}