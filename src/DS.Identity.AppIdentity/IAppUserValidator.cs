using Microsoft.AspNetCore.Identity;

namespace DS.Identity.AppIdentity
{
    public interface IAppUserValidator<TUser> : IUserValidator<TUser> where TUser : AppUser
    {
    }
}
