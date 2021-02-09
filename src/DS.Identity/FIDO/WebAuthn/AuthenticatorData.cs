using System;

namespace DS.Identity.FIDO.WebAuthn
{
    public class AuthenticatorDataFlags
    {
        public bool UserPresent { get; }
        public bool UserVerified { get; }
        public bool AttestedCredentialData { get; }
        public bool ExtensionData { get; }

        public AuthenticatorDataFlags(byte data)
        {
            UserPresent = (data & 0x01) == 0x01;
            UserVerified = (data & 0x04) == 0x04;
            AttestedCredentialData = (data & 0x40) == 0x40;
            ExtensionData = (data & 0x80) == 0x80;
        }
    }

    public class AttestedCredentialData
    {
        public Guid Aaguid { get; }
        public byte[] CredentialId { get; }
        public CredentialPublicKey PublicKey { get; }
            
        public AttestedCredentialData(byte[] data)
        {
            var guid = new byte[16];
            Buffer.BlockCopy(data, 0, guid, 0, 16);
            Aaguid = new Guid(BitConverter.ToString(guid).Replace("-", ""));

            var credIdLen = BitConverter.ToUInt16(new[] {data[17], data[16]}, 0);
            CredentialId = new byte[credIdLen];
            Buffer.BlockCopy(data, 18, CredentialId, 0, credIdLen);

            var cborData = new byte[data.Length - 18 - credIdLen];
            Buffer.BlockCopy(data, 18 + credIdLen, cborData, 0, cborData.Length);
            PublicKey = new CredentialPublicKey(cborData);
        }
    }

    public class AuthenticatorData
    {
        public byte[] RpIdHash { get; }
        
        public AuthenticatorDataFlags Flags { get; }
        public int Counter { get; }
        public AttestedCredentialData AttestedCredentialData { get; }

        public AuthenticatorData(byte[] data)
        {
            RpIdHash = new byte[32];
            Buffer.BlockCopy(data, 0, RpIdHash, 0, 32);
            Flags = new AuthenticatorDataFlags(data[32]);
            Counter = BitConverter.ToInt32(new[] { data[36], data[35], data[34], data[33] }, 0);
            if (Flags.AttestedCredentialData)
            {
                var acd = new byte[data.Length - 37];
                Buffer.BlockCopy(data, 37, acd, 0, data.Length - 37);
                AttestedCredentialData = new AttestedCredentialData(acd);
            }
        }
    }
}