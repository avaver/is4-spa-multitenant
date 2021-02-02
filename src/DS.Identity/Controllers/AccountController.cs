using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using DS.Identity.Multitenancy;
using DS.Identity.Services;
using DS.Identity.WebAuthn;
using DS.Identity.WebAuthn.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
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
            public bool Admin { get; set; }
        }
        
        private readonly ILogger<AccountController> _logger;
        private readonly MultitenantIdentityDbContext _context;
        private readonly KeyMetadataService _metadataService;
        
        public AccountController(ILogger<AccountController> logger, MultitenantIdentityDbContext context, KeyMetadataService metadataService)
        {
            _logger = logger;
            _context = context;
            _metadataService = metadataService;
        }

        [HttpGet]
        public IActionResult AuthCookieClaims()
        {
            _logger.LogInformation("AuthCookie claims requested");
            return Ok(User.Claims
                .OrderBy(c => c.Type)
                .ToDictionary(c => c.Type, c => c.Value));
        }

        [HttpGet]
        public async Task<IActionResult> Tenant()
        {
            _logger.LogInformation("Tenant information requested");
            var tenant = await GetTenant();
            return Ok(tenant);
        }

        [HttpPost]
        [Authorize(Constants.DsClinicAdminPolicy)]
        public async Task<IActionResult> Key([FromBody] CreateKeyRequest request)
        {
            var credentialAttestation = new CredentialAttestation(request.Credential);
            var metadata = await _metadataService.GetMetadataAsync(credentialAttestation.Attestation.AuthenticatorData.AttestedCredentialData.Aaguid);
            var key = new WebAuthnCredential
            {
                Id = WebEncoders.Base64UrlEncode(credentialAttestation.CredentialId),
                PublicKey = WebEncoders.Base64UrlEncode(credentialAttestation.Attestation.AuthenticatorData
                    .AttestedCredentialData.PublicKey.Cbor.EncodeToBytes()),
                MetadataName = metadata?.Description,
                MetadataIcon = metadata?.Icon
            };
            var tenant = await GetTenant();
            tenant.Keys.Add(key);
            if (request.Admin)
            {
                if (!string.IsNullOrEmpty(tenant.AdminKeyId))
                {
                    await DeleteKeyAsync(tenant.AdminKeyId);
                }
                tenant.AdminKeyId = key.Id;
            }
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Registered {0} key {1}", request.Admin ? "admin" : "device", request.Credential.Id);
            return new JsonResult(metadata);
        }

        [HttpDelete("{id}")]
        [Authorize(Constants.DsClinicAdminPolicy)]
        public async Task<IActionResult> Key(string id)
        {
            await DeleteKeyAsync(id);
            return Ok();
        }

        private async Task<Tenant> GetTenant()
        {
            var tenantName = User.FindFirstValue(MultitenantClaimTypes.Tenant);
            var tenant = (await _context.Tenants
                .Include(t => t.Keys)
                .Where(t => t.Name == tenantName)
                .ToListAsync()).Single();
            return tenant;
        }

        private async Task DeleteKeyAsync(string id)
        {
            var tenant = await GetTenant();
            var key = tenant.Keys.SingleOrDefault(k => k.Id == id);
            if (key != null)
            {
                tenant.Keys.Remove(key);
                if (tenant.AdminKeyId == key.Id)
                {
                    tenant.AdminKeyId = null;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}