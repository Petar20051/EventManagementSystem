﻿using EventManagementSystem.Infrastructure.Data.Entities;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventMaganementSystem.Data
{
    public class EventDbContext : IdentityDbContext
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
        public DbSet<PayPalDetails> PayPalDetails { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserEvent> UserEvents { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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



            modelBuilder.Entity<UserEvent>()
                .HasKey(ue => new { ue.UserId, ue.EventId });
        }

    }
}
