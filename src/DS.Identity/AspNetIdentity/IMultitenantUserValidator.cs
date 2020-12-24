using Microsoft.AspNetCore.Identity;

namespace DS.Identity.AspNetIdentity
{
    public interface IMultitenantUserValidator<TUser> : IUserValidator<TUser> where TUser : MultitenantUser
    {
    }
}
