using FoundationKit.API.Example.Domain.Mappings;
using FoundationKit.Core.Controllers;

namespace FoundationKit.API.Example.Controllers;

public class PeopleController : ApiMapController<IPersonMapService, PersonDto, PersonInput, PersonEdit>
{
    public PeopleController(IPersonMapService service) : base(service)
    {
    }
}
