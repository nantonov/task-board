using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskBoard.Mcp;

var builder = Host.CreateApplicationBuilder(args);

// stdout is the MCP protocol channel; every log line must go to stderr.
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

var apiUrl = builder.Configuration["BOARD_API_URL"] ?? "http://127.0.0.1:5080/";
builder.Services.AddSingleton(new HttpClient
{
    BaseAddress = new Uri(apiUrl.EndsWith('/') ? apiUrl : apiUrl + "/"),
    Timeout = TimeSpan.FromSeconds(5),
});
builder.Services.AddSingleton<BoardApiClient>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
