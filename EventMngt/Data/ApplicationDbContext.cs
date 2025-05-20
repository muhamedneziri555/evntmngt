using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventMngt.Models;

namespace EventMngt.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Registration> Registrations { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Event>()
            .HasOne(e => e.Organizer)
            .WithMany()
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Event>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Registration>()
            .HasOne(r => r.Event)
            .WithMany()
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Registration>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Feedback>()
            .HasOne(f => f.Event)
            .WithMany()
            .HasForeignKey(f => f.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Feedback>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 