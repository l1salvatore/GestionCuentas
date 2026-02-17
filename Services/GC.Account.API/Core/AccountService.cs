using GC.Account.API.Data;
using GC.Account.API.Enums;
using GC.Account.API.Models;
using GC.Account.API.Services.Withdraw.Rules;
using Microsoft.EntityFrameworkCore;

namespace GC.Account.API.Core
{
    public class AccountService : IAccountService
    {
        private readonly IEnumerable<IAccountWithdrawRule> _rules;
        private readonly AccountDbContext _context;

        /// <summary>
        /// El servicio de cuenta se encarga de manejar toda la lógica relacionada con las cuentas,
        /// incluyendo validaciones de negocio (reglas) y manejo de concurrencia.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="rules"></param>
        public AccountService(AccountDbContext context, IEnumerable<IAccountWithdrawRule> rules)
        {
            _context = context;
            _rules = rules;
        }

        /// <summary>
        /// Busca la cuenta usando el ID del usuario (útil para el login inicial)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<Models.Account> GetAccountByUserIdAsync(int userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null) throw new KeyNotFoundException("Cuenta no encontrada.");

            return account;
        }


        /// <summary>
        /// Crea la cuenta vinculada al usuario que viene del Token.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Models.Account> CreateAccountAsync(int userId, string firstName, string lastName)
        {
            // 1. Validar que no exista ya (Regla de Negocio)
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (existingAccount != null)
            {
                throw new InvalidOperationException($"El usuario {userId} ya tiene una cuenta.");
            }

            // 2. Crear la cuenta
            var newAccount = new Models.Account
            {
                UserId = userId,
                FirstName = firstName,
                LastName = lastName,
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            return newAccount;
        }

        /// <summary>
        /// Suma saldo y registra transacción. Para manejar la concurrencia, 
        /// implementa un retry simple basado en la excepción de concurrencia de EF.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task DepositAsync(int accountId, decimal amount)
        {
            // Validación básica
            if (amount <= 0) throw new ArgumentException("El monto debe ser positivo.");

            // Para manejar la concurrencia, vamos a implementar un retry simple.
            int maxRetries = 3;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {

                    var account = await GetAccountByUserIdAsync(accountId);

                    // Se actualiza el balance en memoria (No se ha guardado aún, por lo que el RowVersion sigue siendo el mismo)
                    account.Balance += amount;

                    // Se agrega la transacción al contexto (Relación)
                    _context.Transactions.Add(new Transaction
                    {
                        AccountId = accountId,
                        Amount = amount,
                        Type = TransactionType.Deposit,
                        TransactionDate = DateTime.UtcNow,
                    });

                    // Se intenta guardar. 
                    // Si nadie más modificó esta cuenta desde que la leímos, 
                    // el RowVersion coincide y se guarda sin problemas.
                    await _context.SaveChangesAsync();

                    // Si llegamos acá, salió todo bien. Salimos del loop/método.
                    return;
                }
                // Si alguien más modificó esta cuenta después de que la leímos, el RowVersion no coincide y EF lanza esta excepción.
                catch (DbUpdateConcurrencyException)
                {

                    // Limpiamos el ChangeTracker para evitar conflictos con las entidades que ya tenemos cargadas
                    // (incluyendo la cuenta que falló)
                    _context.ChangeTracker.Clear();

                    // Si ya intentamos el máximo de reintentos,
                    // lanzamos una excepción para que el cliente sepa que intente más tarde.
                    if (i == maxRetries - 1)
                    {
                        throw new Exception("Ocurrió un error al procesar tu depósito. Por favor, intenta nuevamente.");
                    }

                    // Si no, el loop vuelve a arrancar (i++), 
                    // vuelve a leer el saldo (que ahora ya tiene el cambio del otro hilo)
                    // y vuelve a sumar nuestro monto sobre el saldo actualizado.
                }
                catch (KeyNotFoundException ex)
                {
                    throw new KeyNotFoundException(ex.Message);
                }
            }
        }


        /// <summary>
        /// Resta saldo (validando reglas) y registra transacción. Para manejar la concurrencia,
        /// implementa un retry simple basado en la excepción de concurrencia de EF.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<decimal> GetBalanceAsync(int userId)
        {
            var account = await GetAccountByUserIdAsync(userId);

            return account.Balance;
        }

        /// <summary>
        /// Resta saldo (validando reglas) y registra transacción. Para manejar la concurrencia,
        /// implementa un retry simple basado en la excepción de concurrencia de EF.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task WithdrawAsync(int accountId, decimal amount)
        {
            var account = await GetAccountByUserIdAsync(accountId);

            // Para manejar la concurrencia, vamos a implementar un retry simple.
            int maxRetries = 3;

            for (int i = 0; i < maxRetries; i++)
            {
                // Ejecuciones de validaciones (Reglas de Negocio)
                foreach (var rule in _rules)
                {
                    await rule.ValidateAsync(account, amount);
                }

                // Si pasó todas las reglas, procesamos
                account.Balance -= amount;
                _context.Transactions.Add(new Transaction
                {
                    AccountId = accountId,
                    Amount = -amount,
                    Type = TransactionType.Withdrawal,
                    TransactionDate = DateTime.UtcNow,
                });

                // Aquí podríamos implementar la misma lógica de retry que en el depósito, pero para no repetir código,
                try
                {
                    await _context.SaveChangesAsync();

                    // Si llegamos acá, salió todo bien. Salimos del loop/método.
                    return;
                }
                catch (DbUpdateConcurrencyException)
                {
                    _context.ChangeTracker.Clear();

                    // Si ya intentamos el máximo de reintentos,
                    // lanzamos una excepción para que el cliente sepa que intente más tarde.
                    if (i == maxRetries - 1)
                    {
                        throw new Exception("Ocurrió un error al procesar el retiro. Por favor, intenta nuevamente.");
                    }

                }
            }
        }
    }
}