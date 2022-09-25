using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;


namespace Encrypt.Controllers
{
    [ApiController]
    [Route("api/v1/encrypt")]
    public class EncryptController: ControllerBase
    {

        [HttpPost]
        public ActionResult Post([FromBody] IDictionary<string, string> arguments)
        {
            if (!arguments.ContainsKey("datosJSONtxt"))
                return BadRequest(new Dictionary<string, object>
                        {
                            { "error", true },
                            { "code", 400 },
                            { "message", "La clave 'datosJSONtxt' es requerida." }
                        });
            if (!arguments.ContainsKey("rsaPublicKey"))
                return BadRequest(new Dictionary<string, object>
                        {
                            { "error", true },
                            { "code", 400 },
                            { "message", "La clave 'rsaPublicKey' es requerida." }
                        });

            var datosJSONtxt = arguments["datosJSONtxt"];
            var rsaPublicKey = arguments["rsaPublicKey"];
            var pData = AESEncryptString(datosJSONtxt);
            var cadena = pData.Vi+"::"+pData.salt+"::" +pData.Passphrase;
            byte[] data = Encoding.UTF8.GetBytes(cadena.ToString());
            byte[] dataPK = Convert.FromBase64String(rsaPublicKey);

            X509Certificate2 x509Certificate = new X509Certificate2(dataPK);
            var rsaPK = x509Certificate.GetRSAPublicKey();
            RsaKeyParameters publickeyRestored = (RsaKeyParameters)PublicKeyFactory.CreateKey(rsaPK.ExportSubjectPublicKeyInfo());

            IBufferedCipher cipher2 = CipherUtilities.GetCipher("RSA/ECB/OAEPWithSHA1AndMGF1Padding");
            cipher2.Init(true, publickeyRestored);
            byte[] encryptedBytes = cipher2.DoFinal(data);

            var result = Convert.ToBase64String(encryptedBytes);

            return Ok(new Dictionary<string, string>
                        {
                            { "xObjEnc", result + ":::" + pData.CypherData},
                            { "Vi", pData.Vi},
                            { "salt1", pData.salt},
                            { "Passphrase", pData.Passphrase}
                        });
        }

        private static string RSAEncryptWithPK(string pData, string rsaPublicKey)
        {
            byte[] data = Encoding.UTF8.GetBytes(pData.ToString());
            byte[] dataPK = Convert.FromBase64String(rsaPublicKey);

            X509Certificate2 x509Certificate = new X509Certificate2(dataPK);
            var rsaPK = x509Certificate.GetRSAPublicKey();
            RsaKeyParameters publickeyRestored = (RsaKeyParameters)PublicKeyFactory.CreateKey(rsaPK.ExportSubjectPublicKeyInfo());

            IBufferedCipher cipher2 = CipherUtilities.GetCipher("RSA/ECB/OAEPWITHSHA256ANDMGF1WITHSHA1PADDING");
            cipher2.Init(true, publickeyRestored);
            byte[] encryptedBytes = cipher2.DoFinal(data);

            return Convert.ToBase64String(encryptedBytes);
        }

        public static ComponenteCifradoDTO AESEncryptString(string datosJSONtxt)
        {
            //Se generan las llaves aleatorias.
            string passphrase = Keys.CreateRandom(true, 16);
            string vi1 = Keys.CreateRandom(false, 16);
            string salt1 = Keys.CreateRandom(false, 16);

            byte[] viBytes = Encoding.Default.GetBytes(vi1);
            string viHex = Convert.ToHexString(viBytes);

            byte[] saltBytes = Encoding.Default.GetBytes(salt1);
            string saltHex = Convert.ToHexString(saltBytes);

            var viByte = Encoding.Default.GetBytes(viHex);
            var saltByte = Encoding.Default.GetBytes(saltHex);

            //Se genera el Secret key
            int iterations = 1000;
            var rfc2898 = new Rfc2898DeriveBytes(passphrase, saltByte, iterations, HashAlgorithmName.SHA1);

            byte[] key = rfc2898.GetBytes(16);
            byte[] data = Encoding.UTF8.GetBytes(datosJSONtxt);

            //Cifrado Bouncyclastle
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
            cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), viBytes));
            byte[] encryptedBytes = cipher.DoFinal(data);

            return new ComponenteCifradoDTO()
            {
                Vi = viHex,
                salt = saltHex,
                Passphrase = passphrase,
                CypherData = Convert.ToBase64String(encryptedBytes)
            };
        }

        public class Keys
        {
            private static readonly string key = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXTZabcdefghiklmnopqrstuvwxyz*&- %/!?*+=()";
            private static readonly string rndkey = "0123456789abcdefghiklmnopqrstuvwxyz";

            public static string CreateRandom(bool Extendido, int largo)
            {
                StringBuilder sb = new StringBuilder();
                Random rnd = new Random();
                var random = Extendido ? key : rndkey;
                while (sb.Length < largo)
                {
                    int index = (int)(rnd.NextDouble() * random.Length);

                    sb.Append(random.ToCharArray(index, 1));
                }

                return sb.ToString();
            }
        }

    }

    
}

