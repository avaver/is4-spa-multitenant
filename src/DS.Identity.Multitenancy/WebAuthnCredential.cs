namespace DS.Identity.Multitenancy
{
    public class WebAuthnCredential
    {
        public string Id { get; set; }
        public string MetadataName { get; set; }
        public string MetadataIcon { get; set; }
        public string PublicKey { get; set; }
        public string AttestationCert { get; set; }
        public int SignCount { get; set; }
    }
}