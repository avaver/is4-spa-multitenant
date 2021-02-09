namespace DS.Identity.FIDO.WebAuthn
{
    public class ClientData
    {
        public string Type { get; }
        public string Origin { get; }
        public bool? CrossOrigin { get; }
        public byte[] Challenge { get; }

        public ClientData(string type, string origin, bool? crossOrigin, byte[] challenge)
        {
            Type = type;
            Origin = origin;
            CrossOrigin = crossOrigin;
            Challenge = challenge;
        }
    }
}