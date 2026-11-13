namespace Api.Security;

/// <summary>
/// Connects server-to-server OIDC calls to the container address while discovery,
/// redirects and token issuer validation retain the public authority.
/// </summary>
public sealed class AuthorityBackchannelHandler : DelegatingHandler
{
    private readonly Uri authority;
    private readonly Uri destination;

    public AuthorityBackchannelHandler(Uri authority, Uri destination)
    {
        Validate(authority, nameof(authority));
        Validate(destination, nameof(destination));
        this.authority = authority;
        this.destination = destination;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri;
        var prefix = authority.AbsolutePath.TrimEnd('/') + "/";
        if (uri is not null && uri.IsAbsoluteUri &&
            uri.Scheme == authority.Scheme && uri.Host == authority.Host && uri.Port == authority.Port &&
            uri.AbsolutePath.StartsWith(prefix, StringComparison.Ordinal))
        {
            // OpenIddict builds discovery endpoint URLs from the HTTP Host header.
            // Keep the public host even though the TCP destination is internal.
            request.Headers.Host = uri.Authority;
            request.RequestUri = new UriBuilder(uri)
            {
                Scheme = destination.Scheme,
                Host = destination.Host,
                Port = destination.Port,
                Path = destination.AbsolutePath.TrimEnd('/') + "/" + uri.AbsolutePath[prefix.Length..]
            }.Uri;
        }
        return base.SendAsync(request, cancellationToken);
    }

    private static void Validate(Uri uri, string parameter)
    {
        if (!uri.IsAbsoluteUri || uri.Scheme is not ("http" or "https") ||
            !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new ArgumentException("Use uma URL HTTP ou HTTPS absoluta, sem credenciais, query ou fragmento.", parameter);
    }
}
