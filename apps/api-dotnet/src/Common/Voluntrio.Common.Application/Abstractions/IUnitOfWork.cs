namespace Voluntrio.Common.Application.Abstractions;

/// <summary>
/// Coordinates transactional execution for application operations.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Executes an operation within a transaction and commits conditionally.
    /// </summary>
    /// <typeparam name="TResponse">The type returned by the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="shouldCommit">Determines whether to commit based on operation response.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation response.</returns>
    Task<TResponse> ExecuteInTransactionAsync<TResponse>(
        Func<CancellationToken, Task<TResponse>> operation,
        Func<TResponse, bool> shouldCommit,
        CancellationToken cancellationToken);
}
