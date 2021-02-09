namespace DS.Identity.FIDO.WebAuthn.BrowserContracts
{
    public class AuthenticatorAttestationResponse
    {
        public string ClientDataJSON { get; set; }
        public string AttestationObject { get; set; }
    }
}