using System.Dynamic;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace DS.Identity.WebAuthn.Model
{
    public class CredentialAttestation
    {
        public byte[] CredentialId { get; }
        public string CredentialType { get; }
        public ClientData ClientData { get; }
        
        public AttestationObject Attestation { get; }

        public CredentialAttestation(PublicKeyCredentialAttestation attestation)
        {
            CredentialId = WebEncoders.Base64UrlDecode(attestation.Id);
            CredentialType = attestation.Type;
            
            var json = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(attestation.Response.ClientDataJSON));
            var clientDataJson = JsonSerializer.Deserialize<ClientDataJson>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            ClientData = new ClientData(clientDataJson);
            Attestation = new AttestationObject(WebEncoders.Base64UrlDecode(attestation.Response.AttestationObject));
        }
    }
}