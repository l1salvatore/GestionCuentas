// Regla: No podés sacar más de lo que tenés
using GC.Account.API.Models;
using GC.Account.API.Services.Withdraw.Rules;

public class DailyLimitRule : IAccountWithdrawRule
{
    public Task ValidateAsync(Account account, decimal amount)
    {
        if (amount > 50000)
            throw new InvalidOperationException("El monto supera el límite diario permitido.");
        return Task.CompletedTask;
    }
}