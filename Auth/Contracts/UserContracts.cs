using System.ComponentModel.DataAnnotations;

namespace Auth.Contracts;

public sealed record LoginRequest(
    [property: Required, StringLength(256)] string Login,
    [property: Required, StringLength(128)] string Password,
    string? ReturnUrl);
public sealed record PasswordChangeRequest(
    [property: Required] string CurrentPassword,
    [property: Required, StringLength(128, MinimumLength = 10)] string NewPassword);
public sealed record CreateUserRequest(
    [property: Required, StringLength(200)] string FullName,
    [property: Required, StringLength(256), RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Use letras, números, ponto, hífen ou sublinhado no usuário.")] string UserName,
    [property: Required, EmailAddress, StringLength(256)] string Email,
    [property: Required, StringLength(128, MinimumLength = 10)] string Password,
    string[] Roles, bool IsActive = true, bool MustChangePassword = true,
    [property: StringLength(50)] string? Crmv = null,
    [property: StringLength(2, MinimumLength = 2)] string? CrmvUf = null);
public sealed record UpdateUserRequest(
    [property: Required, StringLength(200)] string FullName,
    [property: Required, StringLength(256), RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Use letras, números, ponto, hífen ou sublinhado no usuário.")] string UserName,
    [property: Required, EmailAddress, StringLength(256)] string Email,
    string[] Roles, bool IsActive, [property: Required] string ConcurrencyStamp,
    [property: StringLength(50)] string? Crmv = null,
    [property: StringLength(2, MinimumLength = 2)] string? CrmvUf = null);
public sealed record ResetPasswordRequest(
    [property: Required, StringLength(128, MinimumLength = 10)] string NewPassword,
    [property: Required] string ConcurrencyStamp, bool MustChangePassword = true);
public sealed record VersionRequest([property: Required] string ConcurrencyStamp);
public sealed record UserResponse(Guid Id, string FullName, string UserName, string Email, bool IsActive,
    bool MustChangePassword, bool IsLockedOut, DateTimeOffset? LockoutEnd, DateTimeOffset? LastLoginAt,
    DateTimeOffset CreationDateTime, DateTimeOffset? LastModificationDateTime,
    string ConcurrencyStamp, string[] Roles, string[] EffectiveRoles, string? Crmv, string? CrmvUf);
public sealed record UserPageResponse(UserResponse[] Items, int Total, int Page, int PageSize);
public sealed record AuthSessionResponse(bool Authenticated, Guid? Id, string? FullName, string? UserName,
    string? Email, string[] Roles, bool MustChangePassword, string CsrfToken);
public sealed record LoginResponse(string NextUrl, bool MustChangePassword);
public sealed record SetupStatusResponse(bool Required);
public sealed record InitialSetupRequest(
    [property: Required, StringLength(200)] string FullName,
    [property: Required, StringLength(256), RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Use letras, números, ponto, hífen ou sublinhado no usuário.")] string UserName,
    [property: Required, EmailAddress, StringLength(256)] string Email,
    [property: Required, StringLength(128, MinimumLength = 10)] string Password,
    [property: Required, Compare(nameof(InitialSetupRequest.Password), ErrorMessage = "A confirmação da senha não confere.")] string ConfirmPassword,
    string? ReturnUrl);
public sealed record RoleResponse(string Name, string Description, string[] Includes);
public sealed record AuditEventResponse(Guid Id, string Action, Guid? ActorId, string? ActorName, string? IpAddress,
    string? BeforeJson, string? AfterJson, DateTimeOffset CreationDateTime);
public sealed record AuditPageResponse(AuditEventResponse[] Items, int Total, int Page, int PageSize);

public sealed class RequestValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var argument in context.Arguments.Where(x => x?.GetType().Namespace == typeof(LoginRequest).Namespace))
        {
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(argument!, new ValidationContext(argument!), results, true))
                return Results.ValidationProblem(results.SelectMany(x => x.MemberNames.Select(name => (name, x.ErrorMessage)))
                    .GroupBy(x => x.name).ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage ?? "Valor inválido").ToArray()));
        }
        return await next(context);
    }
}
