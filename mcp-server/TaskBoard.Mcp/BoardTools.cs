using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace TaskBoard.Mcp;

[McpServerToolType]
public sealed partial class BoardTools(BoardApiClient board)
{
    [GeneratedRegex(@"^[A-Z]+-\d+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex KeyPattern();

    [McpServerTool(Name = "create_task")]
    [Description("Create a card in the To Do column of the personal task board. Call it when a new ticket is started.")]
    public async Task<string> CreateTaskAsync(
        [Description("Ticket key, e.g. BRD-12")] string key,
        [Description("Short card title")] string title,
        [Description("Ticket text shown on the card")] string description,
        CancellationToken cancellationToken)
    {
        EnsureKey(key);
        var card = await CallAsync(() => board.CreateAsync(key, title, description, cancellationToken));
        return "Created " + Describe(card);
    }

    [McpServerTool(Name = "move_task")]
    [Description("Move an existing card to another column of the personal task board.")]
    public async Task<string> MoveTaskAsync(
        [Description("Ticket key, e.g. BRD-12")] string key,
        [Description("Target column")] BoardColumn status,
        CancellationToken cancellationToken)
    {
        EnsureKey(key);
        var card = await CallAsync(() => board.MoveAsync(key, status, cancellationToken));
        return "Moved " + Describe(card);
    }

    [McpServerTool(Name = "list_tasks", ReadOnly = true)]
    [Description("List every card on the personal task board, grouped by column.")]
    public async Task<string> ListTasksAsync(CancellationToken cancellationToken)
    {
        var cards = await CallAsync(() => board.ListAsync(cancellationToken));
        if (cards.Count == 0)
        {
            return "The board is empty.";
        }

        var text = new StringBuilder();
        foreach (var column in Enum.GetValues<BoardColumn>())
        {
            text.AppendLine($"{column}:");
            foreach (var card in cards.Where(c => c.Status == column))
            {
                text.AppendLine($"  {card.Key} — {card.Title}");
            }
        }

        return text.ToString().TrimEnd();
    }

    private static void EnsureKey(string key)
    {
        if (!KeyPattern().IsMatch(key))
        {
            throw new McpException($"Key '{key}' is not a ticket key; expected capital letters, a dash and digits, e.g. BRD-12.");
        }
    }

    private static async Task<T> CallAsync<T>(Func<Task<T>> call)
    {
        try
        {
            return await call();
        }
        catch (BoardApiException ex)
        {
            throw new McpException(ex.Message, ex);
        }
    }

    private static string Describe(TaskCardDto card) =>
        $"""
        {card.Key} "{card.Title}" — now in {card.Status}
        Description: {(card.Description.Length == 0 ? "(none)" : card.Description)}
        Created: {card.CreatedAt:O}; updated: {card.UpdatedAt:O}
        """;
}
