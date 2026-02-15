using GC.Account.API.Data;
using GC.Account.API.Enums;
using GC.Account.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GC.Account.API.Core
{
    public class AccountService : IAccountService
    {
        private readonly AccountDbContext _context;

        public AccountService(AccountDbContext context)
        {
            _context = context;
        }

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
                    // Se lee la cuenta (Incluye el RowVersion)
                    var account = await _context.Accounts.FindAsync(accountId);

                    if (account == null) throw new KeyNotFoundException("Cuenta no encontrada.");

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
            }
        }

        public Task<Models.Account?> GetAccountByUserIdAsync(int userId)
        {
            var account = _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null) throw new KeyNotFoundException("Cuenta no encontrada.");

            return account;

        }

        public async Task<decimal> GetBalanceAsync(int userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null) throw new KeyNotFoundException("Cuenta no encontrada.");

            return account.Balance;
        }

        public Task WithdrawAsync(int accountId, decimal amount)
        {
            throw new NotImplementedException();
        }
    }
}