using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeOrbit.Domain.Entities;
using CodeOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CodeOrbit.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<UserProgress> UserProgresses { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<UserStreak> UserStreaks { get; set; }
        public DbSet<FavoriteQuestion> FavoriteQuestions { get; set; }
        public DbSet<DailyChallenge> DailyChallenges { get; set; }
        public DbSet<DailyChallengeQuestion> DailyChallengeQuestions { get; set; }
        public DbSet<UserChallengeAttempt> UserChallengeAttempts { get; set; }
        public DbSet<UserChallengeAnswer> UserChallengeAnswers { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<UserBadge> UserBadges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Bir kullanıcı aynı rozeti 2 kez kazanamaz
            modelBuilder.Entity<UserBadge>()
                .HasIndex(ub => new { ub.UserId, ub.BadgeId })
                .IsUnique();
            // FriendRequest ilişkileri
            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Sender)
                .WithMany()
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany()
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            // Aynı kullanıcıya birden fazla pending istek gönderilemez
            modelBuilder.Entity<FriendRequest>()
                .HasIndex(fr => new { fr.SenderId, fr.ReceiverId, fr.Status })
                .IsUnique();

            // Friendship ilişkileri
            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User1)
                .WithMany()
                .HasForeignKey(f => f.User1Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User2)
                .WithMany()
                .HasForeignKey(f => f.User2Id)
                .OnDelete(DeleteBehavior.NoAction);

            // Aynı arkadaşlık iki kez oluşturulamaz
            modelBuilder.Entity<Friendship>()
                .HasIndex(f => new { f.User1Id, f.User2Id })
                .IsUnique();
            // QuizQuestion cascade ayarları (mevcut)
            modelBuilder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Quiz)
                .WithMany(q => q.QuizQuestions)
                .HasForeignKey(qq => qq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Question)
                .WithMany()
                .HasForeignKey(qq => qq.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<QuizQuestion>()
                .HasOne(qq => qq.UserAnswerOption)
                .WithMany()
                .HasForeignKey(qq => qq.UserAnswerOptionId)
                .OnDelete(DeleteBehavior.NoAction);

            // DailyChallengeQuestion cascade ayarları (YENİ)
            modelBuilder.Entity<DailyChallengeQuestion>()
                .HasOne(dcq => dcq.DailyChallenge)
                .WithMany(dc => dc.Questions)
                .HasForeignKey(dcq => dcq.DailyChallengeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyChallengeQuestion>()
                .HasOne(dcq => dcq.Question)
                .WithMany()
                .HasForeignKey(dcq => dcq.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            // UserChallengeAnswer cascade ayarları (mevcut ama tekrar ekle)
            modelBuilder.Entity<UserChallengeAnswer>()
                .HasOne(a => a.Attempt)
                .WithMany(u => u.Answers)
                .HasForeignKey(a => a.UserChallengeAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserChallengeAnswer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserChallengeAnswer>()
                .HasOne(a => a.SelectedOption)
                .WithMany()
                .HasForeignKey(a => a.SelectedOptionId)
                .OnDelete(DeleteBehavior.NoAction);

            // Unique constraint (mevcut)
            modelBuilder.Entity<UserChallengeAttempt>()
                .HasIndex(u => new { u.UserId, u.DailyChallengeId })
                .IsUnique();

            modelBuilder.Entity<FavoriteQuestion>()
                .HasIndex(f => new { f.UserId, f.QuestionId })
                .IsUnique();
            // Seed Badges
            modelBuilder.Entity<Badge>().HasData(
                new Badge { Id = 1, Name = "İlk Adım", Description = "İlk quiz'ini tamamla", Icon = "🎯", Requirement = "Complete1Quiz", RequiredCount = 1 },
                new Badge { Id = 2, Name = "Meraklı", Description = "10 quiz tamamla", Icon = "📚", Requirement = "Complete10Quizzes", RequiredCount = 10 },
                new Badge { Id = 3, Name = "Uzman", Description = "50 quiz tamamla", Icon = "🎓", Requirement = "Complete50Quizzes", RequiredCount = 50 },
                new Badge { Id = 4, Name = "Mükemmeliyetçi", Description = "Bir quiz'de 100% başarı", Icon = "💯", Requirement = "PerfectScore", RequiredCount = 1 },
                new Badge { Id = 5, Name = "Hızlı", Description = "7 günlük streak", Icon = "⚡", Requirement = "Streak7Days", RequiredCount = 7 },
                new Badge { Id = 6, Name = "Ateş Topu", Description = "30 günlük streak", Icon = "🔥", Requirement = "Streak30Days", RequiredCount = 30 },
                new Badge { Id = 7, Name = "Sosyal", Description = "5 arkadaş edin", Icon = "👥", Requirement = "Have5Friends", RequiredCount = 5 },
                new Badge { Id = 8, Name = "Challenge Şampiyonu", Description = "10 günlük challenge tamamla", Icon = "🏆", Requirement = "Complete10Challenges", RequiredCount = 10 }
            );
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "C#", Language = ProgrammingLanguage.CSharp },
                new Category { Id = 2, Name = "Java", Language = ProgrammingLanguage.Java },
                new Category { Id = 3, Name = "Kotlin", Language = ProgrammingLanguage.Kotlin },
                new Category { Id = 4, Name = "Python", Language = ProgrammingLanguage.Python },
                new Category { Id = 5, Name = "JavaScript", Language = ProgrammingLanguage.JavaScript }
            );
        }
    }
}
