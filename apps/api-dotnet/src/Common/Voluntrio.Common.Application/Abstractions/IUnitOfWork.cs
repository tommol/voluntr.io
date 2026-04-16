namespace Voluntrio.Common.Application.Abstractions;

public interface IUnitOfWork
{
    Task<TResponse> ExecuteInTransactionAsync<TResponse>(
        Func<CancellationToken, Task<TResponse>> operation,
        Func<TResponse, bool> shouldCommit,
        CancellationToken cancellationToken);
}
