namespace DS.Identity.FIDO.Metadata
{
    public class MetadataDirectoryEntryStatus
    {
        public string Status { get; set; }
        public string EffectiveDate { get; set; }
        public string Certificate { get; set; }
        public string CertificateNumber { get; set; }
        public string CertificationDescriptor { get; set; }
        public string CertificationPolicyVersion { get; set; }
        public string CertificationRequirementsVersion { get; set; }
        public string Url { get; set; }
    }
}