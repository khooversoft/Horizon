using Toolbox.Extensions;

namespace Toolbox.Security;

public class FakeAuthAccess : IAuthAccess
{
    public Task<string> GetDisplayName() => "User Display Name".ToTaskResult();

    public Task<string?> GetEmail() => "user@example.com".ToTaskResult<string?>();

    public Task<string> GetUserName() => "username".ToTaskResult();
}
