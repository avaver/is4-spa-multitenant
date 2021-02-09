namespace DS.Identity.FIDO.WebAuthn
{
    public class CredentialAttestation: CredentialBase
    {
        public AttestationObject Attestation { get; }

        public CredentialAttestation(byte[] credentialId, string credentialType, ClientData clientData,
            AttestationObject attestation) : base(credentialId, credentialType, clientData)
        {
            Attestation = attestation;
        }
    }
}