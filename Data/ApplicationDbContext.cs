using ders_programi_yonetim_sistemi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ders_programi_yonetim_sistemi.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
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
        builder.Entity<ClassModel>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<Instructor>().HasIndex(x => x.UserId).IsUnique();

        builder.Entity<Course>()
            .HasOne(x => x.Instructor)
            .WithMany(x => x.Courses)
            .HasForeignKey(x => x.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>()
            .HasOne(x => x.Day)
            .WithMany(x => x.Schedules)
            .HasForeignKey(x => x.DayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>()
            .HasOne(x => x.TimeSlot)
            .WithMany(x => x.Schedules)
            .HasForeignKey(x => x.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>()
            .HasOne(x => x.Classroom)
            .WithMany(x => x.Schedules)
            .HasForeignKey(x => x.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>()
            .HasOne(x => x.ClassModel)
            .WithMany(x => x.Schedules)
            .HasForeignKey(x => x.ClassModelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>()
            .HasOne(x => x.Course)
            .WithMany(x => x.Schedules)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Schedule>()
            .HasOne(x => x.Instructor)
            .WithMany(x => x.Schedules)
            .HasForeignKey(x => x.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .HasOne(x => x.Instructor)
            .WithMany()
            .HasForeignKey(x => x.InstructorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Instructor>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
