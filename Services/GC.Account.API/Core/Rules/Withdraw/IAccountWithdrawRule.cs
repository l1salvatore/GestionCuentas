using GC.Account.API.Models;

namespace GC.Account.API.Services.Withdraw.Rules
{
    public interface IAccountWithdrawRule
    {
        // Esta interfaz define una regla de negocio que se puede aplicar a las operaciones de cuenta.
        Task ValidateAsync(Models.Account account, decimal amount);
    }
}