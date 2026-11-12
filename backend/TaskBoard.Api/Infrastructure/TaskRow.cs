using System.Globalization;
using TaskBoard.Api.Domain;

namespace TaskBoard.Api.Infrastructure;

// SQLite has no date type and Dapper cannot cast TEXT to DateTimeOffset, so rows are read
// as strings and converted here rather than through a global type handler.
public sealed class TaskRow
{
    public string Key { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string Status { get; init; } = "";
    public string CreatedAt { get; init; } = "";
    public string UpdatedAt { get; init; } = "";

    public TaskCard ToCard() => new(
        Key,
        Title,
        Description,
        Enum.Parse<CardStatus>(Status),
        DateTimeOffset.Parse(CreatedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        DateTimeOffset.Parse(UpdatedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
}
