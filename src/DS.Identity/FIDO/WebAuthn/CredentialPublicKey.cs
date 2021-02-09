using System;
using PeterO.Cbor;

namespace DS.Identity.FIDO.WebAuthn
{
    public class CredentialPublicKey
    {
        public readonly CBORObject Cbor;
        public COSE.KeyType KeyType { get; }
        public COSE.Algorithm HashAlgorithm { get; }

        public COSE.Curve Curve { get; }
        public byte[] X { get; }
        public byte[] Y { get; }
        
        public byte[] N { get; }
        public byte[] E { get; }

        public CredentialPublicKey(CBORObject cbor)
        {
            Cbor = cbor;
            KeyType = (COSE.KeyType) Cbor[CBORObject.FromObject(COSE.KeyCommonParameter.KeyType)].AsInt32();
            HashAlgorithm = (COSE.Algorithm) Cbor[CBORObject.FromObject(COSE.KeyCommonParameter.Algorithm)].AsInt32();
            switch (KeyType)
            {
                case COSE.KeyType.EC2:
                    Curve = (COSE.Curve) Cbor[CBORObject.FromObject(COSE.KeyTypeParameter.C)].AsInt32();
                    X = Cbor[CBORObject.FromObject(COSE.KeyTypeParameter.X)].GetByteString();
                    Y = Cbor[CBORObject.FromObject(COSE.KeyTypeParameter.Y)].GetByteString();
                    break;
                case COSE.KeyType.Symmetric:
                    N = Cbor[CBORObject.FromObject(COSE.KeyTypeParameter.N)].GetByteString();
                    E = Cbor[CBORObject.FromObject(COSE.KeyTypeParameter.E)].GetByteString();
                    break;
                default:
                    throw new NotSupportedException("Unsupported public key type " + Enum.GetName(KeyType));
            }
        }

        public CredentialPublicKey(byte[] data) : this(CBORObject.DecodeFromBytes(data))
        {
        }
    }
}