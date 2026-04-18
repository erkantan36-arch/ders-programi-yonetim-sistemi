using ders_programi_yonetim_sistemi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ders_programi_yonetim_sistemi.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Day> Days => Set<Day>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<ClassModel> Classes => Set<ClassModel>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Schedule> Schedules => Set<Schedule>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Day>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<Classroom>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<TimeSlot>().HasIndex(x => new { x.StartTime, x.EndTime }).IsUnique();
        builder.Entity<ClassModel>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<Course>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<Instructor>().HasIndex(x => x.Email).IsUnique();

        builder.Entity<Instructor>()
            .HasOne(i => i.ApplicationUser)
            .WithMany()
            .HasForeignKey(i => i.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Schedule>()
            .HasOne(s => s.Day)
            .WithMany(d => d.Schedules)
            .HasForeignKey(s => s.DayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>()
            .HasOne(s => s.TimeSlot)
            .WithMany(t => t.Schedules)
            .HasForeignKey(s => s.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>()
            .HasOne(s => s.Classroom)
            .WithMany(c => c.Schedules)
            .HasForeignKey(s => s.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>()
            .HasOne(s => s.ClassModel)
            .WithMany(c => c.Schedules)
            .HasForeignKey(s => s.ClassModelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>()
            .HasOne(s => s.Course)
            .WithMany(c => c.Schedules)
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>()
            .HasOne(s => s.Instructor)
            .WithMany(i => i.Schedules)
            .HasForeignKey(s => s.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>().HasIndex(s => new { s.DayId, s.TimeSlotId, s.ClassroomId }).IsUnique();
        builder.Entity<Schedule>().HasIndex(s => new { s.DayId, s.TimeSlotId, s.ClassModelId }).IsUnique();
        builder.Entity<Schedule>().HasIndex(s => new { s.DayId, s.TimeSlotId, s.InstructorId }).IsUnique();
    }
}
