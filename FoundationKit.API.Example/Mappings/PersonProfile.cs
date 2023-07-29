using AutoMapper;
using FoundationKit.API.Example.Domain.Mappings;

namespace FoundationKit.API.Example.Mappings;

public class PersonProfile : Profile
{
    public PersonProfile()
    {
        CreateMap<Person, PersonDto>().ReverseMap();
        CreateMap<Person, PersonInput>().ReverseMap();
        CreateMap<Person, PersonEdit>().ReverseMap();
    }
}
