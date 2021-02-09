using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using PeterO.Cbor;

namespace DS.Identity.FIDO.WebAuthn
{
    public class AttestationStatement
    {
        public COSE.Algorithm Algorithm { get; private set; }
        public byte[] Signature { get; private set; }
        public X509Certificate2[] Certificates { get; private set; }
        
        public AttestationStatement(CBORObject attestationStatement, string format)
        {
            switch (format)
            {
                case AttStmtFormat.PACKED:
                    GetAlgorithm(attestationStatement);
                    GetSignature(attestationStatement);
                    GetCertificates(attestationStatement);
                    break;
                case AttStmtFormat.FIDOU2F:
                    GetSignature(attestationStatement);
                    GetCertificates(attestationStatement);
                    break;
                case AttStmtFormat.APPLE:
                    GetCertificates(attestationStatement);
                    break;
                default:
                    throw new NotSupportedException($"Attestation statement format {format} is not supported");
            }
        }

        public void Verify()
        {
            
        }

        private void GetAlgorithm(CBORObject statement)
        {
            if (statement.ContainsKey("alg"))
            {
                Algorithm = (COSE.Algorithm) statement["alg"].AsInt32();
            }
        }
        
        private void GetSignature(CBORObject statement)
        {
            if (statement.ContainsKey("sig"))
            {
                Signature = statement["sig"].ToObject<byte[]>();
            }
        }
        
        private void GetCertificates(CBORObject statement)
        {
            if (statement.ContainsKey("x5c"))
            {
                var certs = new List<X509Certificate2>();
                var data = statement["x5c"].ToObject<byte[][]>();
                foreach (var cert in data)  
                {
                    certs.Add(new X509Certificate2(cert));
                }

                Certificates = certs.ToArray();
            }
        }
    }
}