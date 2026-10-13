using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Api.Security;

public sealed class VetSessionValidator(IHttpClientFactory clients, IConfiguration config, ServerTicketStore store)
{
    public async Task Validate(CookieValidatePrincipalContext context)
    {
        var session = store.Find(context.Properties.GetString("session-key") ?? "");
        if (session is null) { context.RejectPrincipal(); return; }
        await session.Gate.WaitAsync(context.HttpContext.RequestAborted);
        try
        {
            // Another concurrent request may have rotated the refresh token while this request waited.
            var latest = store.Find(context.Properties.GetString("session-key") ?? "");
            if (latest is null) { context.RejectPrincipal(); return; }
            var properties = latest.Ticket.Properties;
            var client = clients.CreateClient("identity");
            var issuer = config["Authentication:Authority"]!.TrimEnd('/');
            var expiry = DateTimeOffset.TryParse(properties.GetTokenValue("expires_at"), CultureInfo.InvariantCulture, out var parsed) ? parsed : DateTimeOffset.MinValue;
            if (expiry <= DateTimeOffset.UtcNow.AddSeconds(60))
            {
                using var refresh = await client.PostAsync(issuer + "/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token", ["refresh_token"] = properties.GetTokenValue("refresh_token") ?? "",
                    ["client_id"] = config["Authentication:ClientId"]!, ["client_secret"] = config["Authentication:ClientSecret"]!
                }), context.HttpContext.RequestAborted);
                if ((int)refresh.StatusCode >= 500) refresh.EnsureSuccessStatusCode();
                if (!refresh.IsSuccessStatusCode) { await Reject(context); return; }
                using var tokens = JsonDocument.Parse(await refresh.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
                var root = tokens.RootElement;
                var values = properties.GetTokens().ToDictionary(x => x.Name, x => x.Value);
                values["access_token"] = root.GetProperty("access_token").GetString()!;
                if (root.TryGetProperty("refresh_token", out var rt)) values["refresh_token"] = rt.GetString()!;
                if (root.TryGetProperty("id_token", out var id)) values["id_token"] = id.GetString()!;
                values["expires_at"] = DateTimeOffset.UtcNow.AddSeconds(root.GetProperty("expires_in").GetInt32()).ToString("o", CultureInfo.InvariantCulture);
                properties.StoreTokens(values.Select(x => new AuthenticationToken { Name = x.Key, Value = x.Value }));
            }
            using var request = new HttpRequestMessage(HttpMethod.Get, issuer + "/connect/userinfo");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", properties.GetTokenValue("access_token"));
            using var response = await client.SendAsync(request, context.HttpContext.RequestAborted);
            if ((int)response.StatusCode >= 500) response.EnsureSuccessStatusCode();
            if (!response.IsSuccessStatusCode) { await Reject(context); return; }
            using var data = JsonDocument.Parse(await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
            var subject = data.RootElement.GetProperty("sub").GetString()!;
            if (subject != context.Principal?.FindFirstValue("sub")) { await Reject(context); return; }
            var identity = new ClaimsIdentity("VetSession", "name", "role");
            foreach (var claim in data.RootElement.EnumerateObject())
            {
                if (claim.Value.ValueKind == JsonValueKind.Array)
                    foreach (var item in claim.Value.EnumerateArray()) identity.AddClaim(new Claim(claim.Name, item.GetString()!));
                else if (claim.Value.ValueKind == JsonValueKind.String) identity.AddClaim(new Claim(claim.Name, claim.Value.GetString()!));
            }
            var principal = new ClaimsPrincipal(identity);
            context.ReplacePrincipal(principal);
            context.Properties.StoreTokens(properties.GetTokens());
            await store.RenewAsync(context.Properties.GetString("session-key")!, new AuthenticationTicket(principal, context.Properties, context.Scheme.Name));
        }
        finally { session.Gate.Release(); }
    }
    private static async Task Reject(CookieValidatePrincipalContext context)
    { context.RejectPrincipal(); await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); }
}
