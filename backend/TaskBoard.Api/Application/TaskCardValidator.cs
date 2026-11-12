using System.Text.RegularExpressions;
using TaskBoard.Api.Domain;

namespace TaskBoard.Api.Application;

public static partial class TaskCardValidator
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 4000;

    [GeneratedRegex(@"^[A-Z]+-\d+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex KeyPattern();

    public static IReadOnlyDictionary<string, string[]> ValidateNewCard(string? key, string? title, string? description)
    {
        var errors = new Dictionary<string, string[]>();

        if (!IsValidKey(key))
        {
            errors["key"] = ["Key must look like BRD-12: capital letters, a dash, digits."];
        }

        var trimmedTitle = title?.Trim();
        if (string.IsNullOrEmpty(trimmedTitle) || trimmedTitle.Length > TitleMaxLength)
        {
            errors["title"] = [$"Title is required and must be at most {TitleMaxLength} characters."];
        }

        if (description is { Length: > DescriptionMaxLength })
        {
            errors["description"] = [$"Description must be at most {DescriptionMaxLength} characters."];
        }

        return errors;
    }

    // Exact names only: Enum.TryParse would also accept "1" or "ToDo,Done".
    public static IReadOnlyDictionary<string, string[]> ValidateStatus(string? value, out CardStatus status)
    {
        status = default;
        if (value is not null && Enum.GetNames<CardStatus>().Contains(value, StringComparer.Ordinal))
        {
            status = Enum.Parse<CardStatus>(value);
            return new Dictionary<string, string[]>();
        }

        return new Dictionary<string, string[]>
        {
            ["status"] = [$"Status must be one of: {string.Join(", ", Enum.GetNames<CardStatus>())}."],
        };
    }

    public static bool IsValidKey(string? key) => key is not null && KeyPattern().IsMatch(key);
}
