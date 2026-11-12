using TaskBoard.Api.Application;

namespace TaskBoard.Api.Endpoints;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        // Anonymous on purpose: the API listens on loopback only and serves one local user.
        // Who may write is enforced by the agent host, which reaches the board only through
        // the MCP server (see DECISIONS.md).
        var tasks = app.MapGroup("/api/tasks").AllowAnonymous();

        tasks.MapGet("/", ListAsync);
        tasks.MapGet("/{key}", GetAsync);
        tasks.MapPost("/", CreateAsync);
        tasks.MapPatch("/{key}", MoveAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(TaskBoardService board, CancellationToken cancellationToken)
    {
        var cards = await board.ListAsync(cancellationToken);
        return TypedResults.Ok(cards.Select(TaskCardResponse.From).ToList());
    }

    private static async Task<IResult> GetAsync(string key, TaskBoardService board, CancellationToken cancellationToken)
    {
        var result = await board.GetAsync(key, cancellationToken);
        return ToHttpResult(result, key, card => TypedResults.Ok(TaskCardResponse.From(card)));
    }

    private static async Task<IResult> CreateAsync(
        CreateTaskRequest request, TaskBoardService board, CancellationToken cancellationToken)
    {
        var result = await board.CreateAsync(request.Key, request.Title, request.Description, cancellationToken);
        return ToHttpResult(result, request.Key, card =>
            TypedResults.Created($"/api/tasks/{card.Key}", TaskCardResponse.From(card)));
    }

    private static async Task<IResult> MoveAsync(
        string key, UpdateTaskStatusRequest request, TaskBoardService board, CancellationToken cancellationToken)
    {
        var result = await board.MoveAsync(key, request.Status, cancellationToken);
        return ToHttpResult(result, key, card => TypedResults.Ok(TaskCardResponse.From(card)));
    }

    private static IResult ToHttpResult(
        TaskOperationResult result, string? key, Func<Domain.TaskCard, IResult> onSuccess)
    {
        // Echo the key back only when it is well-formed; anything else stays out of the response.
        var shownKey = TaskCardValidator.IsValidKey(key) ? $"key '{key}'" : "that key";

        return result.Outcome switch
        {
            TaskOperationOutcome.Success => onSuccess(result.Card!),
            TaskOperationOutcome.Invalid => TypedResults.ValidationProblem(result.Errors!),
            TaskOperationOutcome.NotFound => TypedResults.Problem(
                title: "Card not found", detail: $"No card with {shownKey}.", statusCode: StatusCodes.Status404NotFound),
            TaskOperationOutcome.Conflict => TypedResults.Problem(
                title: "Card already exists", detail: $"A card with {shownKey} already exists.", statusCode: StatusCodes.Status409Conflict),
            _ => throw new InvalidOperationException($"Unhandled outcome {result.Outcome}."),
        };
    }
}
