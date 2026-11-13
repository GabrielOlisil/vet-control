using Microsoft.AspNetCore.Identity;

namespace Auth.Security;

public sealed class PortugueseIdentityErrors : IdentityErrorDescriber
{
    public override IdentityError DuplicateUserName(string userName) => new()
    {
        Code = nameof(DuplicateUserName),
        Description = $"O nome de usuário '{userName}' já está em uso."
    };

    public override IdentityError DuplicateEmail(string email) => new()
    {
        Code = nameof(DuplicateEmail),
        Description = "O e-mail já está em uso."
    };
}
