using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Verification> Verifications { get; set; }
        public DbSet<UserInterest> UserInterests { get; set; }
        public DbSet<InterestCategory> InterestCategories { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<MeetupEvent> MeetupEvents { get; set; }
        public DbSet<UserMeetup> UserMeetups { get; set; }
        public DbSet<MeetupLocationSuggestion> MeetupLocationSuggestions { get; set; }
        public DbSet<MeetupFeedback> MeetupFeedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== CONVERSATION & MESSAGE RELATIONSHIPS ==========
            
            // Conversation double foreign key - User1
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User1)
                .WithMany(u => u.Conversations1)
                .HasForeignKey(c => c.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Conversation double foreign key - User2
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User2)
                .WithMany(u => u.Conversations2)
                .HasForeignKey(c => c.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Optional meetup link for time-limited chat around a meetup
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.MeetupEvent)
                .WithMany(m => m.Conversations)
                .HasForeignKey(c => c.MeetupEventId)
                .OnDelete(DeleteBehavior.SetNull);

            // Message sender
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            
            // ========== MEETUP EVENT RELATIONSHIPS ==========
            
            // MeetupEvent Creator
            modelBuilder.Entity<MeetupEvent>()
                .HasOne(m => m.Creator)
                .WithMany(u => u.CreatedMeetups)
                .HasForeignKey(m => m.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);

            // MeetupEvent Participants
            modelBuilder.Entity<UserMeetup>()
                .HasOne(um => um.User)
                .WithMany(u => u.JoinedMeetups)
                .HasForeignKey(um => um.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserMeetup>()
                .HasOne(um => um.MeetupEvent)
                .WithMany(m => m.Participants)
                .HasForeignKey(um => um.MeetupEventId)
                .OnDelete(DeleteBehavior.Cascade);

            // MeetupEvent-Activity one-to-many (simplified: one event = one activity)
            modelBuilder.Entity<MeetupEvent>()
                .HasOne(m => m.Activity)
                .WithMany(a => a.MeetupEvents)
                .HasForeignKey(m => m.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MeetupLocationSuggestion>()
                .HasOne(s => s.MeetupEvent)
                .WithMany(m => m.LocationSuggestions)
                .HasForeignKey(s => s.MeetupEventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MeetupLocationSuggestion>()
                .HasOne(s => s.SuggestedByUser)
                .WithMany(u => u.SuggestedLocations)
                .HasForeignKey(s => s.SuggestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MeetupFeedback>()
                .HasOne(f => f.MeetupEvent)
                .WithMany(m => m.Feedbacks)
                .HasForeignKey(f => f.MeetupEventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MeetupFeedback>()
                .HasOne(f => f.User)
                .WithMany(u => u.MeetupFeedbacks)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========== UNIQUE CONSTRAINTS & INDEXES ==========

            // User unique constraints
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            // Location-based matching indexes (for efficient queries)
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => new { u.Region, u.Suburb })
                .HasDatabaseName("IX_User_Region_Suburb");

            modelBuilder.Entity<MeetupEvent>()
                .HasIndex(m => new { m.Region, m.Suburb, m.EventDate })
                .HasDatabaseName("IX_MeetupEvent_Location_Date");

            // UserMeetup: one user per event (no duplicates)
            modelBuilder.Entity<UserMeetup>()
                .HasIndex(um => new { um.UserId, um.MeetupEventId })
                .IsUnique();

            modelBuilder.Entity<MeetupFeedback>()
                .HasIndex(f => new { f.UserId, f.MeetupEventId })
                .IsUnique();

            // ========== SEED DATA ==========

            modelBuilder.Entity<InterestCategory>().HasData(
                new InterestCategory { Id = 1, Name = "Sports" },
                new InterestCategory { Id = 2, Name = "Art" },
                new InterestCategory { Id = 3, Name = "Music" },
                new InterestCategory { Id = 4, Name = "Technology" },
                new InterestCategory { Id = 5, Name = "Gaming" },
                new InterestCategory { Id = 6, Name = "Fitness" },
                new InterestCategory { Id = 7, Name = "Travel" },
                new InterestCategory { Id = 8, Name = "Other" }
            );

            modelBuilder.Entity<Activity>().HasData(
                new Activity { Id = 1, Name = "Coffee", Description = "Coffee meetups" },
                new Activity { Id = 2, Name = "Walk", Description = "Walking and exploring" },
                new Activity { Id = 3, Name = "Gym", Description = "Fitness and workouts" },
                new Activity { Id = 4, Name = "Food", Description = "Dining and meals" },
                new Activity { Id = 5, Name = "Drinks", Description = "Drinks and hangouts" },
                new Activity { Id = 6, Name = "Explore", Description = "Explore somewhere new" }
            );
        }
    }
}