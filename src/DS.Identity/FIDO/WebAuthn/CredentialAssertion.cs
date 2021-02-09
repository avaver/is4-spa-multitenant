namespace DS.Identity.FIDO.WebAuthn
{
    public class CredentialAssertion: CredentialBase
    {
        public byte[] Signature { get; }
        public byte[] UserHandle { get; }
        
        public AuthenticatorData AuthenticatorData { get; }

        public CredentialAssertion(byte[] credentialId, string credentialType, ClientData clientData, byte[] signature,
            byte[] userHandle, AuthenticatorData authenticatorData) : base(credentialId, credentialType, clientData)
        {
            Signature = signature;
            UserHandle = userHandle;
            AuthenticatorData = authenticatorData;
        }
    }
}