using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DS.Identity.Multitenancy
{
    public class MultitenantUserValidator : IMultitenantUserValidator<MultitenantUser>
    {
        public MultitenantUserValidator(IdentityErrorDescriber errors = null)
        {
            Describer = errors ?? new IdentityErrorDescriber();
        }

        private IdentityErrorDescriber Describer { get; set; }

        public virtual async Task<IdentityResult> ValidateAsync(UserManager<MultitenantUser> manager, MultitenantUser user)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var multitenantManager = manager as MultitenantUserManager;
            if (multitenantManager == null)
            {
                throw new ArgumentException("MultitenantUserValidator can only be used with MultitenantUserManager", nameof(manager));
            }

            var errors = new List<IdentityError>();
            await ValidateUserName(multitenantManager, user, errors);
            if (multitenantManager.Options.User.RequireUniqueEmail)
            {
                await ValidateEmail(multitenantManager, user, errors);
            }
            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        private async Task ValidateUserName(MultitenantUserManager manager, MultitenantUser user, ICollection<IdentityError> errors)
        {
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                errors.Add(Describer.InvalidUserName(user.UserName));
            }
            else if (!string.IsNullOrEmpty(manager.Options.User.AllowedUserNameCharacters) &&
                user.UserName.Any(c => !manager.Options.User.AllowedUserNameCharacters.Contains(c)))
            {
                errors.Add(Describer.InvalidUserName(user.UserName));
            }
            else
            {
                var owner = await manager.FindByNameAndTenantAsync(user.UserName, user.TenantName);
                if (owner != null && !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user)))
                {
                    errors.Add(Describer.DuplicateUserName(user.UserName));
                }
            }
        }

        private async Task ValidateEmail(MultitenantUserManager manager, MultitenantUser user, List<IdentityError> errors)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                errors.Add(Describer.InvalidEmail(user.Email));
                return;
            }
            if (!new EmailAddressAttribute().IsValid(user.Email))
            {
                errors.Add(Describer.InvalidEmail(user.Email));
                return;
            }
            var owner = await manager.FindByEmailAndTenantAsync(user.Email, user.TenantName);
            if (owner != null &&
                !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user)))
            {
                errors.Add(Describer.DuplicateEmail(user.Email));
            }
        }
    }
}
