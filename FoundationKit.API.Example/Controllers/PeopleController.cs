using FoundationKit.Core;

namespace FoundationKit.API.Example.Controllers;

public class PeopleController : ApiCoreController<Person, IPersonService>
{
    public PeopleController(IPersonService service) : base(service)
    {
    }
}
