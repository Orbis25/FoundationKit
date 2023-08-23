﻿namespace FoundationKit.Core.Controllers;

public abstract class MvcCoreController<TModel, TService> : Controller, IMvcCoreController<TModel>
    where TModel : BaseModel
    where TService : IBaseRepository<TModel>
{
    private readonly TService _service;
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
            ShowAlert("Error", MvcCoreNotification.Error);
            return View(inputModel);
        }

        ShowAlert("Success");

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Remove a entity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("{id:guid}")]
    public virtual async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
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
    /// Get a entity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public virtual async Task<IActionResult> GetById(Guid id,
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
            return View(nameof(GetById), editModel);

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
            ShowAlert("Error", MvcCoreNotification.Error);
            return View(nameof(GetById), editModel);
        }

        ShowAlert("Success");
        return RedirectToAction(nameof(Index));
    }

    public virtual void ShowAlert(string title, MvcCoreNotification type = MvcCoreNotification.Success, string? message = default, string? config = default)
        => this.SendNotification(title, type, message, config);

    public virtual string? GetUserId(string? claimType = ClaimTypes.NameIdentifier)
    {
        return HttpContext?.User?.Claims?.FirstOrDefault((c => c.Type == claimType))?.Value;
    }
}