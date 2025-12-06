namespace Posts.Application.Core
{
    public interface IEncryption
    {
        public string Encrypt(string plainText);
        public string Decrypt(string plainText);
    }
}
