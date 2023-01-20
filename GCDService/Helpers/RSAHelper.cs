
using System.Security.Cryptography;
using System.Text;

namespace GCDService.Helpers
{

    public static class RSAHelper
    {
        private static readonly string _privateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIICWwIBAAKBgHu8HS5T3nIOKlGJzDgwGjdb3x9826T6Mz9XRuKYUM1Xe1N1Vddf
qoR7oC0w1AxMRTC/VEs614KtHNJVD4AsL/zjUKO7vKWCRu/HPe7qeWI2nFNCuxqk
+Ky3Pw0hrdILXU0v1XNQIfqok4pyWfQzrk9UZWhKit1R22/pb/3X8GcVAgMBAAEC
gYA+p11bVQqBVFznVtA671iHCZUsm4uYuTxz6VyyZpAbuh1vgC2MVhvA49ySpXPq
GkC601b6lPwFZmT+uCWWMEnHnjCzj6Zt0I9sDZZ7tZiZiiI99g3e/eu6VhEt1Jq0
TCsXe7J+2d7PBChb+ivzgIbhccMDR31AJURFQ1MrKYA8bQJBAL3XyyUdDMdCEgAY
ofOhQRuPeTYBzD55p2I4N/ICOvgmQG6FYAEPLebfz2Rfmh4dfvJ5+fBIqkjQLIYQ
Up9dsa8CQQCm2rRIMxs93CM81s4YAXohjhjNsP1myp6IVqNmTIzkpACBo44/0yhR
epT3AVX793IV1mDbOP+9H8OKrr6h/nh7AkEAi7oEfvB+sznh3chDivmo3gwffqyc
E2+ezx/prWoO1Q3yZmYsXxs5AbDBCHOFD51ODlHQsBYLn0P5QUNKTJm9aQJAaz+W
s/XNo9R0/e0gacPBSgI5JTWHm+PPRNlTjDTWNzzHaozDqXjDqKO/TomYSC4EMc8r
UJ3xnBHnsaOiuyODMwJATvmBBENegtRhw5DRmnJio/hbY8mGK1QtCennnkRejf7t
IhHpCxCQA2lrivPglUKU7hIZW9vwyGE/YljUq/1Y1Q==
-----END RSA PRIVATE KEY-----";
        private static readonly System.Security.Cryptography.RSA PrivateKey = GetPrivateKeyFromPemFile();

        public static string Decrypt(string encrypted)
        {
            var decryptedBytes = PrivateKey.Decrypt(Convert.FromBase64String(encrypted), RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
        }

        private static System.Security.Cryptography.RSA GetPrivateKeyFromPemFile()
        {
            var rsa = System.Security.Cryptography.RSA.Create();
            rsa.ImportFromPem(_privateKey.ToCharArray());
            return rsa;
        }

        private static string GetPath(string fileName, string filePath)
        {
            var path = Path.Combine(".", filePath);
            return Path.Combine(path, fileName);
        }
    }
}
