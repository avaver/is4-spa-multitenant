namespace DS.Identity.FIDO.Metadata
{
    public class MetadataDirectory
    {
        public string NextUpdate { get; set; }
        public string LegalHeader { get; set; }
        public int No { get; set; }
        public MetadataDirectoryEntry[] Entries { get; set; }
    }
}