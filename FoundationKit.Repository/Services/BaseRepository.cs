namespace FoundationKit.Repository.Services;

/// <summary>
/// Permit create the base logic of services
/// </summary>
/// <typeparam name="TContext">The class represent a dbcontext</typeparam>
/// <typeparam name="TEntity">The entity class</typeparam>
public abstract class BaseRepository<TContext, TModel> : IBaseRepository<TModel>
    where TContext : DbContext
    where TModel : BaseModel
{
    private readonly TContext _context;
    protected BaseRepository(TContext context)
    {
        _context = context;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new DbUpdateException(ex.GetBaseException().Message);
        }
    }

    public async Task<string?> CommitAndResultAsync(CancellationToken cancellationToken = default)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return default;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return (ex.GetBaseException().Message);
        }
    }

    public virtual async Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken = default)
    {
        _context.Set<TModel>().Add(model);

        await CommitAsync(cancellationToken);

        return model;
    }

    public virtual IQueryable<TModel> GetAll(Expression<Func<TModel, bool>>? expression = default,
        bool orderDesc = true, Expression<Func<TModel, object>>? ordered = default
        , params Expression<Func<TModel, object>>[] includes)
    {
        var results = _context.Set<TModel>().AsQueryable();
        if (expression != null)
            results = results.Where(expression);

        foreach (var include in includes) results = results.Include(include);

        ///Order elements desc or asc
        if (ordered != null && orderDesc) results = results.OrderByDescending(ordered);

        else if (!orderDesc && ordered != null) results = results.OrderBy(ordered);

        else if (orderDesc) results = results.OrderByDescending(x => x.CreatedAt);

        else results = results.OrderBy(x => x.CreatedAt);

        return results;
    }

    public virtual async Task<PaginationResult<TModel>> GetPaginatedListAsync(Paginate paginate,
        Expression<Func<TModel, bool>>? expression = default,
        Expression<Func<TModel, object>>? ordered = default,
        CancellationToken cancellationToken = default,
        params Expression<Func<TModel, object>>[] includes)
    {
        var results = GetAll(expression, paginate.OrderByDesc, ordered, includes);

        if (paginate.NoPaginate)
        {
            return new()
            {
                Results = await results.AsNoTracking().ToListAsync(cancellationToken)
            };
        }

        var total = results.Count();
        var pages = (int)Math.Ceiling((decimal)total / paginate.Qyt);

        results = results.Skip((paginate.Page - 1) * paginate.Qyt).Take(paginate.Qyt);

        return new PaginationResult<TModel>
        {
            ActualPage = paginate.Page,
            Qyt = paginate.Qyt,
            PageTotal = pages,
            Total = total,
            Results = await results.AsNoTracking().ToListAsync(cancellationToken)
        };

    }

    public virtual async Task<IEnumerable<TModel>> GetListAsync(bool orderDesc = true,
        Expression<Func<TModel, bool>>? expression = null,
        Expression<Func<TModel, object>>? ordered = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<TModel, object>>[] includes)
        => await GetAll(expression, orderDesc, ordered, includes).ToListAsync(cancellationToken);

    public virtual async Task<TModel?> UpdateAsync(TModel model, bool verifyEntity = true, CancellationToken cancellationToken = default)
    {
        if (verifyEntity)
        {
            var entity = await GetByIdAsync(model.Id, true);

            if (entity == null)
                return default;

            model.CreatedBy = entity.CreatedBy;
            model.CreatedAt = entity.CreatedAt;
        }

        _context.Set<TModel>().Update(model);

        await CommitAsync(cancellationToken);

        return model;
    }

    public virtual async Task<TModel?> GetByIdAsync(Guid id,
        bool asNotTraking = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<TModel, object>>[] includes)
    {
        var results = GetAll(null, true, x => x.CreatedAt, includes);

        if (asNotTraking) results = results.AsNoTracking();

        return await results.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    }

    public virtual async Task<bool> SoftRemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id);

        if (entity == null) return false;

        entity.IsDeleted = true;

        _context.Set<TModel>().Update(entity);

        await CommitAsync(cancellationToken);

        return true;

    }

    public virtual async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, true);

        if (entity == null) return false;

        _context.Set<TModel>().Remove(entity);

        await CommitAsync(cancellationToken);

        return true;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default, params Expression<Func<TModel, bool>>[] expression)
    {
        var result = _context.Set<TModel>()
            .AsQueryable();

        foreach (var item in expression) result = result.Where(item);

        return await result.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistAsync(Expression<Func<TModel, bool>>? expression = default, CancellationToken cancellationToken = default)
    {
        var result = _context.Set<TModel>();

        if (expression != null)
        {
            return await result.AnyAsync(expression, cancellationToken);
        }

        return await result.AnyAsync(cancellationToken);
    }
}
