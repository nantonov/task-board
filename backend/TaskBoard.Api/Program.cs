using System.Text.Json.Serialization;
using TaskBoard.Api.Application;
using TaskBoard.Api.Endpoints;
using TaskBoard.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var boardSection = builder.Configuration.GetSection(BoardOptions.SectionName);
builder.Services.Configure<BoardOptions>(boardSection);
var uiOrigin = boardSection.Get<BoardOptions>()?.UiOrigin ?? new BoardOptions().UiOrigin;

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<SqliteConnectionFactory>();
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<ITaskRepository, TaskRepository>();
builder.Services.AddSingleton<TaskBoardService>();

builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddCors(options => options.AddPolicy(CorsPolicies.UiRead, policy =>
    policy.WithOrigins(uiOrigin).WithMethods(HttpMethods.Get)));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors(CorsPolicies.UiRead);

await app.Services.GetRequiredService<DatabaseInitializer>().EnsureCreatedAsync(app.Lifetime.ApplicationStopping);

app.MapTaskEndpoints();

app.Run();

public partial class Program;
