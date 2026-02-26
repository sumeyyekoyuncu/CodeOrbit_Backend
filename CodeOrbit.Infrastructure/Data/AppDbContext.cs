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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cascade delete sorunlarını önle
            modelBuilder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Quiz)
                .WithMany(q => q.QuizQuestions)
                .HasForeignKey(qq => qq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizQuestion>()
                .HasOne(qq => qq.Question)
                .WithMany()
                .HasForeignKey(qq => qq.QuestionId)
                .OnDelete(DeleteBehavior.NoAction); // ← Burada döngüyü kırıyoruz

            modelBuilder.Entity<QuizQuestion>()
                .HasOne(qq => qq.UserAnswerOption)
                .WithMany()
                .HasForeignKey(qq => qq.UserAnswerOptionId)
                .OnDelete(DeleteBehavior.NoAction); // ← Burada da

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
