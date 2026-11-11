namespace TaskBoard.Api.Domain;

public sealed record TaskCard(
    string Key,
    string Title,
    string Description,
    CardStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
