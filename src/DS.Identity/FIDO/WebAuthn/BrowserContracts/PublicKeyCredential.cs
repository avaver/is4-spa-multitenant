namespace DS.Identity.FIDO.WebAuthn.BrowserContracts
{
    public abstract class PublicKeyCredential
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }
}