namespace FoundationKit.Core.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class ApiMapController<TService, TDtoModel, TInputModel, TEditModel> : ControllerBase
   , IApiMapController<TInputModel, TEditModel>
   where TInputModel : BaseInput
   where TDtoModel : BaseOutput
   where TEditModel : BaseEdit
   where TService : IMapRepository<TInputModel, TEditModel, TDtoModel>
{
    private readonly TService _service;
    protected ApiMapController(TService service) => _service = service;


    [HttpPost]
    public virtual async Task<IActionResult> Create(TInputModel inputModel, CancellationToken cancellationToken = default)
    {
        return Created(nameof(GetById), await _service.Create(inputModel, cancellationToken));
    }

    /// <summary>
    /// Remove a entity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id:guid}")]
    public virtual async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.SoftRemove(id, cancellationToken);

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
    public virtual async Task<IActionResult> GetAll([FromQuery] Paginate paginate,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _service.GetPaginatedList(paginate));
    }


    /// <summary>
    /// Get a entity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public virtual async Task<IActionResult> GetById(Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await _service.GetById(id, cancellationToken: cancellationToken);
        if (response == null) return NotFound("Not Found");
        return Ok(response);
    }

    /// <summary>
    /// Update a entity
    /// </summary>
    /// <param name="editModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public virtual async Task<IActionResult> Update(Guid id, TEditModel editModel,
        CancellationToken cancellationToken = default)
    {
        var exist = await _service.Exist(x => x.Id == id, cancellationToken);
        if (!exist)
            return NotFound(exist);

        editModel.Id = id;
        var response = await _service.Update(editModel, cancellationToken);

        if (response == null) return BadRequest(response);

        return Ok(response);
    }
}
