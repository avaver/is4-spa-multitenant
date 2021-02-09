namespace DS.Identity.FIDO.Metadata
{
    public class MetadataDirectoryEntry
    {
        public string Url { get; set; }
        public string TimeOfLastStatusChange { get; set; }
        public string Hash { get; set; }
        public string Aaid { get; set; }
        public string Aaguid { get; set; }
        public MetadataDirectoryEntryStatus[] StatusReports { get; set; }
    }
}