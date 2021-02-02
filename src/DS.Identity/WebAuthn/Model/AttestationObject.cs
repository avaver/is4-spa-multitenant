using PeterO.Cbor;

namespace DS.Identity.WebAuthn.Model
{
    public class AttestationObject
    {
        public string Format { get; }
        public AttestationStatement AttestationStatement { get; }
        public AuthenticatorData AuthenticatorData { get; }

        public AttestationObject(byte[] data)
        {
            var cbor = CBORObject.DecodeFromBytes(data);
            Format = cbor["fmt"].AsString();
            AttestationStatement = new AttestationStatement(cbor["attStmt"], Format);
            AuthenticatorData = new AuthenticatorData(cbor["authData"].ToObject<byte[]>());
        }
    }
}