using TaskBoard.Api.Domain;

namespace TaskBoard.Api.Endpoints;

public sealed record TaskCardResponse(
    string Key,
    string Title,
    string Description,
    CardStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static TaskCardResponse From(TaskCard card) => new(
        card.Key, card.Title, card.Description, card.Status, card.CreatedAt, card.UpdatedAt);
}
