using Dapper;

namespace TaskBoard.Api.Infrastructure;

public sealed class DatabaseInitializer(SqliteConnectionFactory connectionFactory)
{
    private const string Schema = """
        CREATE TABLE IF NOT EXISTS tasks (
            key         TEXT PRIMARY KEY NOT NULL,
            title       TEXT NOT NULL,
            description TEXT NOT NULL,
            status      TEXT NOT NULL,
            created_at  TEXT NOT NULL,
            updated_at  TEXT NOT NULL
        );
        """;

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(Schema, cancellationToken: cancellationToken));
    }
}
