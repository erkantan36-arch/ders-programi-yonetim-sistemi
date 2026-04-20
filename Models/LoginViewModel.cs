using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class LoginViewModel
{
    [Required, Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Beni hatırla")]
    public bool RememberMe { get; set; }
}
