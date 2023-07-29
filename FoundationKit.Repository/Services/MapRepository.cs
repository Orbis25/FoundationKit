namespace FoundationKit.Repository.Services;

/// <summary>
/// Permit create the base logic of services
/// </summary>
/// <typeparam name="TContext">The class represent a dbcontext</typeparam>
/// <typeparam name="TEntity">The entity class</typeparam>
/// <typeparam name="TInputModel">Represent the class children of BaseInputModel </typeparam>
/// <typeparam name="TEditModel">Represent the class children of BaseEditModel</typeparam>
/// <typeparam name="TDtoModel">Represent the class children of BaseDtoModel</typeparam>
public abstract class MapRepository<TContext, TEntity, TInputModel, TEditModel, TDtoModel> :
    IMapRepository<TInputModel, TEditModel, TDtoModel>
    where TContext : DbContext
    where TEntity : BaseModel
    where TInputModel : BaseInput
    where TDtoModel : BaseOutput
    where TEditModel : BaseEdit
{
    private readonly TContext _context;
    private readonly IMapper _mapper;
    protected MapRepository(TContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

    public virtual async Task<TDtoModel?> Create(TInputModel model, CancellationToken cancellationToken = default)
    {
        var _model = _mapper.Map<TInputModel, TEntity>(model);

        _context.Set<TEntity>().Add(_model);

        await CommitAsync(cancellationToken);

        return _mapper.Map<TEntity, TDtoModel>(_model);
    }

    public virtual IQueryable<TDtoModel> GetAll(Expression<Func<TDtoModel, bool>>? expression = default,
        bool orderDesc = true, Expression<Func<TDtoModel, object>>? ordered = default
        , params Expression<Func<TDtoModel, object>>[] includes)
    {
        var results = _context.Set<TEntity>().ProjectTo<TDtoModel>(_mapper.ConfigurationProvider).AsQueryable();
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

    public virtual async Task<PaginationResult<TDtoModel>> GetPaginatedList(Paginate paginate,
        Expression<Func<TDtoModel, bool>>? expression = default,
        Expression<Func<TDtoModel, object>>? ordered = default,
        CancellationToken cancellationToken = default,
        params Expression<Func<TDtoModel, object>>[] includes)
    {
        var results = GetAll(expression, paginate.OrderByDesc, ordered, includes);
        var total = results.Count();
        var pages = (int)Math.Ceiling((decimal)total / paginate.Qyt);

        results = results.Skip((paginate.Page - 1) * paginate.Qyt).Take(paginate.Qyt);

        return new PaginationResult<TDtoModel>
        {
            ActualPage = paginate.Page,
            Qyt = paginate.Qyt,
            PageTotal = pages,
            Total = total,
            Results = await results.AsNoTracking().ToListAsync(cancellationToken)
        };

    }

    public virtual async Task<IEnumerable<TDtoModel>> GetList(Expression<Func<TDtoModel, bool>>? expression = default,
        bool orderDesc = true,
        Expression<Func<TDtoModel, object>>? ordered = default,
        CancellationToken cancellationToken = default,
        params Expression<Func<TDtoModel, object>>[] includes)
        => await GetAll(expression, orderDesc, ordered, includes).ToListAsync(cancellationToken);

    public virtual async Task<TDtoModel?> Update(TEditModel model, CancellationToken cancellationToken = default)
    {
        var entity = await GetById(model.Id, true);

        if (entity == null)
            return default;

        model.CreatedBy = entity.CreatedBy;
        model.CreatedAt = entity.CreatedAt;

        var _model = _mapper.Map<TEditModel, TEntity>(model);
        _context.Set<TEntity>().Update(_model);

        await CommitAsync(cancellationToken);

        return _mapper.Map<TEntity, TDtoModel>(_model);

    }

    public virtual async Task<TDtoModel?> GetById(Guid id,
        bool asNotTraking = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<TDtoModel, object>>[] includes)
    {
        var results = _context.Set<TEntity>().AsQueryable()
            .ProjectTo<TDtoModel>(_mapper.ConfigurationProvider);

        if (results == null) return default;

        if (includes != null)
            foreach (var include in includes) results = results.Include(include);

        if (asNotTraking) results = results.AsNoTracking();

        return await results.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public virtual async Task<bool> SoftRemove(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await GetById(id, true);
        if (result == null) return false;

        var entity = _mapper.Map<TDtoModel, TEntity>(result);

        if (entity == null) return false;

        entity.IsDeleted = true;

        _context.Set<TEntity>().Update(entity);

        await CommitAsync(cancellationToken);

        return true;

    }

    public virtual async Task<bool> Remove(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await GetById(id, true);
        if (result == null) return false;

        var entity = _mapper.Map<TDtoModel, TEntity>(result);

        if (entity == null) return false;

        _context.Set<TEntity>().Remove(entity);

        await CommitAsync(cancellationToken);

        return true;
    }

    public async Task<int> Count(CancellationToken cancellationToken = default, params Expression<Func<TDtoModel, bool>>[] expression)
    {
        var result = _context.Set<TEntity>().ProjectTo<TDtoModel>(_mapper.ConfigurationProvider)
            .AsQueryable();

        foreach (var item in expression) result = result.Where(item);

        return await result.CountAsync(cancellationToken);
    }

    public async Task<bool> Exist(Expression<Func<TDtoModel, bool>>? expression = default, CancellationToken cancellationToken = default)
    {
        var result = _context.Set<TEntity>().ProjectTo<TDtoModel>(_mapper.ConfigurationProvider);

        if (expression == null) return false;

        return await result.AnyAsync(expression, cancellationToken);
    }
}