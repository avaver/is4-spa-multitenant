using Microsoft.AspNetCore.Identity;

namespace DS.Identity.Multitenancy
{
    public interface IMultitenantUserValidator<TUser> : IUserValidator<TUser> where TUser : MultitenantUser
    {
    }
}
