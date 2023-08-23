namespace FoundationKit.Domain.Config;
public interface IDbContextSchema
{
    string Schema { get; }
}

#pragma warning disable EF1001 // Internal EF Core API usage.
public class MigrationDbContextConfigAssembly : MigrationsAssembly
#pragma warning restore EF1001 // Internal EF Core API usage.
{
    private readonly DbContext _dbContext;
    public MigrationDbContextConfigAssembly(ICurrentDbContext currentContext,
        IDbContextOptions options,
        IMigrationsIdGenerator idGenerator,
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
#pragma warning disable EF1001 // Internal EF Core API usage.
        : base(currentContext, options, idGenerator, logger)
#pragma warning restore EF1001 // Internal EF Core API usage.
    {
        _dbContext = currentContext.Context;
    }

    public override Migration CreateMigration(TypeInfo migrationClass,
        string activeProvider)
    {
        if (activeProvider == null)
            throw new ArgumentNullException(nameof(activeProvider));

        var hasCtorWithSchema = migrationClass
                .GetConstructor(new[] { typeof(IDbContextSchema) }) != null;

        if (hasCtorWithSchema && _dbContext is IDbContextSchema schema && migrationClass != null)
        {
            var activator = Activator.CreateInstance(migrationClass.AsType(), schema);

            if (activator == null)
                throw new InvalidOperationException("invalid migration creation with : MigrationDbContextConfigAssembly");

            var instance = (Migration)activator;
            instance.ActiveProvider = activeProvider;

            return instance;
        }

#pragma warning disable CS8604 // Possible null reference argument.
        return CreateMigration(migrationClass, activeProvider);
#pragma warning restore CS8604 // Possible null reference argument.
    }
}

