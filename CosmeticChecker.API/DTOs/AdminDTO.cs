using System.ComponentModel.DataAnnotations;

namespace CosmeticChecker.API.DTOs
{
    public class ChangeRoleDto
    {
        [Required]
        public int RoleId { get; set; }
    }

    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
