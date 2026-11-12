using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace TaskBoard.Api.Tests;

// One SQLite file per factory instance, so tests never share board state.
public sealed class BoardApiFactory(Action<IServiceCollection>? replaceServices = null)
    : WebApplicationFactory<Program>
{
    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), "task-board-tests", $"{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Board:DatabasePath", _databasePath);
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<TimeProvider>(new SteppingTimeProvider());
            replaceServices?.Invoke(services);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }
}
