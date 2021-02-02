using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace DS.Identity.Services
{
    public class AuthenticatorInfo
    {
        public string Description { get; set; }
        public string Icon { get; set; }
    }

    public class MetadataDirectoryEntryStatus
    {
        public string Status { get; set; }
        public string EffectiveDate { get; set; }
        public string Certificate { get; set; }
        public string CertificateNumber { get; set; }
        public string CertificationDescriptor { get; set; }
        public string CertificationPolicyVersion { get; set; }
        public string CertificationRequirementsVersion { get; set; }
        public string Url { get; set; }
    }

    public class MetadataDirectoryEntry
    {
        public string Url { get; set; }
        public string TimeOfLastStatusChange { get; set; }
        public string Hash { get; set; }
        public string Aaid { get; set; }
        public string Aaguid { get; set; }
        public MetadataDirectoryEntryStatus[] StatusReports { get; set; }
    }
    
    public class MetadataDirectory
    {
        public string NextUpdate { get; set; }
        public string LegalHeader { get; set; }
        public int No { get; set; }
        public MetadataDirectoryEntry[] Entries { get; set; }
    }

    public class KeyMetadataService
    {
        private const string AccessToken = "c3381747eaf0613766d47efc9e5597af1978a9a9dcc36989";
        
        private MetadataDirectory _metadata;
        private readonly ILogger<KeyMetadataService> _logger;
        private readonly ConcurrentDictionary<Guid, AuthenticatorInfo> _cache = new();

        public KeyMetadataService(ILogger<KeyMetadataService> logger)
        {
            _logger = logger;
        }

        public async Task Init()
        {
            _logger.LogInformation("Metadata service initialization");
            using var client = new HttpClient();
            var jwt = await client.GetStringAsync("https://mds2.fidoalliance.org/?token=" + AccessToken);
            var token = new JwtSecurityToken(jwt);
            _metadata = new MetadataDirectory
            {
                Entries = JsonSerializer.Deserialize<MetadataDirectoryEntry[]>(
                    token.Payload["entries"].ToString() ?? string.Empty, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
                No = int.Parse(token.Payload["no"].ToString() ?? "0"),
                NextUpdate = token.Payload["nextUpdate"].ToString(),
                LegalHeader = token.Payload["legalHeader"].ToString()
            };
            _logger.LogInformation("Metadata directory loaded. Entries: {0}", _metadata?.Entries?.Length);
        }

        public async Task<AuthenticatorInfo> GetMetadataAsync(Guid aaguid)
        {
            _logger.LogInformation("Metadata requested for aaguid {0}", aaguid);
            if (_cache.TryGetValue(aaguid, out AuthenticatorInfo authenticatorInfo))
            {
                _logger.LogInformation("Found cached metadata for aaguid {0}", aaguid);
                return authenticatorInfo;
            }

            var entry = _metadata.Entries?.Where(e => !string.IsNullOrEmpty(e.Aaguid) && new Guid(e.Aaguid) == aaguid).SingleOrDefault();
            if (entry == null)
            {
                _logger.LogInformation("Metadata for aaguid {0} not found", aaguid);
                return null;
            }

            _logger.LogInformation("Found metadata entry for aaguid {0}, loading full object from {1}", aaguid, entry.Url);
            using var client = new HttpClient();
            var jwt = await client.GetStringAsync(entry.Url + "/?token=" + AccessToken);
            var json = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(jwt));
            var info = JsonSerializer.Deserialize<AuthenticatorInfo>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            _cache.TryAdd(aaguid, info);
            _logger.LogInformation("Authenticator aaguid {0} matched \"{1}\"", aaguid, info?.Description);
            return info;
        }
    }
}