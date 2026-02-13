using System.Security.Cryptography;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using GC.Auth.API.Models;
namespace GC.Auth.API.Helpers
{
    public static class JwtHelper
    {
        // Configuración de seguridad (Estándares OWASP 2024)
        public static string CreateJwtToken(User user)
        {
            return GenerateJwtToken(user);
        }

        // --- MÉTODO PRIVADO PARA GENERAR EL TOKEN ---
        private static string GenerateJwtToken(User user)
        {
            // Se define los "Claims", es decir los datos que van adentro del token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // El "sub" es el identificador del usuario
                new Claim(JwtRegisteredClaimNames.Email, user.Email), // El email también lo incluimos como claim
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Un ID único para el token (opcional pero recomendado)
                };

            // Leemos la Llave Privada (RSA)
            // Definir la ruta del archivo de llave (puede variar según el entorno)
            string keyPath = Path.Combine(Directory.GetCurrentDirectory(), "Keys", "private_key.pem");

            // Si no está ahí, probamos subiendo niveles (para desarrollo)
            if (!System.IO.File.Exists(keyPath))
            {
                keyPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Keys", "private_key.pem");
            }

            // Si no está ahí, probamos en la raíz (para Docker)
            if (!System.IO.File.Exists(keyPath))
            {
                keyPath = "/app/Keys/private_key.pem";
            }

            // Si no se encuentra el archivo de llave, lanzamos una excepción (esto debería ser manejado adecuadamente en producción)
            if (!System.IO.File.Exists(keyPath))
            {
                throw new FileNotFoundException($"No se encontró el archivo de llave en: {keyPath}");
            }

            // Cargar la llave RSA desde el archivo PEM
            var rsa = RSA.Create(); // Crear una instancia de RSA
            string privateKeyContent = System.IO.File.ReadAllText(keyPath); // Leer el contenido del archivo PEM
            rsa.ImportFromPem(privateKeyContent); // Importar la llave al objeto RSA

            // Crear las credenciales de firma usando la llave RSA y el algoritmo RS256
            var key = new RsaSecurityKey(rsa); // Crear una clave de seguridad a partir del objeto RSA
            var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256); // Crear las credenciales de firma usando el algoritmo RS256s

            // Crear el token JWT con los claims, la fecha de expiración y las credenciales de firma
            var token = new JwtSecurityToken(
                issuer: "GC.Auth",      // Quién lo emite
                audience: "GC.Account", // Quién lo va a usar
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Dura 1 hora
                signingCredentials: creds
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}