using System;
using System.ComponentModel.DataAnnotations;
using API.Attributes;

namespace API.DTOs.Account
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [StringLength(8,MinimumLength = 4)]
        public string Password  { get; set; }

        [Required] public string KnowAs { get; set; }
        [Required] public string Gender { get; set; }
        [AgeMustAfterOrEqualTo(18,ErrorMessage = "your age must after or equal to 18")]
        [Required] public DateTimeOffset DateOfBirth { get; set; }
        [Required] public string City { get; set; }
        [Required] public string Country { get; set; }

    }
}
