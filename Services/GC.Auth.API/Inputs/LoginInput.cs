using System.ComponentModel.DataAnnotations;

namespace GC.Auth.API.Inputs
{
    public class LoginInput
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}