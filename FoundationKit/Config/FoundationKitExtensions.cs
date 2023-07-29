using FoundationKit.Domain.Persistence;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FoundationKit.Extensions;

public static class FoundationKitExtensions
{
    /// <summary>
    /// Add basic configurations for work with the library
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddFoundationKit(this IServiceCollection services, Assembly assembly)
    {
        services.AddAutoMapper(assembly);
        return services;
    }

    /// <summary>
    /// Add configuration with identity and automapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddFoundationKitIdentity<T, TDbContext>(
        this IServiceCollection services,
        Assembly assembly)
        where T : IdentityUser
        where TDbContext : IdentityDbContext<T>
    {
        services.AddAutoMapper(assembly);
        services.AddIdentityCore<T>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<TDbContext>();

        return services;
    }
}
