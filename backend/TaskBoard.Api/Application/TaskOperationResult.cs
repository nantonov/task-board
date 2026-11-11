using TaskBoard.Api.Domain;

namespace TaskBoard.Api.Application;

public sealed record TaskOperationResult(
    TaskOperationOutcome Outcome,
    TaskCard? Card = null,
    IReadOnlyDictionary<string, string[]>? Errors = null)
{
    public static TaskOperationResult Success(TaskCard card) => new(TaskOperationOutcome.Success, card);

    public static TaskOperationResult Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(TaskOperationOutcome.Invalid, Errors: errors);

    public static TaskOperationResult NotFound() => new(TaskOperationOutcome.NotFound);

    public static TaskOperationResult Conflict() => new(TaskOperationOutcome.Conflict);
}
