# Ders Programı Yönetim Sistemi

ASP.NET Core MVC (.NET 8) ile geliştirilmiş üniversite ders programı yönetim sistemi.

## Özellikler

- Günler: Pazartesi - Cuma
- Derslikler: Anfi-1..4 ve Derslik-1..8
- Saatler: 7 zaman dilimi
- Sınıflar: 1-A, 1-B, 2-A, 2-B, 3-A, 3-B, 4-A, 4-B
- Ders ve hoca yönetimi
- Haftalık ders programı görüntüleme
- Çakışma kontrolü (aynı gün/saatte derslik, sınıf veya hoca)
- Rol bazlı yetkilendirme
  - **Admin**: Tüm işlemler
  - **Instructor**: Sadece kendi ders/ders programı kayıtları
  - **Viewer**: Program görüntüleme

## Kurulum

```bash
dotnet restore
dotnet run
```

Uygulama ilk açılışta veritabanını ve seed verilerini oluşturur.

## Varsayılan kullanıcılar

- `admin / Admin123!`
- `ozlem / Instructor123!`
- `dogan / Instructor123!`
- `viewer / Viewer123!`
