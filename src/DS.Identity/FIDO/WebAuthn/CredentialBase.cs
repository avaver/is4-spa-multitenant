namespace DS.Identity.FIDO.WebAuthn
{
    public abstract class CredentialBase
    {
        public byte[] CredentialId { get; }
        public string CredentialType { get; }
        public ClientData ClientData { get; }


        public CredentialBase(byte[] credentialId, string credentialType, ClientData clientData)
        {
            CredentialId = credentialId;
            CredentialType = credentialType;
            ClientData = clientData;
        }
    }
}