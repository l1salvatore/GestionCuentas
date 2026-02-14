using System.ComponentModel.DataAnnotations;

namespace GC.Account.API.DTOs
{
    public class CreateAccountRequest
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;
    }
}