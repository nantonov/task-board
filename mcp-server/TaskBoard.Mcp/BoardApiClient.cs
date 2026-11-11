using System.Net.Http.Json;
using System.Text.Json;

namespace TaskBoard.Mcp;

public sealed class BoardApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public Task<IReadOnlyList<TaskCardDto>> ListAsync(CancellationToken cancellationToken) =>
        SendAsync<IReadOnlyList<TaskCardDto>>(new HttpRequestMessage(HttpMethod.Get, "api/tasks"), cancellationToken);

    public Task<TaskCardDto> CreateAsync(string key, string title, string description, CancellationToken cancellationToken) =>
        SendAsync<TaskCardDto>(
            new HttpRequestMessage(HttpMethod.Post, "api/tasks")
            {
                Content = JsonContent.Create(new { key, title, description }, options: Json),
            },
            cancellationToken);

    public Task<TaskCardDto> MoveAsync(string key, BoardColumn column, CancellationToken cancellationToken) =>
        SendAsync<TaskCardDto>(
            new HttpRequestMessage(HttpMethod.Patch, $"api/tasks/{Uri.EscapeDataString(key)}")
            {
                Content = JsonContent.Create(new { status = column.ToString() }, options: Json),
            },
            cancellationToken);

    private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var _ = request;
        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new BoardApiException(
                $"Board backend unreachable at {http.BaseAddress} ({ex.Message}). The card was NOT updated; " +
                "start the backend and repeat the call.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new BoardApiException(
                $"Board backend at {http.BaseAddress} did not answer within {http.Timeout.TotalSeconds:0}s. " +
                "The card state is unknown; check it with list_tasks once the backend responds.", ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new BoardApiException(
                    $"Board backend rejected the request: {(int)response.StatusCode} {await DescribeProblemAsync(response, cancellationToken)}");
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<T>(Json, cancellationToken)
                    ?? throw new BoardApiException("Board backend returned an empty body.");
            }
            catch (JsonException ex)
            {
                throw new BoardApiException($"Board backend returned a payload this server cannot read: {ex.Message}", ex);
            }
        }
    }

    private static async Task<string> DescribeProblemAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            using var problem = JsonDocument.Parse(body);
            var root = problem.RootElement;
            var parts = new List<string>();
            if (root.TryGetProperty("title", out var title)) parts.Add(title.GetString() ?? "");
            if (root.TryGetProperty("detail", out var detail)) parts.Add(detail.GetString() ?? "");
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                parts.AddRange(errors.EnumerateObject().Select(e => e.Value.ValueKind == JsonValueKind.Array
                    ? $"{e.Name}: {string.Join(" ", e.Value.EnumerateArray().Select(m => m.ToString()))}"
                    : $"{e.Name}: {e.Value}"));
            }

            return string.Join(" — ", parts.Where(p => p.Length > 0));
        }
        catch (JsonException)
        {
            return response.ReasonPhrase ?? "";
        }
    }
}
