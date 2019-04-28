using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace GSA.Authentication.LegacyFormsAuthentication
{
    /// <remarks>Thank you https://gist.github.com/azechi/031ab6dd86d23c7444a8ca1085163c50</remarks>
    public class LegacyFormsAuthenticationService : ILegacyFormsAuthenticationService
    {
        private const int SHA1_HASH_SIZE = 20;

        public class Config
        {
            public const string ConfigSectionName = "LegacyFormsAuthenticationConfig";

            public FormsCompatibilityModes FormsCompatibilityMode { get; set; } = FormsCompatibilityModes.Framework20SP1;

            public LegacyFormsAuthenticationCryptographyAlgorithms CryptographyAlgorithm { get; set; } = LegacyFormsAuthenticationCryptographyAlgorithms.AES;

            public LegacyFormsAuthenticationHashAlgorithms HashAlgorithm { get; set; } = LegacyFormsAuthenticationHashAlgorithms.SHA1;

            public string DecryptionKey { get; set; }

            public string HashValidationKey { get; set; }
        }

        private IOptions<Config> ConfigOptions { get; }

        public LegacyFormsAuthenticationService(Config config)
            : this(new OptionsWrapper<Config>(config))
        { }

        public LegacyFormsAuthenticationService(IOptions<Config> configOptions)
        {
            ConfigOptions = configOptions;
        }

        public FormsAuthenticationCookie Unprotect(string encryptedTicket)
        {
            var config = ConfigOptions.Value;

            switch (config.FormsCompatibilityMode)
            {
                case FormsCompatibilityModes.Framework20SP1:
                    break;
                default:
                    throw new UnexpectedSwitchValueException(config.FormsCompatibilityMode);
            }

            int hashSize;
            switch (config.HashAlgorithm)
            {
                case LegacyFormsAuthenticationHashAlgorithms.SHA1:
                    hashSize = SHA1_HASH_SIZE;
                    break;
                default:
                    throw new UnexpectedSwitchValueException(config.HashAlgorithm);
            }

            SymmetricAlgorithm decryptAlgorithm;
            switch (config.CryptographyAlgorithm)
            {
                case LegacyFormsAuthenticationCryptographyAlgorithms.AES:
                    decryptAlgorithm = Aes.Create();
                    break;
                case LegacyFormsAuthenticationCryptographyAlgorithms.TripleDES:
                    decryptAlgorithm = TripleDES.Create();
                    break;
                default:
                    throw new UnexpectedSwitchValueException(config.CryptographyAlgorithm);
            };
            decryptAlgorithm.Key = HexToBinary(config.DecryptionKey);
            decryptAlgorithm.GenerateIV();
            decryptAlgorithm.IV = new byte[decryptAlgorithm.IV.Length];


            var ivLengthDecryption = (decryptAlgorithm.KeySize / 8) + (((decryptAlgorithm.KeySize & 7) != 0) ? 1 : 0);

            var bBlob = HexToBinary(encryptedTicket);
            var buf = GetUnHashedData(bBlob, hashSize, config.HashValidationKey);

            byte[] paddedData;

            using (var st = new MemoryStream())
            {
                using (var cryptoTransform = decryptAlgorithm.CreateDecryptor())
                {
                    using (var cs = new CryptoStream(st, cryptoTransform, CryptoStreamMode.Write))
                    {

                        cs.Write(buf, 0, buf.Length);
                        cs.FlushFinalBlock();
                        paddedData = st.ToArray();
                    }
                }
            }

            // strip IV
            var bDataLength = paddedData.Length - ivLengthDecryption;
            var bData = new byte[bDataLength];
            Buffer.BlockCopy(paddedData, ivLengthDecryption, bData, 0, bDataLength);

            using (var st = new MemoryStream(bData))
            {
                using (var reader = new SerializingBinaryReader(st))
                {
                    var c = new FormsAuthenticationCookie();
                    var formatVersion = reader.ReadByte();

                    var version = reader.ReadByte();

                    var utcTicks = reader.ReadInt64();
                    c.Issued = new DateTimeOffset(new DateTime(utcTicks, DateTimeKind.Utc));

                    var spacer = reader.ReadByte();

                    var expireDateTicks = reader.ReadInt64();
                    c.Expires = new DateTimeOffset(new DateTime(expireDateTicks, DateTimeKind.Utc));

                    var persistenceField = reader.ReadByte();

                    c.UserName = reader.ReadString();
                    c.UserData = reader.ReadString();
                    c.CookiePath = reader.ReadString();

                    var footer = reader.ReadByte();

                    return c;
                }

            }
        }

        private sealed class SerializingBinaryReader : BinaryReader
        {
            public SerializingBinaryReader(Stream input)
                : base(input)
            {
            }

            public override string ReadString()
            {
                int charCount = Read7BitEncodedInt();
                byte[] bytes = ReadBytes(charCount * 2);

                char[] chars = new char[charCount];
                for (int i = 0; i < chars.Length; i++)
                {
                    chars[i] = (char)(bytes[2 * i] | (bytes[2 * i + 1] << 8));
                }

                return new String(chars);
            }
        }

        private static byte[] GetUnHashedData(byte[] bufHashed, int hashSize, string hashValidationKey)
        {
            if (hashValidationKey != null)
            {
                if (!VerifyHashData(bufHashed, hashSize, hashValidationKey))
                {
                    return null;
                }
            }
            var buf2 = new byte[bufHashed.Length - hashSize];
            Buffer.BlockCopy(bufHashed, 0, buf2, 0, buf2.Length);
            return buf2;
        }

        private static bool VerifyHashData(byte[] bufHashed, int hashSize, string hashValidationKey)
        {
            var hasher = new HMACSHA1(HexToBinary(hashValidationKey));
            var bMac = hasher.ComputeHash(bufHashed, 0, bufHashed.Length - hashSize);

            var lastPos = bufHashed.Length - hashSize;

            var hashCheckFailed = false;
            for (var iter = 0; iter < hashSize; iter++)
            {
                if (bMac[iter] != bufHashed[lastPos + iter])
                {
                    hashCheckFailed = true;
                }
            }

            return !hashCheckFailed;
        }

        static int HexToInt(char h)
        {
            return
                (h >= '0' && h <= '9') ? h - '0'
                : (h >= 'a' && h <= 'f') ? h - 'a' + 10
                : (h >= 'A' && h <= 'F') ? h - 'A' + 10
                : -1
                ;
        }

        static byte[] HexToBinary(string data)
        {
            if (data == null || data.Length % 2 != 0)
            {
                return null;
            }

            var binary = new byte[data.Length / 2];

            for (int i = 0; i < binary.Length; i++)
            {
                int highNibble = HexToInt(data[2 * i]);
                int lowNibble = HexToInt(data[2 * i + 1]);

                if (highNibble == -1 || lowNibble == -1)
                {
                    return null;
                }

                binary[i] = (byte)((highNibble << 4) | lowNibble);
            }

            return binary;

        }
    }
}
