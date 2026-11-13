extern alias VetApi;
using System.Net;
using System.Text;
using Xunit;
using AuthorityBackchannelHandler = VetApi::Api.Security.AuthorityBackchannelHandler;

namespace Api.Tests;

public sealed class AuthorityBackchannelTests
{
    [Theory]
    [InlineData("http://localhost:8081/.well-known/openid-configuration", "http://auth:8081/.well-known/openid-configuration")]
    [InlineData("http://localhost:8081/connect/token?code=a%2Fb&state=c", "http://auth:8081/connect/token?code=a%2Fb&state=c")]
    [InlineData("http://localhost:8081/connect/userinfo", "http://auth:8081/connect/userinfo")]
    [InlineData("http://third-party.test/connect/token", "http://third-party.test/connect/token")]
    [InlineData("https://localhost:8081/connect/token", "https://localhost:8081/connect/token")]
    [InlineData("http://localhost:8082/connect/token", "http://localhost:8082/connect/token")]
    [InlineData("http://localhost.evil.test:8081/connect/token", "http://localhost.evil.test:8081/connect/token")]
    public async Task OnlyPublicAuthorityRequestsUseTheInternalAddress(string url, string expected)
    {
        using var recorder = new RecordingHandler();
        using var handler = new AuthorityBackchannelHandler(new Uri("http://localhost:8081/"), new Uri("http://auth:8081/"))
        { InnerHandler = recorder };
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StringContent("grant_type=refresh_token") };
        request.Headers.Authorization = new("Bearer", "test-token");
        using var response = await client.SendAsync(request);
        Assert.Equal(expected, recorder.Uri?.AbsoluteUri);
        Assert.Equal(url == expected ? null : new Uri(url).Authority, recorder.Host);
        Assert.Equal(HttpMethod.Post, recorder.Method);
        Assert.Equal("grant_type=refresh_token", recorder.Body);
        Assert.Equal("Bearer test-token", recorder.Authorization);
        // The discovery response's public issuer and browser endpoints are never rewritten.
        Assert.Equal(RecordingHandler.Discovery, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("http://localhost:8081/identity/connect/token", "http://auth:8081/internal/connect/token")]
    [InlineData("http://localhost:8081/identity-other/connect/token", "http://localhost:8081/identity-other/connect/token")]
    public async Task AuthorityPathIsRespected(string url, string expected)
    {
        using var recorder = new RecordingHandler();
        using var client = new HttpClient(new AuthorityBackchannelHandler(new Uri("http://localhost:8081/identity/"), new Uri("http://auth:8081/internal/"))
        { InnerHandler = recorder });
        await client.GetAsync(url);
        Assert.Equal(expected, recorder.Uri?.AbsoluteUri);
        Assert.Equal(url == expected ? null : new Uri(url).Authority, recorder.Host);
    }

    [Theory]
    [InlineData("file:///tmp/identity")]
    [InlineData("http://user:password@auth:8081/")]
    [InlineData("http://auth:8081/?query=true")]
    [InlineData("http://auth:8081/#fragment")]
    [InlineData("relative")]
    public void InvalidAddressesAreRejected(string url)
    {
        var invalid = new Uri(url, UriKind.RelativeOrAbsolute);
        var valid = new Uri("http://localhost:8081/");
        Assert.Throws<ArgumentException>(() => new AuthorityBackchannelHandler(valid, invalid));
        Assert.Throws<ArgumentException>(() => new AuthorityBackchannelHandler(invalid, valid));
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public const string Discovery = "{\"issuer\":\"http://localhost:8081/\",\"authorization_endpoint\":\"http://localhost:8081/connect/authorize\"}";
        public Uri? Uri { get; private set; }
        public HttpMethod? Method { get; private set; }
        public string? Body { get; private set; }
        public string? Authorization { get; private set; }
        public string? Host { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Uri = request.RequestUri;
            Method = request.Method;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            Authorization = request.Headers.Authorization?.ToString();
            Host = request.Headers.Host;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Discovery, Encoding.UTF8, "application/json") };
        }
    }
}
