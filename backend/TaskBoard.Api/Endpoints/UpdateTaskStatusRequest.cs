namespace TaskBoard.Api.Endpoints;

// Status stays a string at the boundary so an unknown value becomes a field-level
// validation error (errors.status) instead of a JSON deserialization failure.
public sealed record UpdateTaskStatusRequest(string? Status);
