namespace DS.Identity.WebAuthn
{
    public abstract class PublicKeyCredential
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }
    
    public class PublicKeyCredentialAttestation : PublicKeyCredential
    {
        public AuthenticatorAttestationResponse Response { get; set; }
    }
    
    public class PublicKeyCredentialAssertion : PublicKeyCredential
    {
        public AuthenticatorAssertionResponse Response { get; set; }
    }

    public class AuthenticatorAttestationResponse
    {
        public string ClientDataJSON { get; set; }
        public string AttestationObject { get; set; }
    }
    
    public class AuthenticatorAssertionResponse
    {
        public string ClientDataJSON { get; set; }
        public string AuthenticatorData { get; set; }
        public string Signature { get; set; }
        public string UserHandle { get; set; }
    }

    public class ClientDataJson
    {
        public string Type { get; set; }
        public string Origin { get; set; }
        public bool? CrossOrigin { get; set; }
        public string Challenge { get; set; }
    }
}