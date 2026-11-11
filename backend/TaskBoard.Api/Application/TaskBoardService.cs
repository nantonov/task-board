using TaskBoard.Api.Domain;

namespace TaskBoard.Api.Application;

public sealed class TaskBoardService(
    ITaskRepository repository,
    TimeProvider timeProvider,
    ILogger<TaskBoardService> logger)
{
    public Task<IReadOnlyList<TaskCard>> ListAsync(CancellationToken cancellationToken) =>
        repository.ListAsync(cancellationToken);

    public async Task<TaskOperationResult> GetAsync(string key, CancellationToken cancellationToken)
    {
        var card = await repository.FindAsync(key, cancellationToken);
        return card is null ? TaskOperationResult.NotFound() : TaskOperationResult.Success(card);
    }

    public async Task<TaskOperationResult> CreateAsync(
        string? key, string? title, string? description, CancellationToken cancellationToken)
    {
        var errors = TaskCardValidator.ValidateNewCard(key, title, description);
        if (errors.Count > 0)
        {
            return TaskOperationResult.Invalid(errors);
        }

        var now = timeProvider.GetUtcNow();
        var card = new TaskCard(key!, title!.Trim(), description ?? "", CardStatus.ToDo, now, now);

        if (!await repository.TryInsertAsync(card, cancellationToken))
        {
            return TaskOperationResult.Conflict();
        }

        logger.LogInformation("Card {Key} created in {Status}", card.Key, card.Status);
        return TaskOperationResult.Success(card);
    }

    public async Task<TaskOperationResult> MoveAsync(
        string key, string? status, CancellationToken cancellationToken)
    {
        var errors = TaskCardValidator.ValidateStatus(status, out var newStatus);
        if (errors.Count > 0)
        {
            return TaskOperationResult.Invalid(errors);
        }

        if (!await repository.TryUpdateStatusAsync(key, newStatus, timeProvider.GetUtcNow(), cancellationToken))
        {
            return TaskOperationResult.NotFound();
        }

        logger.LogInformation("Card {Key} moved to {Status}", key, newStatus);
        return await GetAsync(key, cancellationToken);
    }
}
