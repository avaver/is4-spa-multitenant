namespace DS.Identity.AppIdentity
{
    public class KeyCredential
    {
        public string Id { get; set; }
        public string TenantName { get; set; }
        public string NormalizedTenantName { get; set; }
        public string PublicKey { get; set; }
        public string AttestationCert { get; set; }
        public int SignCount { get; set; }
        public bool IsAdminKey { get; set; }
        public string MetadataName { get; set; }
        public string MetadataIcon { get; set; }
    }
}