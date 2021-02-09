namespace DS.Identity.FIDO.WebAuthn.BrowserContracts
{
    public class PublicKeyCredentialAssertion : PublicKeyCredential
    {
        public AuthenticatorAssertionResponse Response { get; set; }
    }
}