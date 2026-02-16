// Regla: No podés sacar más de lo que tenés
using GC.Account.API.Services.Withdraw.Rules;
using GC.Account.API.Models;
public class SufficientBalanceRule : IAccountWithdrawRule
{
    public Task ValidateAsync(Account account, decimal amount)
    {
        if (account.Balance < amount)
            throw new InvalidOperationException("Saldo insuficiente.");
        return Task.CompletedTask;
    }
}