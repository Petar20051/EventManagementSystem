using EventManagementSystem.Infrastructure.Data.Entities;
using EventManagementSystem.Infrastructure.Data.Enums;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventMaganementSystem.Data
{
    public class EventDbContext : IdentityDbContext<ApplicationUser>
    {
        public EventDbContext(DbContextOptions<EventDbContext> options)
            : base(options)
        {
        }


        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<CreditCardDetails> CreditCardDetails { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserEvent> UserEvents { get; set; }

        public DbSet<EventInvitation> EventInvitations { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>()
       .HasOne(t => t.Event)
       .WithMany(e => e.Tickets)
       .HasForeignKey(t => t.EventId);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.Reservations)
                .WithOne(r => r.Event)
                .HasForeignKey(r => r.EventId);
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
        .Property(e => e.SponsoredAmount)
        .HasColumnType("decimal(18, 2)");


            modelBuilder.Entity<Discount>()
                .Property(e => e.Percentage)
                .HasColumnType("decimal(5, 2)");


            modelBuilder.Entity<Payment>()
                .Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<Payment>()
       .HasOne(p => p.Reservation)
       .WithMany()
       .HasForeignKey(p => p.ReservationId)
       .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
       .Property(e => e.TicketPrice)
       .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Reservation>()
                .Property(r => r.TotalAmount)
                .HasColumnType("decimal(18, 2)");



            modelBuilder.Entity<UserEvent>()
                .HasKey(ue => new { ue.UserId, ue.EventId });

            modelBuilder.Entity<EventInvitation>()
            .HasOne(ei => ei.Sender)
            .WithMany(u => u.SentInvitations)
            .HasForeignKey(ei => ei.SenderId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete if user is deleted

            // Configure one-to-many relationship between User and ReceivedInvitations
            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.Receiver)
                .WithMany(u => u.ReceivedInvitations)
                .HasForeignKey(ei => ei.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete if user is deleted

            // Configure one-to-many relationship between Event and Invitations
            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.Event)
                .WithMany(e => e.Invitations)
                .HasForeignKey(ei => ei.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
       .HasOne(e => e.Venue)
       .WithMany(v => v.Events) // Configuring the one-to-many relationship
       .HasForeignKey(e => e.VenueId)
       .OnDelete(DeleteBehavior.Cascade); // Adjust as needed

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany() // Assuming ApplicationUser does not have a collection of Events
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = "Guest", NormalizedName = "GUEST" },
            new IdentityRole { Id = "2", Name = "Organizer", NormalizedName = "ORGANIZER" },
            new IdentityRole { Id = "3", Name = "Sponsor", NormalizedName = "SPONSOR" },
            new IdentityRole { Id = "4", Name = "Admin", NormalizedName = "ADMIN" }
        );


            // Seed Venues
            modelBuilder.Entity<Venue>().HasData(
                new Venue { Id = 1, Name = "Conference Hall A", Address = "City Center", Capacity = 500 },
                new Venue { Id = 2, Name = "Workshop Room B", Address = "Tech Park", Capacity = 150 }
            );

            
        }

    }
}
