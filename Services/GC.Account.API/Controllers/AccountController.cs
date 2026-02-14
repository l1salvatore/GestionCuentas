using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GC.Account.API.Core;
using GC.Account.API.DTOs;

namespace GC.Account.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // POST: api/account
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            try
            {
                // Buscamos el claim "NameIdentifier" (que suele ser el 'sub' o 'uid' del token)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("El token no contiene un ID de usuario válido.");
                }

                // Parseamos el ID que venía encriptado en el token
                int userId = int.Parse(userIdClaim.Value);

                // Llamamos al servicio para crear la cuenta, pasando el ID del usuario y los datos del request
                var account = await _accountService.CreateAccountAsync(userId, request.FirstName, request.LastName);

                // Agregamos el email al DTO de respuesta (opcional, pero útil para el frontend)
                account.Email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

                // Devolvemos la cuenta creada con un status 201 Created
                return CreatedAtAction(nameof(GetMyAccount), new { }, account);
            }
            catch (InvalidOperationException ex) // Ej: Ya tiene cuenta
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno.", details = ex.Message });
            }
        }

        // GET: api/account
        [HttpGet]
        public async Task<IActionResult> GetMyAccount()
        {
            // Buscamos el claim "NameIdentifier" para obtener el ID del usuario
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            // Si no encontramos el claim, devolvemos 401 Unauthorized
            if (userIdClaim == null) return Unauthorized();
            // Parseamos el ID del usuario desde el claim
            int userId = int.Parse(userIdClaim.Value);

            // Llamamos al servicio para obtener la cuenta vinculada a ese ID de usuario
            var account = await _accountService.GetAccountByUserIdAsync(userId);

            // Si no se encuentra la cuenta, devolvemos 404 Not Found con un mensaje amigable
            if (account == null) return NotFound("No tenés una cuenta creada todavía.");

            // Agregamos el email al DTO de respuesta (opcional, pero útil para el frontend)
            account.Email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

            // Devolvemos la cuenta encontrada con un status 200 OK
            return Ok(account);
        }
    }
}