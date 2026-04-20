using ders_programi_yonetim_sistemi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ders_programi_yonetim_sistemi.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (await context.Database.CanConnectAsync())
        {
            var hasDaysTable = await TableExistsAsync(context, "Days");
            var hasIdentityTable = await TableExistsAsync(context, "AspNetUsers");

            if (!hasDaysTable && hasIdentityTable)
            {
                await context.Database.EnsureDeletedAsync();
            }
        }

        await context.Database.MigrateAsync();

        foreach (var role in new[] { "Admin", "Instructor", "Viewer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (!await context.Days.AnyAsync())
        {
            var days = new[] { "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma" };
            context.Days.AddRange(days.Select((d, i) => new Day { Id = i + 1, Name = d }));
        }

        if (!await context.Classrooms.AnyAsync())
        {
            var classrooms = new[]
            {
                "Anfi-1", "Anfi-2", "Anfi-3", "Anfi-4",
                "Derslik-1", "Derslik-2", "Derslik-3", "Derslik-4", "Derslik-5", "Derslik-6", "Derslik-7", "Derslik-8"
            };
            context.Classrooms.AddRange(classrooms.Select((c, i) => new Classroom { Id = i + 1, Name = c }));
        }

        if (!await context.TimeSlots.AnyAsync())
        {
            context.TimeSlots.AddRange(new[]
            {
                new TimeSlot { Id = 1, StartTime = new TimeOnly(8,0), EndTime = new TimeOnly(8,45), DisplayName = "08:00 - 08:45" },
                new TimeSlot { Id = 2, StartTime = new TimeOnly(9,0), EndTime = new TimeOnly(9,45), DisplayName = "09:00 - 09:45" },
                new TimeSlot { Id = 3, StartTime = new TimeOnly(10,0), EndTime = new TimeOnly(10,45), DisplayName = "10:00 - 10:45" },
                new TimeSlot { Id = 4, StartTime = new TimeOnly(13,0), EndTime = new TimeOnly(13,45), DisplayName = "13:00 - 13:45" },
                new TimeSlot { Id = 5, StartTime = new TimeOnly(14,0), EndTime = new TimeOnly(14,45), DisplayName = "14:00 - 14:45" },
                new TimeSlot { Id = 6, StartTime = new TimeOnly(15,0), EndTime = new TimeOnly(15,45), DisplayName = "15:00 - 15:45" },
                new TimeSlot { Id = 7, StartTime = new TimeOnly(16,0), EndTime = new TimeOnly(16,45), DisplayName = "16:00 - 16:45" }
            });
        }

        if (!await context.Classes.AnyAsync())
        {
            var classes = new[] { "1-A", "1-B", "2-A", "2-B", "3-A", "3-B", "4-A", "4-B" };
            context.Classes.AddRange(classes.Select((c, i) => new ClassModel { Id = i + 1, Name = c }));
        }

        if (!await context.Instructors.AnyAsync())
        {
            context.Instructors.AddRange(new[]
            {
                new Instructor { Id = 1, FullName = "Prof. Dr. Özlem KARABULUTLU" },
                new Instructor { Id = 2, FullName = "Doç. Dr. Doğan AKÇA" },
                new Instructor { Id = 3, FullName = "Dr. Öğr. Üyesi Cansu MİNE AYDIN" }
            });
        }

        await context.SaveChangesAsync();

        if (!await context.Courses.AnyAsync())
        {
            context.Courses.AddRange(new[]
            {
                new Course { Name = "Mikrobiyoloji", InstructorId = 1 },
                new Course { Name = "Türk Dili", InstructorId = 2 },
                new Course { Name = "Parazitoloji", InstructorId = 3 },
                new Course { Name = "Patoloji", InstructorId = 1 }
            });
            await context.SaveChangesAsync();
        }

        await EnsureUserAsync(userManager, "admin", "admin@dpy.com", "Admin123!", "Admin", null, "Sistem Yöneticisi");
        await EnsureUserAsync(userManager, "ozlem", "ozlem@dpy.com", "Instructor123!", "Instructor", 1, "Prof. Dr. Özlem KARABULUTLU");
        await EnsureUserAsync(userManager, "dogan", "dogan@dpy.com", "Instructor123!", "Instructor", 2, "Doç. Dr. Doğan AKÇA");
        await EnsureUserAsync(userManager, "viewer", "viewer@dpy.com", "Viewer123!", "Viewer", null, "Görüntüleyici Kullanıcı");

        var usersWithInstructors = await context.Users
            .Where(u => u.InstructorId.HasValue)
            .Select(u => new { u.Id, InstructorId = u.InstructorId!.Value })
            .ToListAsync();

        foreach (var item in usersWithInstructors)
        {
            var instructor = await context.Instructors.FindAsync(item.InstructorId);
            if (instructor != null && string.IsNullOrEmpty(instructor.UserId))
            {
                instructor.UserId = item.Id;
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task<bool> TableExistsAsync(ApplicationDbContext context, string tableName)
    {
        await using var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=$name";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "$name";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string userName,
        string email,
        string password,
        string role,
        int? instructorId,
        string fullName)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                InstructorId = instructorId
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Kullanıcı oluşturulamadı: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
