using System.Net;
using System.Text;

namespace TaskBoard.Mcp.Tests;

// Stands in for the board backend at the HTTP boundary: records what the tools sent and
// answers with a canned status and body.
public sealed class StubBackend(HttpStatusCode status, string body, string mediaType = "application/json")
    : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    public string? LastRequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, mediaType) };
    }
}
