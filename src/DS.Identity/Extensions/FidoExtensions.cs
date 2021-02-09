using System.Text;
using System.Text.Json;
using DS.Identity.FIDO.WebAuthn;
using DS.Identity.FIDO.WebAuthn.BrowserContracts;
using Microsoft.AspNetCore.WebUtilities;

namespace DS.Identity.Extensions
{
    public static class FidoExtensions
    {
        public static CredentialAttestation ToCredentialAttestation(this PublicKeyCredentialAttestation browserObject)
        {
            var credentialId = WebEncoders.Base64UrlDecode(browserObject.Id);
            var clientData = GetClientDataFromBase64UrlString(browserObject.Response.ClientDataJSON);
            var attestationObjectData = WebEncoders.Base64UrlDecode(browserObject.Response.AttestationObject);
            var attestationObject = new AttestationObject(attestationObjectData);

            return new CredentialAttestation(credentialId, browserObject.Type, clientData, attestationObject);
        }

        public static CredentialAssertion ToCredentialAssertion(this PublicKeyCredentialAssertion browserObject)
        {
            var credentialId = WebEncoders.Base64UrlDecode(browserObject.Id);
            var clientData = GetClientDataFromBase64UrlString(browserObject.Response.ClientDataJSON);
            var signature = WebEncoders.Base64UrlDecode(browserObject.Response.Signature);
            var userHandle = browserObject.Response.UserHandle == null ? null : WebEncoders.Base64UrlDecode(browserObject.Response.UserHandle);
            var authenticatorData = new AuthenticatorData(WebEncoders.Base64UrlDecode(browserObject.Response.AuthenticatorData));
            return new CredentialAssertion(credentialId, browserObject.Type, clientData, signature, userHandle, authenticatorData);
        }

        private static ClientData GetClientDataFromBase64UrlString(string base64UrlString)
        {
            var json = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(base64UrlString));
            var clientDataJson = JsonSerializer.Deserialize<ClientDataJson>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var challenge = WebEncoders.Base64UrlDecode(clientDataJson!.Challenge);
            return new ClientData(clientDataJson.Type, clientDataJson.Origin, clientDataJson.CrossOrigin, challenge);
        }
    }
}