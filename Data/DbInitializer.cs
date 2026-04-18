using ders_programi_yonetim_sistemi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ders_programi_yonetim_sistemi.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (context.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
        {
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            await context.Database.MigrateAsync();
        }

        var roles = new[] { "Admin", "Instructor", "Viewer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (!await context.Days.AnyAsync())
        {
            var days = new[]
            {
                new Day { Name = "Pazartesi", Order = 1 },
                new Day { Name = "Salı", Order = 2 },
                new Day { Name = "Çarşamba", Order = 3 },
                new Day { Name = "Perşembe", Order = 4 },
                new Day { Name = "Cuma", Order = 5 }
            };
            await context.Days.AddRangeAsync(days);
        }

        if (!await context.Classrooms.AnyAsync())
        {
            var classrooms = new List<Classroom>();
            for (var i = 1; i <= 4; i++)
            {
                classrooms.Add(new Classroom { Name = $"Anfi-{i}", Capacity = 120, IsAmphi = true });
            }

            for (var i = 1; i <= 8; i++)
            {
                classrooms.Add(new Classroom { Name = $"Derslik-{i}", Capacity = 45, IsAmphi = false });
            }

            await context.Classrooms.AddRangeAsync(classrooms);
        }

        if (!await context.TimeSlots.AnyAsync())
        {
            var slots = new[]
            {
                new TimeSlot { StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(8,45), Label = "08:00-08:45" },
                new TimeSlot { StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(9,45), Label = "09:00-09:45" },
                new TimeSlot { StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(10,45), Label = "10:00-10:45" },
                new TimeSlot { StartTime = new TimeOnly(11, 0), EndTime = new TimeOnly(11,45), Label = "11:00-11:45" },
                new TimeSlot { StartTime = new TimeOnly(13, 0), EndTime = new TimeOnly(13,45), Label = "13:00-13:45" },
                new TimeSlot { StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(14,45), Label = "14:00-14:45" },
                new TimeSlot { StartTime = new TimeOnly(15, 0), EndTime = new TimeOnly(15,45), Label = "15:00-15:45" }
            };
            await context.TimeSlots.AddRangeAsync(slots);
        }

        if (!await context.Classes.AnyAsync())
        {
            await context.Classes.AddRangeAsync(new[]
            {
                new ClassModel { Name = "1-A" }, new ClassModel { Name = "1-B" },
                new ClassModel { Name = "2-A" }, new ClassModel { Name = "2-B" },
                new ClassModel { Name = "3-A" }, new ClassModel { Name = "3-B" },
                new ClassModel { Name = "4-A" }, new ClassModel { Name = "4-B" }
            });
        }

        if (!await context.Courses.AnyAsync())
        {
            await context.Courses.AddRangeAsync(new[]
            {
                new Course { Name = "Mikrobiyoloji", Code = "MBY101", Credit = 3 },
                new Course { Name = "Türk Dili", Code = "TRD102", Credit = 2 },
                new Course { Name = "Parazitoloji", Code = "PRZ201", Credit = 3 },
                new Course { Name = "Patoloji", Code = "PAT202", Credit = 3 },
                new Course { Name = "Biyokimya", Code = "BYK203", Credit = 3 },
                new Course { Name = "Farmakoloji", Code = "FRM301", Credit = 4 }
            });
        }

        await context.SaveChangesAsync();

        var userSeeds = new[]
        {
            new ApplicationUser { UserName = "admin@ders.com", Email = "admin@ders.com", EmailConfirmed = true, FullName = "Sistem Yöneticisi", Title = "Admin" , Department = "Yönetim" },
            new ApplicationUser { UserName = "ozlem@ders.com", Email = "ozlem@ders.com", EmailConfirmed = true, FullName = "Özlem Kaya", Title = "Prof. Dr.", Department = "Veterinerlik" },
            new ApplicationUser { UserName = "dogan@ders.com", Email = "dogan@ders.com", EmailConfirmed = true, FullName = "Doğan Demir", Title = "Doç. Dr.", Department = "Veterinerlik" },
            new ApplicationUser { UserName = "ayse@ders.com", Email = "ayse@ders.com", EmailConfirmed = true, FullName = "Ayşe Işık", Title = "Dr. Öğr. Üyesi", Department = "Veterinerlik" },
            new ApplicationUser { UserName = "viewer@ders.com", Email = "viewer@ders.com", EmailConfirmed = true, FullName = "Gözlem Kullanıcı", Title = "Öğrenci", Department = "Hazırlık" }
        };

        foreach (var seed in userSeeds)
        {
            var existing = await userManager.FindByEmailAsync(seed.Email!);
            if (existing is null)
            {
                var createResult = await userManager.CreateAsync(seed, seed.Email!.StartsWith("admin") ? "Admin@123" : "User@123");
                if (!createResult.Succeeded)
                {
                    continue;
                }
            }
        }

        async Task AssignRoleAsync(string email, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is not null && !await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

        await AssignRoleAsync("admin@ders.com", "Admin");
        await AssignRoleAsync("ozlem@ders.com", "Instructor");
        await AssignRoleAsync("dogan@ders.com", "Instructor");
        await AssignRoleAsync("ayse@ders.com", "Instructor");
        await AssignRoleAsync("viewer@ders.com", "Viewer");

        if (!await context.Instructors.AnyAsync())
        {
            var instructors = new[]
            {
                new Instructor { FullName = "Özlem Kaya", Title = "Prof. Dr.", Email = "ozlem@ders.com", PhoneNumber = "05550000001", ApplicationUserId = (await userManager.FindByEmailAsync("ozlem@ders.com"))?.Id },
                new Instructor { FullName = "Doğan Demir", Title = "Doç. Dr.", Email = "dogan@ders.com", PhoneNumber = "05550000002", ApplicationUserId = (await userManager.FindByEmailAsync("dogan@ders.com"))?.Id },
                new Instructor { FullName = "Ayşe Işık", Title = "Dr. Öğr. Üyesi", Email = "ayse@ders.com", PhoneNumber = "05550000003", ApplicationUserId = (await userManager.FindByEmailAsync("ayse@ders.com"))?.Id },
                new Instructor { FullName = "Murat Yılmaz", Title = "Prof. Dr.", Email = "murat@ders.com", PhoneNumber = "05550000004" },
                new Instructor { FullName = "Zehra Tunç", Title = "Doç. Dr.", Email = "zehra@ders.com", PhoneNumber = "05550000005" },
                new Instructor { FullName = "Can Aksoy", Title = "Dr. Öğr. Üyesi", Email = "can@ders.com", PhoneNumber = "05550000006" }
            };
            await context.Instructors.AddRangeAsync(instructors);
            await context.SaveChangesAsync();
        }

        if (!await context.Schedules.AnyAsync())
        {
            var days = await context.Days.OrderBy(d => d.Order).ToListAsync();
            var slots = await context.TimeSlots.OrderBy(t => t.StartTime).ToListAsync();
            var classrooms = await context.Classrooms.OrderBy(c => c.Name).ToListAsync();
            var classes = await context.Classes.OrderBy(c => c.Name).ToListAsync();
            var courses = await context.Courses.OrderBy(c => c.Name).ToListAsync();
            var instructors = await context.Instructors.OrderBy(i => i.FullName).ToListAsync();

            var schedules = new[]
            {
                new Schedule { DayId = days[0].Id, TimeSlotId = slots[0].Id, ClassroomId = classrooms[0].Id, ClassModelId = classes[0].Id, CourseId = courses[0].Id, InstructorId = instructors[0].Id },
                new Schedule { DayId = days[1].Id, TimeSlotId = slots[1].Id, ClassroomId = classrooms[1].Id, ClassModelId = classes[1].Id, CourseId = courses[1].Id, InstructorId = instructors[1].Id },
                new Schedule { DayId = days[2].Id, TimeSlotId = slots[2].Id, ClassroomId = classrooms[2].Id, ClassModelId = classes[2].Id, CourseId = courses[2].Id, InstructorId = instructors[2].Id },
                new Schedule { DayId = days[3].Id, TimeSlotId = slots[3].Id, ClassroomId = classrooms[3].Id, ClassModelId = classes[3].Id, CourseId = courses[3].Id, InstructorId = instructors[3].Id },
                new Schedule { DayId = days[4].Id, TimeSlotId = slots[4].Id, ClassroomId = classrooms[4].Id, ClassModelId = classes[4].Id, CourseId = courses[4].Id, InstructorId = instructors[4].Id }
            };
            await context.Schedules.AddRangeAsync(schedules);
            await context.SaveChangesAsync();
        }
    }
}
