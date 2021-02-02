namespace DS.Identity.WebAuthn
{
    public static class AttStmtFormat
    {
        public const string PACKED = "packed";
        public const string FIDOU2F = "fido-u2f";
        public const string TMP = "tpm";
        public const string ANDROIDKEY = "android-key";
        public const string ANDROIDSN = "android-safetynet";
        public const string APPLE = "apple";
        public const string NONE = "none";
    }

    public static class COSE
    {
        public enum KeyCommonParameter
        {
            Reserved = 0,
            KeyType = 1,
            KeyId = 2,
            Algorithm = 3,
            KeyOperations = 4,
            BaseIV = 5
        }

        public enum KeyType
        {
            Reserved = 0,
            OKP = 1,
            EC2 = 2,
            RSA = 3,
            Symmetric = 4
        }

        // OKP: crv, x
        // EC2: crv, x, y
        // RSA: n, e
        // Symmetric: k
        public enum KeyTypeParameter
        {
            // Curve EC identifier
            C = -1,
            // Curve x-coordinate
            X = -2,
            // Curve y-coordinate
            Y = -3,
            // RSA modulus n
            N = -1,
            // RSA public exponent e
            E = -2,
            // Key value
            K = -1
        }

        public enum Algorithm
        {
            RS1 = -65535,
            RS256 = -257,
            RS384 = -258,
            RS512 = -259,
            PS256 = -37,
            PS384 = -38,
            PS512 = -39,
            ES256 = -7,
            ES384 = -35,
            ES512 = -36,
            ES256K = -47,
            EDDSA = -8
        }
        
        public enum Curve
        {
            Reserved = 0,
            P256 = 1,
            P384 = 2,
            P521 = 3,
            X25519 = 4,
            X448 = 5,
            ED25519 = 6,
            ED448 = 7,
            SECP256K1 = 8
        }
    }
}