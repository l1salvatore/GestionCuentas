using Microsoft.EntityFrameworkCore;
using GC.Account.API.Data;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using GC.Account.API.Core;

// Configuración de seguridad (Estándares OWASP 2024)
var builder = WebApplication.CreateBuilder(args);

// Se agrega el DbContext de la cuenta
builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAccountService, AccountService>();

// Se agrega el servicio de controladores
builder.Services.AddControllers();
// Se agrega Swagger/OpenAPI para documentación (opcional pero recomendado)
builder.Services.AddOpenApi(); 

// --- CONFIGURACIÓN DE AUTENTICACIÓN JWT ---
var publicKeyRelativePath = builder.Configuration["JwtSettings:PublicKeyPath"];

// Si no está en el JSON, usamos un default sensato
if (string.IsNullOrEmpty(publicKeyRelativePath))
{
    publicKeyRelativePath = "keys/public_key.pem"; 
}

// Construimos la ruta absoluta a la llave pública
var publicKeyPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), publicKeyRelativePath));

// Verificamos que el archivo de la llave pública exista antes de intentar cargarlo
if (!File.Exists(publicKeyPath))
{
    throw new FileNotFoundException($"CRITICAL: No se encontró la llave pública en: {publicKeyPath}. Revisá el 'PublicKeyPath' en appsettings.json.");
}

// Cargar la llave RSA desde el archivo PEM
var publicKeyPem = File.ReadAllText(publicKeyPath);
// Crear una instancia de RSA y cargar la llave pública
var rsa = RSA.Create(); 
// Importar la llave pública al objeto RSA
rsa.ImportFromPem(publicKeyPem);

// Configuración de autenticación JWT usando la llave pública para validar los tokens emitidos por el Auth Service
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configuración de validación de tokens JWT
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Validación de la firma del token usando la llave pública RSA
            ValidateIssuerSigningKey = true,
            
            // Se establece la llave de validación a partir de la llave pública RSA
            IssuerSigningKey = new RsaSecurityKey(rsa), 
            
            // Validación de otros parámetros del token 
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, 

            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication(); 

app.UseAuthorization();  

app.MapControllers();

app.Run();