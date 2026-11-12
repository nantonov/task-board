using TaskBoard.Api.Domain;

namespace TaskBoard.Api.Application;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskCard>> ListAsync(CancellationToken cancellationToken);

    Task<TaskCard?> FindAsync(string key, CancellationToken cancellationToken);

    /// <returns>false when a card with this key already exists.</returns>
    Task<bool> TryInsertAsync(TaskCard card, CancellationToken cancellationToken);

    /// <returns>false when no card has this key.</returns>
    Task<bool> TryUpdateStatusAsync(string key, CardStatus status, DateTimeOffset updatedAt, CancellationToken cancellationToken);
}
