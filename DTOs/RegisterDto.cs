using System.ComponentModel.DataAnnotations;

namespace RestoreApiV2.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        public required string Password { get; set; }
    }
}
