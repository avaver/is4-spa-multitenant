namespace DS.Identity.FIDO.WebAuthn.BrowserContracts
{
    public class PublicKeyCredentialAttestation : PublicKeyCredential
    {
        public AuthenticatorAttestationResponse Response { get; set; }
    }
}