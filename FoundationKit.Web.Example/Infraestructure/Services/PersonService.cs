using FoundationKit.Repository.Services;
using FoundationKit.Web.Example.Application.interfaces;
using FoundationKit.Web.Example.Domain.Models;
using FoundationKit.Web.Example.Domain.Persistence;

namespace FoundationKit.Web.Example.Infraestructure.Services;

public class PersonService : BaseRepository<ApplicationDbContext, Person>, IPersonService
{
    public PersonService(ApplicationDbContext context) : base(context)
    {
    }
}