using System;
using System.Linq;
using System.Threading.Tasks;
using DS.Identity.FIDO.WebAuthn.BrowserContracts;
using DS.Identity.AppIdentity;
using DS.Identity.Extensions;
using DS.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace DS.Identity.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        public class CreateKeyRequest
        {
            public PublicKeyCredentialAttestation Credential { get; set; }
            public bool IsAdminKey { get; set; }
        }

        public class UserDto
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public bool? IsAdmin { get; set; }
            public bool? IsLocked { get; set; }
        }

        private readonly AppUserManager _userManager;
        private readonly KeyCredentialManager _keyManager;
        private readonly FidoMetadataService _metadataService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppUserManager userManager, KeyCredentialManager keyManager, AppIdentityDbContext context, FidoMetadataService metadataService, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _keyManager = keyManager;
            _metadataService = metadataService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult AuthCookieClaims()
        {
            _logger.LogInformation("Requested claims for user {0} (tenant: {1})", User.Identity?.Name, User.Tenant());
            return Ok(User.Claims
                .OrderBy(c => c.Type)
                .ToDictionary(c => c.Type, c => c.Value));
        }

        [HttpGet]
        public async Task<IActionResult> Keys()
        {
            _logger.LogInformation("Requested authentication keys for tenant {0} by {1}", User.Tenant(), User.Identity?.Name);
            var keys = await _keyManager.GetTenantKeysAsync(User.Tenant());
            return Ok(keys);
        }

        [HttpPost]
        [Authorize(Constants.ClinicAdminPolicy)]
        public async Task<IActionResult> Keys([FromBody] CreateKeyRequest request)
        {
            _logger.LogInformation("Adding {0} authentication key {1} to tenant {2} by {3}", request.IsAdminKey ? "admin" : "device", request.Credential.Id, User.Tenant(), User.Identity?.Name);
            var credentialAttestation = request.Credential.ToCredentialAttestation();
            var metadata = await _metadataService.GetMetadataAsync(credentialAttestation.Attestation.AuthenticatorData.AttestedCredentialData.Aaguid);
            var credential = new KeyCredential
            {
                Id = WebEncoders.Base64UrlEncode(credentialAttestation.CredentialId),
                PublicKey = WebEncoders.Base64UrlEncode(credentialAttestation.Attestation.AuthenticatorData
                    .AttestedCredentialData.PublicKey.Cbor.EncodeToBytes()),
                MetadataName = metadata?.Description,
                MetadataIcon = metadata?.Icon,
                TenantName = User.Tenant(),
                IsAdminKey = request.IsAdminKey
            };
            await _keyManager.CreateAsync(credential);
            _logger.LogInformation("Registered {0} authentication key {1}", request.IsAdminKey ? "admin" : "device", request.Credential.Id);
            return Ok(metadata);
        }

        [HttpDelete("{id}")]
        [Authorize(Constants.ClinicAdminPolicy)]
        public async Task<IActionResult> Keys(string id)
        {
            _logger.LogInformation("Deleting authentication key {0} from tenant {1} by {2}", id, User.Tenant(), User.Identity?.Name);
            var key = await _keyManager.FindByIdAsync(id);
            if (key == null)
            {
                return NotFound();
            }

            await _keyManager.DeleteAsync(key);
            return Ok();
        }

        [HttpGet]
        [Authorize(Constants.ClinicAdminPolicy)]
        public async Task<IActionResult> Users()
        {
            _logger.LogInformation("Requested users list for tenant {0} by {1}", User.Tenant(), User.Identity?.Name);
            var users = await _userManager.FindByTenantAsync(User.Tenant());
            return Ok(users.Select(u => new UserDto { Username = u.UserName, IsAdmin = u.IsClinicAdmin, IsLocked = DateTimeOffset.UtcNow < (u.LockoutEnd ?? DateTimeOffset.MinValue) }));
        }

        [HttpPost]
        [Authorize(Constants.ClinicAdminPolicy)]
        public async Task<IActionResult> Users([FromBody] UserDto request)
        {
            var user = new AppUser
            {
                TenantName = User.Tenant(),
                UserName = request.Username,
                IsClinicAdmin = request.IsAdmin.GetValueOrDefault(),
                LockoutEnd = request.IsLocked.GetValueOrDefault() ? DateTimeOffset.MaxValue : DateTimeOffset.MinValue
            };
            await _userManager.CreateAsync(user, request.Password);
            
            return Ok();
        }

        [HttpPatch("{username}")]
        [Authorize(Constants.ClinicAdminPolicy)]
        public async Task<IActionResult> Users(string username, [FromBody] UserDto request)
        {
            _logger.LogInformation("Updating user {0} in tenant {1} by {2}", username, User.Tenant(), User.Identity?.Name);
            var user = await _userManager.FindByNameAndTenantAsync(username, User.Tenant());
            if (user == null)
            {
                return NotFound();
            }
            
            if (!string.IsNullOrEmpty(request.Password))
            {
                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, request.Password);
            }

            if (request.IsLocked.HasValue)
            {
                user.LockoutEnd = request.IsLocked.Value ? DateTimeOffset.MaxValue : DateTimeOffset.MinValue;
            }

            if (request.IsAdmin.HasValue)
            {
                user.IsClinicAdmin = request.IsAdmin.Value;
            }
            await _userManager.UpdateAsync(user);

            return Ok();
        }
    }
}