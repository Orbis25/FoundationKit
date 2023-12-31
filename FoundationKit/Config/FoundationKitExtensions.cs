﻿namespace FoundationKit.Extensions;

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
    public static IServiceCollection AddFoundationKitIdentityWithMapper<T, TDbContext>(
        this IServiceCollection services,
        Assembly assembly)
        where T : IdentityUser
        where TDbContext : IdentityDbContext<T>
    {
        services.AddAutoMapper(assembly);

        services.AddIdentity<T, IdentityRole>()
               .AddRoles<IdentityRole>()
               .AddEntityFrameworkStores<TDbContext>()
               .AddSignInManager<SignInManager<T>>()
               .AddDefaultTokenProviders();

        return services;
    }

    /// Add configuration with identity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddFoundationKitIdentity<T, TDbContext>(
        this IServiceCollection services)
        where T : IdentityUser
        where TDbContext : IdentityDbContext<T>
    {
        services.AddIdentity<T, IdentityRole>()
               .AddRoles<IdentityRole>()
               .AddEntityFrameworkStores<TDbContext>()
               .AddSignInManager<SignInManager<T>>()
               .AddDefaultTokenProviders();

        return services;
    }


    /// <summary>
    /// Add RsaEncryptor config
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options">options with privateKey and publicKey</param>
    /// <returns></returns>
    public static IServiceCollection AddFoundationKitEncryptor(this IServiceCollection services, EncryptorOption options,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.TryAddSingleton(provider => options);
                break;
            case ServiceLifetime.Transient:
                services.TryAddTransient(provider => options);
                break;
            default:
                services.TryAddScoped(provider => options);
                break;
        }


        services.AddScoped<IEncryptorService, EncryptorService>();

        return services;
    }
}
