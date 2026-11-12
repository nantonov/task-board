using System.Globalization;
using Dapper;
using TaskBoard.Api.Application;
using TaskBoard.Api.Domain;

namespace TaskBoard.Api.Infrastructure;

public sealed class TaskRepository(SqliteConnectionFactory connectionFactory) : ITaskRepository
{
    private const string SelectColumns =
        "key AS Key, title AS Title, description AS Description, status AS Status, " +
        "created_at AS CreatedAt, updated_at AS UpdatedAt";

    public async Task<IReadOnlyList<TaskCard>> ListAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var rows = await connection.QueryAsync<TaskRow>(new CommandDefinition(
            $"SELECT {SelectColumns} FROM tasks ORDER BY created_at, key",
            cancellationToken: cancellationToken));
        return rows.Select(r => r.ToCard()).ToList();
    }

    public async Task<TaskCard?> FindAsync(string key, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<TaskRow>(new CommandDefinition(
            $"SELECT {SelectColumns} FROM tasks WHERE key = @key",
            new { key },
            cancellationToken: cancellationToken));
        return row?.ToCard();
    }

    public async Task<bool> TryInsertAsync(TaskCard card, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var inserted = await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT OR IGNORE INTO tasks (key, title, description, status, created_at, updated_at)
            VALUES (@Key, @Title, @Description, @Status, @CreatedAt, @UpdatedAt)
            """,
            new
            {
                card.Key,
                card.Title,
                card.Description,
                Status = card.Status.ToString(),
                CreatedAt = Format(card.CreatedAt),
                UpdatedAt = Format(card.UpdatedAt),
            },
            cancellationToken: cancellationToken));
        return inserted == 1;
    }

    public async Task<bool> TryUpdateStatusAsync(
        string key, CardStatus status, DateTimeOffset updatedAt, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var updated = await connection.ExecuteAsync(new CommandDefinition(
            "UPDATE tasks SET status = @status, updated_at = @updatedAt WHERE key = @key",
            new { key, status = status.ToString(), updatedAt = Format(updatedAt) },
            cancellationToken: cancellationToken));
        return updated == 1;
    }

    private static string Format(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
}
