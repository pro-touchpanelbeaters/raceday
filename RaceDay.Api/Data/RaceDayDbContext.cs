using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Entities;

namespace RaceDay.Api.Data;

public class RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<EventImage> EventImages => Set<EventImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.UserId);
            e.Property(x => x.Email).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(50).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.PhoneNumber).HasMaxLength(20);
            e.Property(x => x.ProfileImageUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<Event>(e =>
        {
            e.HasKey(x => x.EventId);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Location).HasMaxLength(200).IsRequired();
            e.Property(x => x.Distance).HasPrecision(6, 2).IsRequired();
            e.Property(x => x.EventType).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(x => x.BannerImageUrl).HasMaxLength(500);
            e.HasOne(x => x.Organiser).WithMany(x => x.OrganisedEvents).HasForeignKey(x => x.OrganiserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(x => x.CategoryId);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Distance).HasPrecision(6, 2);
            e.HasOne(x => x.Event).WithMany(x => x.Categories).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.EventId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<Enrolment>(e =>
        {
            e.HasKey(x => x.EnrolmentId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Participant).WithMany(x => x.Enrolments).HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Event).WithMany(x => x.Enrolments).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Category).WithMany(x => x.Enrolments).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.ParticipantId, x.EventId }).IsUnique();
        });

        modelBuilder.Entity<Result>(e =>
        {
            e.HasKey(x => x.ResultId);
            e.HasIndex(x => x.EnrolmentId).IsUnique();
            e.HasOne(x => x.Enrolment).WithOne(x => x.Result).HasForeignKey<Result>(x => x.EnrolmentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EventImage>(e =>
        {
            e.HasKey(x => x.EventImageId);
            e.Property(x => x.BlobName).HasMaxLength(300).IsRequired();
            e.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.Event).WithMany(x => x.Images).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
