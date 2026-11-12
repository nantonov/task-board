using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace TaskBoard.Api.Tests;

public sealed class TaskEndpointsTests : IAsyncLifetime
{
    private readonly BoardApiFactory _factory = new();
    private HttpClient _client = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync()
    {
        _client = _factory.CreateClient();
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task List_EmptyBoard_ReturnsEmptyArray()
    {
        var response = await _client.GetAsync("/api/tasks", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("[]", await response.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task Create_ValidCard_Returns201InToDoWithLocation()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks",
            new { key = "BRD-1", title = "  Scaffold board ", description = "first card" }, Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/api/tasks/BRD-1", response.Headers.Location?.OriginalString);
        var card = await ReadJsonAsync(response);
        Assert.Equal("BRD-1", card.GetProperty("key").GetString());
        Assert.Equal("Scaffold board", card.GetProperty("title").GetString());
        Assert.Equal("first card", card.GetProperty("description").GetString());
        Assert.Equal("ToDo", card.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Create_WithoutDescription_StoresEmptyDescription()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new { key = "BRD-2", title = "No body" }, Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("", (await ReadJsonAsync(response)).GetProperty("description").GetString());
    }

    [Fact]
    public async Task Create_DuplicateKey_Returns409ProblemDetails()
    {
        await CreateAsync("BRD-3");

        var response = await _client.PostAsJsonAsync("/api/tasks", new { key = "BRD-3", title = "again" }, Ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("BRD-3", (await ReadJsonAsync(response)).GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Create_InvalidFields_Returns400WithFieldErrors()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks",
            new { key = "brd 1", title = "", description = new string('x', 4001) }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await ReadJsonAsync(response)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("key", out _));
        Assert.True(errors.TryGetProperty("title", out _));
        Assert.True(errors.TryGetProperty("description", out _));
    }

    [Fact]
    public async Task Create_TitlePaddedPastLimit_AcceptedWhenTrimmedTitleFits()
    {
        var title = "  " + new string('t', 200) + "  ";

        var response = await _client.PostAsJsonAsync("/api/tasks", new { key = "BRD-8", title }, Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(200, (await ReadJsonAsync(response)).GetProperty("title").GetString()!.Length);
    }

    [Fact]
    public async Task Get_ExistingCard_ReturnsIt()
    {
        await CreateAsync("BRD-4");

        var response = await _client.GetAsync("/api/tasks/BRD-4", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("BRD-4", (await ReadJsonAsync(response)).GetProperty("key").GetString());
    }

    [Fact]
    public async Task Get_MissingCard_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync("/api/tasks/BRD-404", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Patch_KnownStatus_MovesCardAndListShowsIt()
    {
        await CreateAsync("BRD-5");

        var response = await _client.PatchAsJsonAsync("/api/tasks/BRD-5", new { status = "InProgress" }, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("InProgress", (await ReadJsonAsync(response)).GetProperty("status").GetString());
        var list = await _client.GetFromJsonAsync<JsonElement>("/api/tasks", Ct);
        Assert.Equal("InProgress", list[0].GetProperty("status").GetString());
    }

    [Theory]
    [InlineData("Doing")]
    [InlineData("1")]
    [InlineData("ToDo,Done")]
    [InlineData("done")]
    public async Task Patch_UnknownStatus_Returns400WithStatusError(string status)
    {
        await CreateAsync("BRD-6");

        var response = await _client.PatchAsJsonAsync("/api/tasks/BRD-6", new { status }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True((await ReadJsonAsync(response)).GetProperty("errors").TryGetProperty("status", out _));
    }

    [Fact]
    public async Task Get_MalformedKey_Returns404WithoutEchoingTheInput()
    {
        var response = await _client.GetAsync("/api/tasks/%3Cscript%3E", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("No card with that key.", (await ReadJsonAsync(response)).GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Patch_MissingCard_Returns404()
    {
        var response = await _client.PatchAsJsonAsync("/api/tasks/BRD-404", new { status = "Done" }, Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_OrdersByCreationTimeNotByKey()
    {
        await CreateAsync("BRD-9");
        await CreateAsync("BRD-10");

        var list = await _client.GetFromJsonAsync<JsonElement>("/api/tasks", Ct);

        Assert.Equal(["BRD-9", "BRD-10"], list.EnumerateArray().Select(c => c.GetProperty("key").GetString()));
    }

    private async Task CreateAsync(string key)
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new { key, title = $"Card {key}" }, Ct);
        response.EnsureSuccessStatusCode();
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(Ct);
}
