namespace FoundationKit.Domain.Config;

public abstract class EFCoreConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
       where TEntity : BaseModel
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);
        ConfigureEF(builder);
    }

    public abstract void ConfigureEF(EntityTypeBuilder<TEntity> builder);
}
