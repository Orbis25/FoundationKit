using FoundationKit.Domain.Option;

namespace FoundationKit.Domain.Persistence;

public abstract class FoundationKitIdentityDbContext<TUser>(DbContextOptions options) :
    IdentityDbContext<TUser>(options)
    where TUser : IdentityUser
{
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entity in ChangeTracker.Entries<BaseModel>())
        {
            switch (entity.State)
            {
                case EntityState.Modified:
                    entity.Entity.UpdatedAt = FoundationKitStaticOptions.DateUtc ? DateTime.UtcNow : DateTime.Now;
                    break;
                case EntityState.Added:
                    entity.Entity.CreatedAt =  FoundationKitStaticOptions.DateUtc ? DateTime.UtcNow : DateTime.Now;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        foreach (var entity in ChangeTracker.Entries<BaseModel>())
        {
            switch (entity.State)
            {
                case EntityState.Modified:
                    entity.Entity.UpdatedAt = FoundationKitStaticOptions.DateUtc ? DateTime.UtcNow : DateTime.Now;
                    break;
                case EntityState.Added:
                    entity.Entity.CreatedAt = FoundationKitStaticOptions.DateUtc ? DateTime.UtcNow : DateTime.Now;
                    break;
            }
        }

        return base.SaveChanges();
    }
}