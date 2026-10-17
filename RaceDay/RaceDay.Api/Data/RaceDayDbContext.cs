using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<Organiser> Organisers => Set<Organiser>();

    public DbSet<Participant> Participants => Set<Participant>();

    public DbSet<RaceEvent> Events => Set<RaceEvent>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<RaceResult> Results => Set<RaceResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USER TABLE
        modelBuilder.Entity<AppUser>()
            .ToTable("USER");

        modelBuilder.Entity<AppUser>()
            .HasKey(u => u.UserId);

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<AppUser>()
            .Property(u => u.UserType)
            .IsRequired()
            .HasMaxLength(20);

        // ORGANISERS
        modelBuilder.Entity<Organiser>()
            .ToTable("ORGANISERS");

        modelBuilder.Entity<Organiser>()
            .HasKey(o => o.OrganiserId);

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Organiser)
            .WithMany(o => o.Users)
            .HasForeignKey(u => u.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);

        // PARTICIPANTS
        modelBuilder.Entity<Participant>()
            .ToTable("PARTICIPANTS");

        modelBuilder.Entity<Participant>()
            .HasKey(p => p.ParticipantId);

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Participant)
            .WithMany(p => p.Users)
            .HasForeignKey(u => u.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        // EVENTS
        modelBuilder.Entity<RaceEvent>()
            .ToTable("EVENTS");

        modelBuilder.Entity<RaceEvent>()
            .HasKey(e => e.EventId);

        modelBuilder.Entity<RaceEvent>()
            .HasOne(e => e.Organiser)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);

        // CATEGORIES
        modelBuilder.Entity<Category>()
            .ToTable("CATEGORY");

        modelBuilder.Entity<Category>()
            .HasKey(c => c.CategoryId);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // ENROLLMENTS
        modelBuilder.Entity<Enrollment>()
            .ToTable("ENROLLMENT");

        modelBuilder.Entity<Enrollment>()
            .HasKey(e => e.EnrollmentId);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Participant)
            .WithMany(p => p.Enrollments)
            .HasForeignKey(e => e.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Event)
            .WithMany(e => e.Enrollments)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.ParticipantId, e.EventId })
            .IsUnique();

        // RESULTS
        modelBuilder.Entity<RaceResult>()
            .ToTable("RESULTS");

        modelBuilder.Entity<RaceResult>()
            .HasKey(r => r.ResultId);

        modelBuilder.Entity<RaceResult>()
            .HasIndex(r => r.EnrollmentId)
            .IsUnique();

        modelBuilder.Entity<RaceResult>()
            .HasOne(r => r.Enrollment)
            .WithOne(e => e.Result)
            .HasForeignKey<RaceResult>(r => r.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RaceEvent>()
            .Property(e => e.Distance)
            .HasPrecision(10, 2);
    }
}