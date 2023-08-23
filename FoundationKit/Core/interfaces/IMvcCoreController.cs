using System.Security.Claims;

namespace FoundationKit.Core.interfaces;


/// <summary>
/// Represent the sign of CoreController
/// </summary>
/// <typeparam name="Model">Represent the class model </typeparam>
/// 
public interface IMvcCoreController<Model>
{
    /// <summary>
    /// Endpoint base to create a entity
    /// </summary>
    /// <typeparam name="inputModel">Represent the class children of BaseInputModel </typeparam>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>201 created</returns>
    Task<IActionResult> Create(Model inputModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Endpoint base to updated a entity
    /// </summary>
    /// <typeparam name="editModel">Represent the class children of BaseEditModel </typeparam>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>200 ok</returns>
    Task<IActionResult> Update(Model editModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Endpoint to apply SoftRemove a entity
    /// </summary>
    /// <param name="id">primary key</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>200 ok</returns>
    Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Endpoint to get all registers paginated
    /// </summary>
    /// <param name="paginate">paginations parameters</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>200 ok</returns>
    Task<IActionResult> Index(Paginate paginate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Endpoint to get a entity
    /// </summary>
    /// <param name="id">primary key</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>200 ok</returns>
    Task<IActionResult> Update(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a alert from sweetalert2
    /// Usage the partial view named _NotificationPartial in the view
    /// </summary>
    /// <param name="title">title from notification</param>
    /// <param name="type">type of notification please see <see cref="MvcCoreNotification"/></param>
    /// <param name="message">body message</param>
    void ShowAlert(string title, MvcCoreNotification type = MvcCoreNotification.Success, string? message = default, string? config = default);

    /// <summary>
    /// Get user id from user logged
    /// </summary>
    /// <returns></returns>
    string? GetUserId(string? claimType = ClaimTypes.NameIdentifier);
}
