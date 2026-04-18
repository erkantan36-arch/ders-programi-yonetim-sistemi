# Ders Programı Yönetim Sistemi

ASP.NET Core MVC (.NET 8) ile geliştirilmiş üniversite ders programı yönetim sistemi.

## Özellikler

- Rol tabanlı yetkilendirme (Admin, Instructor, Viewer)
- Haftalık ders programı yönetimi (CRUD)
- Çakışma kontrolü (aynı saatte derslik/sınıf/hoca)
- Dashboard ve yönetim panelleri
- ASP.NET Core Identity ile kimlik doğrulama
- Seed data ile hızlı başlangıç

## Gereksinimler

- .NET 8 SDK
- SQL Server LocalDB veya SQL Server

## Kurulum

```bash
git clone https://github.com/erkantan36-arch/ders-programi-yonetim-sistemi.git
cd ders-programi-yonetim-sistemi
dotnet restore
dotnet ef database update
dotnet run
```

## Migration

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Varsayılan Giriş Bilgileri

- Admin: `admin@ders.com` / `Admin@123`
- Instructor: `ozlem@ders.com` / `User@123`
- Viewer: `viewer@ders.com` / `User@123`

## Kullanım

1. Admin hesabı ile giriş yapın.
2. Admin panelinden tanımları kontrol edin.
3. Ders programı ekranından yeni kayıt oluşturun.
4. Instructor kullanıcıları kendi profili ve programını görüntüleyebilir.
