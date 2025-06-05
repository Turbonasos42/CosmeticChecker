using System.ComponentModel.DataAnnotations;

namespace CosmeticChecker.API.DTOs
{
    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class ChangeRoleRequest
    {
        [Required]
        public int RoleId { get; set; }
    }
}

