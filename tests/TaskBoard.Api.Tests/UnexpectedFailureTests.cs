using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TaskBoard.Api.Application;

namespace TaskBoard.Api.Tests;

public sealed class UnexpectedFailureTests
{
    [Fact]
    public async Task List_StorageThrows_Returns500ProblemWithoutInternals()
    {
        await using var factory = new BoardApiFactory(services =>
            services.AddSingleton<ITaskRepository, FailingTaskRepository>());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tasks", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.DoesNotContain("SQLite", body);
        Assert.DoesNotContain(@"C:\secret", body);
        Assert.DoesNotContain("   at ", body);
    }
}
