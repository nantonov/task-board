namespace TaskBoard.Mcp;

public sealed record TaskCardDto(
    string Key,
    string Title,
    string Description,
    BoardColumn Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
