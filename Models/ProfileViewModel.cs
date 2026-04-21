using System.ComponentModel.DataAnnotations;

namespace ders_programi_yonetim_sistemi.Models;

public class ProfileViewModel
{
    [Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress, Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Ad Soyad")]
    public string? FullName { get; set; }

    [DataType(DataType.Password), Display(Name = "Mevcut Parola")]
    public string? CurrentPassword { get; set; }

    [DataType(DataType.Password), MinLength(6), Display(Name = "Yeni Parola")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password), Compare(nameof(NewPassword)), Display(Name = "Yeni Parola (Tekrar)")]
    public string? ConfirmNewPassword { get; set; }

    [Display(Name = "Bağlı Hoca")]
    public string? InstructorName { get; set; }

    [Display(Name = "Roller")]
    public IList<string> Roles { get; set; } = new List<string>();
}
