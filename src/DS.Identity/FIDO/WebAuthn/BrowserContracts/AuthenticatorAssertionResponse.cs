namespace DS.Identity.FIDO.WebAuthn.BrowserContracts
{
    public class AuthenticatorAssertionResponse
    {
        public string ClientDataJSON { get; set; }
        public string AuthenticatorData { get; set; }
        public string Signature { get; set; }
        public string UserHandle { get; set; }
    }
}