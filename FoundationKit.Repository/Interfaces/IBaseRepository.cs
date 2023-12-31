﻿namespace FoundationKit.Repository.Interfaces;
/// <summary>
/// Base methods
/// </summary>
/// <typeparam name="TModel">Represent the class children of BaseDtoModel</typeparam>
public interface IBaseRepository<TModel>
    where TModel : BaseModel
{
    /// <summary>
    /// Get a list paginated from the database
    /// </summary>
    /// <param name="paginate">Represent a class that contains a parameter of paginations</param>
    /// <param name="expression">Can use by filter data for any property of class</param>
    /// <param name="ordered">Represent the propery of order the data. But the default is used the property (CreatedAt)</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <param name="includes">Represent a list of includes can you use of class for create a joins in the db</param>
    /// <returns>A PaginationResult<TDtoModel> </returns>
    Task<PaginationResult<TModel>> GetPaginatedListAsync(Paginate paginate,
        Expression<Func<TModel, bool>>? expression = default,
        Expression<Func<TModel, object>>? ordered = default,
        CancellationToken cancellationToken = default,
        params Expression<Func<TModel, object>>[] includes);

    /// <summary>
    /// Get a list not paginated
    /// </summary>
    /// <param name="expression">Can use by filter data for any property of class</param>
    /// <param name="orderDesc">Define if the order is desc=true or asc=false</param>
    /// <param name="ordered">Represent the propery of order the data. But the default is used the property (CreatedAt)</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <param name="includes">Represent a list of includes can you use of class for create a joins in the db</param>
    /// <returns><IEnumerable<TDtoModel>></returns>
    Task<IEnumerable<TModel>> GetListAsync(
        bool orderDesc = true,
        Expression<Func<TModel, bool>>? expression = default,
        Expression<Func<TModel, object>>? ordered = default,
        CancellationToken cancellationToken = default,
        params Expression<Func<TModel, object>>[] includes);

    /// <summary>
    /// Get a IQueryable data from entity
    /// </summary>
    /// <param name="expression">Can use by filter data for any property of class</param>
    /// <param name="orderDesc">Define if the order is desc=true or asc=false</param>
    /// <param name="ordered">Represent the propery of order the data. But the default is used the property (CreatedAt)</param>
    /// <param name="includes">Represent a list of includes can you use of class for create a joins in the db</param>
    /// <returns> IQueryable<TDtoModel> </returns>
    IQueryable<TModel> GetAll(
        Expression<Func<TModel, bool>>? expression = default,
        bool orderDesc = true,
        Expression<Func<TModel, object>>? ordered = default,
        params Expression<Func<TModel, object>>[] includes);

    /// <summary>
    /// Create a new entity in the db
    /// </summary>
    /// <param name="model">Represent the class to create children from <c>TInputModel</c></param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>Return a new object mapped to <c>TDtoModel</c></returns>
    Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a Entity
    /// </summary>
    /// <param name="model">Represent the class to create children from <c>TEditModel</c></param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <param name="verifyEntity">flag indating if find the entity or omit the find</param>
    /// <returns>Return a new object mapped to <c>TDtoModel</c></returns>
    Task<TModel?> UpdateAsync(TModel model,  CancellationToken cancellationToken = default, bool verifyEntity = true);
    Task<TModel?> UpdateAsync(TModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a entity
    /// </summary>
    /// <param name="id">Is the identification unique of entity or primary key</param>
    /// <param name="asNotTraking">flag defined for indicate if entity is tracked for entity framework or not. Default is false</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <param name="includes">Represent a list of includes can you use of class for create a joins in the db</param>
    /// <returns>Return a new object mapped to <c>TDtoModel</c></returns>
    Task<TModel?> GetByIdAsync(Guid id, bool asNotTraking = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<TModel, object>>[] includes);

    /// <summary>
    /// Update the entity and set IsDeleted equals to true
    /// </summary>
    /// <param name="id">Is the identification unique of entity or primary key</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>bool</returns>

    Task<bool> SoftRemoveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a entity
    /// </summary>
    /// <param name="id">Is the identification unique of entity or primary key</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>bool</returns>
    Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify if the entity exist
    /// </summary>
    /// <param name="expression">Can use by filter data for any property of class</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>bool</returns>
    Task<bool> ExistAsync(Expression<Func<TModel, bool>>? expression = default, CancellationToken cancellationToken = default);
    /// <summary>
    /// Filter one if exist
    /// </summary>
    /// <param name="expression">Can use by filter data for any property of class</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>bool</returns>
    Task<TModel?> GetOneAsync(Expression<Func<TModel, bool>> expression, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count the entity in the db
    /// </summary>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <param name="expression">List of expressions can use by filter data for any property of class</param>
    /// <returns></returns>
    Task<int> CountAsync(
        CancellationToken cancellationToken = default,
         params Expression<Func<TModel, bool>>[] expression);
    /// <summary>
    /// Commit transaction and response null if is correct and response a string if have any error
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string?> CommitAndResultAsync(CancellationToken cancellationToken = default);
}