namespace FoundationKit.Core.Controllers;

public abstract class MvcCoreController<TModel, TService> : Controller, IMvcCoreController<TModel>
    where TModel : BaseModel
    where TService : IBaseRepository<TModel>
{
    private readonly TService _service;

    protected virtual string CreateSuccess { get; set; } = "Created Success";
    protected virtual string CreateError { get; set; } = "Error creating";
    protected virtual string UpdateSuccess { get; set; } = "Updated Seccess";
    protected virtual string UpdateError { get; set; } = "Error updating";

    public MvcCoreController(TService service)
    {
        _service = service;
    }
    [HttpGet]
    public virtual IActionResult Create() => View(nameof(Create));

    [HttpPost]
    public virtual async Task<IActionResult> Create(TModel inputModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return View(inputModel);

        var result = await _service.CreateAsync(inputModel, cancellationToken);

        if (result == null)
        {
            ShowAlert(CreateError, MvcCoreNotification.Error);
            return View(inputModel);
        }

        ShowAlert(CreateSuccess);

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Remove a entity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
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
    public virtual async Task<IActionResult> Index([FromQuery] Paginate paginate,
        CancellationToken cancellationToken = default)
    {
        return View(nameof(Index), await _service.GetPaginatedListAsync(paginate));
    }


    /// <summary>
    /// Get a entity for update
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<IActionResult> Update(Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await _service.GetByIdAsync(id, cancellationToken: cancellationToken);

        if (response == null)
        {
            //send to not found page
        }

        return View(response);
    }

    /// <summary>
    /// Update a entity
    /// </summary>
    /// <param name="editModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<IActionResult> Update(TModel editModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return View(nameof(Update), editModel);

        var exist = await _service.ExistAsync(x => x.Id == editModel.Id, cancellationToken);
        if (!exist)
        {
            return NotFound();
            //send to not found page
        }

        var response = await _service.UpdateAsync(editModel, cancellationToken: cancellationToken);

        if (response == null)
        {
            //send notification
            ShowAlert(UpdateError, MvcCoreNotification.Error);
            return View(nameof(Update), editModel);
        }

        ShowAlert(UpdateSuccess);
        return RedirectToAction(nameof(Index));
    }

    public virtual void ShowAlert(string title, MvcCoreNotification type = MvcCoreNotification.Success, string? message = default, string? config = default)
        => this.SendNotification(title, type, message, config);

    public virtual string? GetUserId(string? claimType = ClaimTypes.NameIdentifier)
    {
        return HttpContext?.User?.Claims?.FirstOrDefault((c => c.Type == claimType))?.Value;
    }
}
