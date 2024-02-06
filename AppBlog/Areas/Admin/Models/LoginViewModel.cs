using System.ComponentModel.DataAnnotations;

namespace AppBlog.Areas.Admin.Models
{
    public class LoginViewModel
    {
        [Key]
        [MaxLength(50)]
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [Display(Name = "Địa chỉ Email")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage =" Vui lòng nhập Email")]
        public string? Email { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Vui lòng nhập password")]
        [MaxLength(30,ErrorMessage ="Mật khẩu chỉ tối đa 30 ký tự")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

    }
}
