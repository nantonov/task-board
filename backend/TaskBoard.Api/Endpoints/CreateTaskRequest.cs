namespace TaskBoard.Api.Endpoints;

public sealed record CreateTaskRequest(string? Key, string? Title, string? Description);
