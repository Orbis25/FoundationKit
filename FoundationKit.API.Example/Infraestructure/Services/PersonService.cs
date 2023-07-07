namespace FoundationKit.API.Example.Infraestructure.Services;

public class PersonService : BaseRepository<ApplicationDbContext, Person>, IPersonService
{
    public PersonService(ApplicationDbContext context) : base(context)
    {
    }
}