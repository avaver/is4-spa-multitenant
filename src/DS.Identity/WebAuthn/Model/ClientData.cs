using Microsoft.AspNetCore.WebUtilities;

namespace DS.Identity.WebAuthn.Model
{
    public class ClientData
    {
        public string Type { get; set; }
        public string Origin { get; set; }
        public bool CrossOrigin { get; set; }
        public byte[] Challenge { get; set; }

        public ClientData(ClientDataJson clientDataJson)
        {
            Type = clientDataJson.Type;
            Origin = clientDataJson.Origin;
            CrossOrigin = clientDataJson.CrossOrigin.GetValueOrDefault();
            Challenge = WebEncoders.Base64UrlDecode(clientDataJson.Challenge);
        }
    }
}