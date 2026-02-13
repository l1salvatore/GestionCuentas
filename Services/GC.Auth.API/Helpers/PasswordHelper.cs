using System.Security.Cryptography;
using System.Text;

namespace GC.Auth.API.Helpers
{
    public static class PasswordHelper
    {
        // Configuración de seguridad (Estándares OWASP 2024)
        private const int KeySize = 32; // 256 bits
        private const int SaltSize = 16; // 128 bits
        private const int Iterations = 100000; // Suficiente para frenar ataques de fuerza bruta
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256; // El reemplazo de SHA-1

        public static string HashPassword(string password)
        {
            // Se generar un Salt aleatorio
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Se generar el Hash usando la pbkdf2 
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                Algorithm,
                KeySize
            );

            // Se devuelve el formato: "iteraciones.salt.hash" (Todo en base64)
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string passwordHash)
        {
            // 1. Separar las partes
            var parts = passwordHash.Split('.', 3);
            if (parts.Length != 3) return false;

            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var storedHash = Convert.FromBase64String(parts[2]);

            // 2. Volver a hashear la contraseña que ingresó el usuario con el MISMO salt
            byte[] hashToCheck = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                Algorithm,
                KeySize
            );

            // 3. Comparar byte por byte (CryptographicEquals evita ataques de tiempo)
            return CryptographicOperations.FixedTimeEquals(storedHash, hashToCheck);
        }
    }
}