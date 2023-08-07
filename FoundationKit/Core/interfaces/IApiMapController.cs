
namespace FoundationKit.Core.interfaces;
/// <summary>
/// Represent the sign of CoreController
/// </summary>
/// <typeparam name="TInputModel">Represent the class children of BaseInputModel </typeparam>
/// <typeparam name="TEditModel">Represent the class children of BaseEditModel</typeparam>
public interface IApiMapController<TInputModel, TEditModel>
    where TInputModel : BaseInput
    where TEditModel : BaseEdit
{
    /// <summary>
    /// Endpoint base to create a entity
    /// </summary>
    /// <typeparam name="inputModel">Represent the class children of BaseInputModel </typeparam>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>201 created</returns>
    Task<IActionResult> Create(TInputModel inputModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Endpoint base to updated a entity
    /// </summary>
    /// <typeparam name="editModel">Represent the class children of BaseEditModel </typeparam>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>200 ok</returns>
    Task<IActionResult> Update(Guid id,TEditModel editModel, CancellationToken cancellationToken = default);

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
    Task<IActionResult> GetAll(Paginate paginate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Endpoint to get a entity
    /// </summary>
    /// <param name="id">primary key</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>200 ok</returns>
    Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default);
}