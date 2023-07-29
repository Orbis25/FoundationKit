using FoundationKit.API.Example.Domain.Mappings;

namespace FoundationKit.API.Example.Application.interfaces;
public interface IPersonService : IBaseRepository<Person>
{
}

public interface IPersonMapService : IMapRepository<PersonInput, PersonEdit, PersonDto>
{
}