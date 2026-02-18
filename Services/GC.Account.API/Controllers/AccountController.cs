using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GC.Account.API.Core;
using GC.Account.API.DTOs;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;

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

        // POST: api/account/withdraw
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
        {
            try
            {
                // Buscamos el claim "NameIdentifier" (que suele ser el 'sub' o 'uid' del token) 
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized("El token no contiene un ID de usuario válido.");

                // Parseamos el ID que venía encriptado en el token
                int userId = int.Parse(userIdClaim.Value);

                // Intentamos hacer el retiro. Si hay un choque de concurrencia, el servicio se encargará de reintentar automáticamente.
                await _accountService.WithdrawAsync(userId, request.Amount);

                // Si llegamos acá, el retiro se realizó con éxito (sin excepciones no controladas)
                return Ok(new { message = "Retiro realizado con éxito." });
            }
            catch (InvalidOperationException ex) // Ej: Reglas de negocio no cumplidas
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "La cuenta fue modificada por otra operación. Por favor, intentá de nuevo." });
            }       
            catch (KeyNotFoundException)
            {
                return NotFound("Cuenta no encontrada.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno.", details = ex.Message });
            }
        }

        // POST: api/account/deposit
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            try
            {
                // Buscamos el claim "NameIdentifier" (que suele ser el 'sub' o 'uid' del token) 
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized("El token no contiene un ID de usuario válido.");

                // Parseamos el ID que venía encriptado en el token
                int userId = int.Parse(userIdClaim.Value);

                // Intentamos hacer el depósito. Si hay un choque de concurrencia, el servicio se encargará de reintentar automáticamente.
                await _accountService.DepositAsync(userId, request.Amount);

                // Si llegamos acá, el depósito se realizó con éxito (sin excepciones no controladas)
                return Ok(new { message = "Depósito realizado con éxito." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Cuenta no encontrada.");
            }
            catch (Exception ex)
            {
                // Cualquier excepción que no hayamos manejado específicamente se traduce en un error 500 para el cliente, con un mensaje genérico.
                return StatusCode(500, new { message = "Error interno.", details = ex.Message });
            }
        }

        // GET: api/account/balance
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            try
            {
                // Buscamos el claim "NameIdentifier" para obtener el ID del usuario
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized("El token no contiene un ID de usuario válido.");

                // Parseamos el ID del usuario desde el claim
                int userId = int.Parse(userIdClaim.Value);

                // Llamamos al servicio para obtener el balance de la cuenta vinculada a ese ID de usuario
                var account = await _accountService.GetAccountByUserIdAsync(userId);
                decimal balance = account.Balance;

                // Devolvemos el balance encontrado con un status 200 OK
                return Ok(new { balance });
            }
            catch (KeyNotFoundException)
            {
                // Si el servicio lanza una excepción de "no encontrado", devolvemos 404 con el mensaje específico
                return NotFound("Cuenta no encontrada.");
            }
            catch (Exception ex)
            {
                // Cualquier otra excepción se traduce en un error 500 para el cliente, con un mensaje genérico.
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
            try
            {
                // Llamamos al servicio para obtener la cuenta vinculada a ese ID de usuario
                var account = await _accountService.GetAccountByUserIdAsync(userId);

                // Agregamos el email al DTO de respuesta (opcional, pero útil para el frontend)
                account.Email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

                // Devolvemos la cuenta encontrada con un status 200 OK
                return Ok(account);
            }
            catch (KeyNotFoundException)
            {
                // Si el servicio lanza una excepción de "no encontrado", devolvemos 404 con el mensaje específico
                return NotFound("No tenés una cuenta creada todavía.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "La cuenta fue modificada por otra operación. Por favor, intentá de nuevo." });
            }
            catch (Exception ex)
            {
                // Cualquier otra excepción se traduce en un error 500 para el cliente, con un mensaje genérico.
                return StatusCode(500, new { message = "Error interno.", details = ex.Message });
            }
        }
    }
}