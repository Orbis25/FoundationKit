namespace FoundationKit.Domain.Persistence;

public abstract class FoundationKitIdentityDbContext<TUser> :
    IdentityDbContext<TUser> where TUser : IdentityUser
{
    protected FoundationKitIdentityDbContext(DbContextOptions options) : base(options)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entity in ChangeTracker.Entries<BaseModel>())
        {
            switch (entity.State)
            {
                case EntityState.Modified:
                    entity.Entity.UpdatedAt = DateTime.Now;
                    break;
                case EntityState.Added:
                    entity.Entity.CreatedAt = DateTime.Now;
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
                    entity.Entity.UpdatedAt = DateTime.Now;
                    break;
                case EntityState.Added:
                    entity.Entity.CreatedAt = DateTime.Now;
                    break;
            }
        }

        return base.SaveChanges();
    }
}