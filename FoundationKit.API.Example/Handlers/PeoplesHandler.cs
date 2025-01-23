using FoundationKit.API.Handlers.EndpointRouteHandler;

namespace FoundationKit.API.Example.Handlers;

public class PeoplesHandler : IEndpointRouteHandler
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/people", GetAll)
            .WithTags("People")
            .WithName("get all peoples")
            .Produces(404)
            .ProducesValidationProblem();
    }

    private IResult GetAll()
    {
        return TypedResults.Ok("people here");
    }
}