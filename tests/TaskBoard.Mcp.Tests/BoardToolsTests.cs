using System.Net;
using ModelContextProtocol;

namespace TaskBoard.Mcp.Tests;

public sealed class BoardToolsTests
{
    private const string CardJson = """
        {"key":"BRD-7","title":"Add assignee","description":"d","status":"ToDo",
         "createdAt":"2026-11-11T10:00:00+00:00","updatedAt":"2026-11-11T10:00:00+00:00"}
        """;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static BoardTools ToolsOver(HttpMessageHandler handler) =>
        new(new BoardApiClient(new HttpClient(handler) { BaseAddress = new Uri("http://127.0.0.1:5080/") }));

    [Fact]
    public async Task CreateTask_BackendAccepts_PostsCardAndReportsToDo()
    {
        var backend = new StubBackend(HttpStatusCode.Created, CardJson);

        var result = await ToolsOver(backend).CreateTaskAsync("BRD-7", "Add assignee", "d", Ct);

        Assert.Equal(HttpMethod.Post, backend.LastRequest!.Method);
        Assert.Equal("/api/tasks", backend.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("\"key\":\"BRD-7\"", backend.LastRequestBody);
        Assert.Equal(
            "Created BRD-7 \"Add assignee\" — now in ToDo\nDescription: d\n" +
            "Created: 2026-11-11T10:00:00.0000000+00:00; updated: 2026-11-11T10:00:00.0000000+00:00",
            result.ReplaceLineEndings("\n"));
    }

    [Fact]
    public async Task ListTasks_BackendSendsUnknownColumn_FailsLoudlyInsteadOfHidingTheCard()
    {
        var backend = new StubBackend(HttpStatusCode.OK, $"[{CardJson.Replace("\"ToDo\"", "\"Blocked\"")}]");

        var error = await Assert.ThrowsAsync<McpException>(() => ToolsOver(backend).ListTasksAsync(Ct));

        Assert.Contains("cannot read", error.Message);
    }

    [Fact]
    public async Task MoveTask_BackendAccepts_PatchesStatusAsColumnName()
    {
        var backend = new StubBackend(HttpStatusCode.OK, CardJson.Replace("\"ToDo\"", "\"InProgress\""));

        var result = await ToolsOver(backend).MoveTaskAsync("BRD-7", BoardColumn.InProgress, Ct);

        Assert.Equal(HttpMethod.Patch, backend.LastRequest!.Method);
        Assert.Equal("/api/tasks/BRD-7", backend.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("{\"status\":\"InProgress\"}", backend.LastRequestBody);
        Assert.StartsWith("Moved BRD-7 \"Add assignee\" — now in InProgress", result);
    }

    [Fact]
    public async Task MoveTask_UnknownCard_FailsLoudlyWithBackendDetail()
    {
        var backend = new StubBackend(HttpStatusCode.NotFound,
            """{"title":"Card not found","status":404,"detail":"No card with key 'BRD-9'."}""",
            "application/problem+json");

        var error = await Assert.ThrowsAsync<McpException>(() =>
            ToolsOver(backend).MoveTaskAsync("BRD-9", BoardColumn.Done, Ct));

        Assert.Contains("404", error.Message);
        Assert.Contains("No card with key 'BRD-9'.", error.Message);
    }

    [Fact]
    public async Task CreateTask_DuplicateKey_FailsWith409()
    {
        var backend = new StubBackend(HttpStatusCode.Conflict,
            """{"title":"Card already exists","status":409,"detail":"A card with key 'BRD-7' already exists."}""",
            "application/problem+json");

        var error = await Assert.ThrowsAsync<McpException>(() =>
            ToolsOver(backend).CreateTaskAsync("BRD-7", "t", "d", Ct));

        Assert.Contains("409", error.Message);
    }

    [Fact]
    public async Task CreateTask_ValidationProblem_ListsFieldErrors()
    {
        var backend = new StubBackend(HttpStatusCode.BadRequest,
            """{"title":"One or more validation errors occurred.","status":400,"errors":{"title":["Title is required."]}}""",
            "application/problem+json");

        var error = await Assert.ThrowsAsync<McpException>(() =>
            ToolsOver(backend).CreateTaskAsync("BRD-7", "", "d", Ct));

        Assert.Contains("title: Title is required.", error.Message);
    }

    [Theory]
    [InlineData("brd-1")]
    [InlineData("../admin")]
    [InlineData("BRD-1/../../x")]
    public async Task MoveTask_MalformedKey_RejectedBeforeAnyHttpCall(string key)
    {
        var backend = new StubBackend(HttpStatusCode.OK, CardJson);

        await Assert.ThrowsAsync<McpException>(() => ToolsOver(backend).MoveTaskAsync(key, BoardColumn.Done, Ct));

        Assert.Null(backend.LastRequest);
    }

    [Fact]
    public async Task CreateTask_BackendDown_FailsLoudlyAndSaysCardNotUpdated()
    {
        var error = await Assert.ThrowsAsync<McpException>(() =>
            ToolsOver(new RefusingBackend()).CreateTaskAsync("BRD-7", "t", "d", Ct));

        Assert.Contains("unreachable", error.Message);
        Assert.Contains("NOT updated", error.Message);
    }

    private sealed class RefusingBackend : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new HttpRequestException("No connection could be made because the target machine actively refused it.");
    }

    [Fact]
    public async Task ListTasks_GroupsCardsByColumn()
    {
        var backend = new StubBackend(HttpStatusCode.OK, $"[{CardJson}]");

        var result = await ToolsOver(backend).ListTasksAsync(Ct);

        Assert.Equal("ToDo:\n  BRD-7 — Add assignee\nInProgress:\nDone:", result.ReplaceLineEndings("\n"));
    }
}
