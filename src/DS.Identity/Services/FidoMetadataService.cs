using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DS.Identity.FIDO.Metadata;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DS.Identity.Services
{
    public class FidoMetadataService
    {
        private readonly string _accessToken;
        private MetadataDirectory _metadata;
        private readonly ILogger<FidoMetadataService> _logger;
        private readonly ConcurrentDictionary<Guid, AuthenticatorInfo> _cache = new();

        public FidoMetadataService(IConfiguration configuration, ILogger<FidoMetadataService> logger)
        {
            _accessToken = configuration.GetValue<string>("MetadataServiceToken");
            _logger = logger;
        }

        public async Task Init()
        {
            using var client = new HttpClient();
            var jwt = await client.GetStringAsync("https://mds2.fidoalliance.org/?token=" + _accessToken);
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
            
            if (_metadata == null)
            {
                _logger.LogError("KeyMetadataService is not initialized. Call KeyMetadataService.Init() before requesting metadata.");
                return null;
            }

            if (_cache.TryGetValue(aaguid, out AuthenticatorInfo authenticatorInfo))
            {
                _logger.LogDebug("Found cached metadata for aaguid {0}: {1}", aaguid, authenticatorInfo.Description);
                return authenticatorInfo;
            }

            var entry = _metadata.Entries?.Where(e => !string.IsNullOrEmpty(e.Aaguid) && new Guid(e.Aaguid) == aaguid).SingleOrDefault();
            if (entry == null)
            {
                _logger.LogInformation("Metadata for aaguid {0} not found", aaguid);
                return null;
            }

            _logger.LogDebug("Found metadata entry for aaguid {0}, loading full object from {1}", aaguid, entry.Url);
            using var client = new HttpClient();
            var jwt = await client.GetStringAsync(entry.Url + "/?token=" + _accessToken);
            var json = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(jwt));
            var info = JsonSerializer.Deserialize<AuthenticatorInfo>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            _cache.TryAdd(aaguid, info);

            _logger.LogDebug("Loaded metadata for aaguid {0}: {1}", aaguid, info?.Description);
            return info;
        }
    }
}