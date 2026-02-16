using System.ComponentModel.DataAnnotations;

namespace GC.Account.API.DTOs
{
    public class WithdrawRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
        public decimal Amount { get; set; }
        
    }
}