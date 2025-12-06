using Posts.Application.Core;
using System.Security.Cryptography;
using System.Text;

namespace Posts.Infrastructure.Core
{
    internal class Encryption : IEncryption
    {
        private readonly int _tagSizeInBytes = 16;
        private readonly byte[] _key;

        public Encryption(string key)
        {
            if (key.Length < 32)
                throw new ArgumentException("Encryption key should be 32 chars (256-bit)");

            _key = Encoding.UTF8.GetBytes(key.Substring(0, 32));
        }

        public string Encrypt(string plainText)
        {
            using (var aes = new AesGcm(_key, _tagSizeInBytes))
            {
                byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);

                byte[] nonce = RandomNumberGenerator.GetBytes(12);

                byte[] cipherBytes = new byte[plaintextBytes.Length];
                byte[] tag = new byte[16];

                aes.Encrypt(nonce, plaintextBytes, cipherBytes, tag);

                var combined = new byte[nonce.Length + cipherBytes.Length + tag.Length];
                Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
                Buffer.BlockCopy(cipherBytes, 0, combined, nonce.Length, cipherBytes.Length);
                Buffer.BlockCopy(tag, 0, combined, nonce.Length + cipherBytes.Length, tag.Length);

                return Convert.ToBase64String(combined);
            }
        }

        public string Decrypt(string cipherText)
        {
            var combined = Convert.FromBase64String(cipherText);

            using (var aes = new AesGcm(_key, _tagSizeInBytes))
            {
                byte[] nonce = combined[..12];
                byte[] tag = combined[^16..];
                byte[] cipherBytes = combined[12..^16];

                byte[] resultBytes = new byte[cipherBytes.Length];

                aes.Decrypt(nonce, cipherBytes, tag, resultBytes);

                return Encoding.UTF8.GetString(resultBytes);
            }
        }

    }
}
