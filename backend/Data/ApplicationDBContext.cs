using api.Models;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Conversation double foreign key
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User1)
                .WithMany()
                .HasForeignKey(c => c.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User2)
                .WithMany()
                .HasForeignKey(c => c.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Message sender
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraints
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();

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
        }
    }
}