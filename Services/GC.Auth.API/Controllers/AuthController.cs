using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GC.Auth.API.Data;   // El DbContext
using GC.Auth.API.Models; // El modelo User
using GC.Auth.API.DTOs;   // El DTOs de entrada
using GC.Auth.API.Helpers; // La clase de hashing de contraseñas


namespace GC.Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;

        // Inyectamos la base de datos para poder usarla
        public AuthController(AuthDbContext context)
        {
            _context = context;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto input)
        {

            // Se valida que el email no exista ya en la base de datos
            if (await _context.Users.AnyAsync(u => u.Email == input.Email))
            {
                return BadRequest("El email ya existe.");
            }

            // Se crea un nuevo usuario con el email y la contraseña hasheada 
            var user = new User
            {
                Email = input.Email,
                // Se encripta la contraseña antes de guardarla
                Password = PasswordHelper.HashPassword(input.Password)
            };

            //  Se guarda el usuario en la base de datos
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registrado con éxito" });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // Se busca el usuario por mail
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            // Se verifica que el usuario exista y que la contraseña sea correcta
            if (user == null || !PasswordHelper.VerifyPassword(dto.Password, user.Password))
            {
                return Unauthorized("Credenciales inválidas.");
            }

            // Se genera el token JWT para el usuario autenticado
            try
            {
                var token = JwtHelper.CreateJwtToken(user);
                return Ok(new { token });
            }
            catch (FileNotFoundException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }      
    }
}