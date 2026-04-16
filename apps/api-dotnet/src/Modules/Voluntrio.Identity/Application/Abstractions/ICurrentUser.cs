namespace Voluntrio.Identity.Application.Abstractions;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? ExternalSubject { get; }

    Guid? InternalUserId { get; }
}
