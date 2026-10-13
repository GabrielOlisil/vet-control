using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public sealed record VetAuthSessionResponse(bool Authenticated, string? Id, string? Name, string? UserName, string? Email, string[] Roles, string? CsrfToken);

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IConfiguration config, IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet("session")]
    [AllowAnonymous]
    public ActionResult<VetAuthSessionResponse> Session()
    {
        Response.Headers.CacheControl = "no-store";
        var authenticated = User.Identity?.IsAuthenticated == true;
        return Ok(new VetAuthSessionResponse(authenticated, User.FindFirstValue("sub"), User.FindFirstValue("name"),
            User.FindFirstValue("preferred_username"), User.FindFirstValue("email"), User.FindAll("role").Select(x => x.Value).ToArray(),
            authenticated ? antiforgery.GetAndStoreTokens(HttpContext).RequestToken : null));
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        var path = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
        return Challenge(new AuthenticationProperties { RedirectUri = config["Authentication:FrontendOrigin"] + path }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpPost("logout")]
    // Antiforgery is validated by the API-wide middleware before reaching this action.
    public IActionResult Logout() => SignOut(new AuthenticationProperties { RedirectUri = config["Authentication:FrontendOrigin"] + "/login" },
        CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
}
