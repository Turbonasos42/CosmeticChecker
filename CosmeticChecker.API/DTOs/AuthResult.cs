using System.ComponentModel.DataAnnotations;

namespace CosmeticChecker.API.DTOs
{
    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public UserProfileDto User { get; set; }
    }

}
