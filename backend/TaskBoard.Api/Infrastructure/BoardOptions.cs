namespace TaskBoard.Api.Infrastructure;

public sealed class BoardOptions
{
    public const string SectionName = "Board";

    public string DatabasePath { get; set; } = "data/board.db";

    public string UiOrigin { get; set; } = "http://localhost:3000";
}
