using FoundationKit.Domain.Persistence;
using FoundationKit.Web.Example.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FoundationKit.Web.Example.Domain.Persistence;

public class ApplicationDbContext : FoundationKitDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Person> Persons { get; set; }
}