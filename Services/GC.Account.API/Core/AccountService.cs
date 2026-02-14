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
            // // 1. Validaciones
            // if (amount <= 0) throw new ArgumentException("El monto debe ser positivo.");

            // var account = await _context.Accounts
            //         .FirstOrDefaultAsync(a => a.Id == accountId);

            // if (account == null) throw new KeyNotFoundException("Cuenta no encontrada.");

            // // Actualizar el saldo
            // account.Balance += amount;

            // // Registrar la transacción
            // var transactionHistory = new Transaction
            // {
            //     AccountId = accountId,
            //     Amount = amount,
            //     Type = TransactionType.Deposit, 
            //     TransactionDate = DateTime.UtcNow,
            // };

            // // Agregar la transacción al contexto
            // _context.Transactions.Add(transactionHistory);

            // // Guardar los cambios en la base de datos (saldo actualizado + nueva transacción)
            // await _context.SaveChangesAsync();
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