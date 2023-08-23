namespace FoundationKit.Core.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class ApiCoreController<TModel, TService> : ControllerBase
    where TModel : BaseModel
    where TService : IBaseRepository<TModel>
{
    private readonly TService _service;
    public ApiCoreController(TService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get a entity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public virtual async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _service.GetByIdAsync(id, cancellationToken: cancellationToken);

        if (response == null)
            return NotFound();

        return Ok(response);
    }

    /// <summary>
    /// Create a entity
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<IActionResult> AddAsync(TModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await _service.CreateAsync(model, cancellationToken: cancellationToken);

        if (response == null)
            return BadRequest("Error saving data to database");

        return Created(nameof(GetByIdAsync), model);
    }

    /// <summary>
    /// Remove a entity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id:guid}")]
    public virtual async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.SoftRemoveAsync(id, cancellationToken);

        if (!result)
            return NotFound(id);

        return Ok(result);
    }

    /// <summary>
    /// Get all entities paginated
    /// </summary>
    /// <param name="paginate">Paginated model</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<IActionResult> GetAllPaginatedAsync([FromQuery] Paginate paginate,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _service.GetPaginatedListAsync(paginate));
    }


    /// <summary>
    /// Update a entity
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public virtual async Task<IActionResult> UpdateAsync(Guid id, TModel model,
        CancellationToken cancellationToken = default)
    {
        var exist = await _service.ExistAsync(x => x.Id == model.Id, cancellationToken);
        if (!exist)
            return NotFound(exist);

        model.Id = id;
        var response = await _service.UpdateAsync(model, cancellationToken);

        if (response == null) return BadRequest(response);

        return Ok(response);
    }
}