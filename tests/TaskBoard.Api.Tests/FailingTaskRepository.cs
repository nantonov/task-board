using TaskBoard.Api.Application;
using TaskBoard.Api.Domain;

namespace TaskBoard.Api.Tests;

public sealed class FailingTaskRepository : ITaskRepository
{
    private static Exception Failure() =>
        new InvalidOperationException(@"SQLite Error 14: unable to open database file C:\secret\board.db");

    public Task<IReadOnlyList<TaskCard>> ListAsync(CancellationToken cancellationToken) => throw Failure();

    public Task<TaskCard?> FindAsync(string key, CancellationToken cancellationToken) => throw Failure();

    public Task<bool> TryInsertAsync(TaskCard card, CancellationToken cancellationToken) => throw Failure();

    public Task<bool> TryUpdateStatusAsync(string key, CardStatus status, DateTimeOffset updatedAt, CancellationToken cancellationToken) =>
        throw Failure();
}
