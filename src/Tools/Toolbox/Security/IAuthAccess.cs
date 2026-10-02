namespace Toolbox.Security;

public interface IAuthAccess
{
    Task<string> GetDisplayName();
    Task<string> GetUserName();
    Task<string?> GetEmail();
}

