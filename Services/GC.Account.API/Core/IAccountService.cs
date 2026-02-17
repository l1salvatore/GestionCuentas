using GC.Account.API.Models;

namespace GC.Account.API.Core 
{
    public interface IAccountService
    {
        // Crea la cuenta vinculada al usuario que viene del Token
        Task<Models.Account> CreateAccountAsync(int userId, string firstName, string lastName);

        // Busca la cuenta usando el ID del usuario (útil para el login inicial)
        Task<Models.Account> GetAccountByUserIdAsync(int userId);

        // Devuelve el saldo actual
        Task<decimal> GetBalanceAsync(int userId);

        // Suma saldo y registra transacción
        Task DepositAsync(int userId, decimal amount);

        // Resta saldo (validando reglas) y registra transacción
        Task WithdrawAsync(int userId, decimal amount);
    }
}