using AutoMapper;
using FoundationKit.API.Example.Domain.Mappings;

namespace FoundationKit.API.Example.Infraestructure.Services;

public class PersonService : BaseRepository<ApplicationDbContext, Person>, IPersonService
{
    public PersonService(ApplicationDbContext context) : base(context)
    {
    }
}


public class PersonMapService
    : MapRepository<ApplicationIdentityDbContext, Person, PersonInput, PersonEdit, PersonDto>,
    IPersonMapService
{
    public PersonMapService(ApplicationIdentityDbContext context, IMapper mapper) : base(context, mapper)
    {
    }
}