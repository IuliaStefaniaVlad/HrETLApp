using System.ComponentModel.DataAnnotations;

namespace HrappModels
{
    public record TenantInfoModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
