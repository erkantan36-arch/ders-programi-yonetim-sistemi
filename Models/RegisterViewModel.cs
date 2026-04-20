using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class RegisterViewModel
{
    [Required, Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress, Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6), Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Şifre (Tekrar)")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Ad Soyad")]
    public string? FullName { get; set; }
}
