using System.Reflection;
using FoundationKit.API.Handlers.Extensions;

namespace FoundationKit.API.Extensions;

public static class FoundationKitApiExtensions
{
    public static WebApplication UseFoundationKitApiHandlers(this WebApplication app, Assembly assembly)
    {
        app.MapEndpoints(assembly);
        
        return app;
    }
}