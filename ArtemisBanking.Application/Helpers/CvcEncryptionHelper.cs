using System.Security.Cryptography;
using System.Text;

namespace ArtemisBanking.Application.Helpers
{
    public static class CvcEncryptionHelper
    {
        //Genera un CVC aleatorio de 3 dígitos (100-999)
        public static string GenerateCVC()
        {
            var random = new Random();
            return random.Next(100, 1000).ToString(); // Número entre 100 y 999
        }

        public static string EncryptCVC(string cvcPlano)
        {
            if (string.IsNullOrWhiteSpace(cvcPlano))
            {
                throw new ArgumentException("El CVC no puede estar vacío", nameof(cvcPlano));
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(cvcPlano);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        public static string GenerateAndEncryptCVC()
        {
            var cvcPlano = GenerateCVC();
            return EncryptCVC(cvcPlano);
        }
    }
}
