namespace DS.Identity.FIDO.WebAuthn.BrowserContracts
{
    public class ClientDataJson
    {
        public string Type { get; set; }
        public string Origin { get; set; }
        public bool? CrossOrigin { get; set; }
        public string Challenge { get; set; }
    }
}