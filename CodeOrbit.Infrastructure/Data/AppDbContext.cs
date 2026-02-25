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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "C#", Language = ProgrammingLanguage.CSharp },
                new Category { Id = 2, Name = "Java", Language = ProgrammingLanguage.Java },
                new Category { Id = 3, Name = "Kotlin", Language = ProgrammingLanguage.Kotlin },
                new Category { Id = 4, Name = "Python", Language = ProgrammingLanguage.Python },
                new Category { Id = 5, Name = "JavaScript", Language = ProgrammingLanguage.JavaScript }
            );

            // Seed Questions
            modelBuilder.Entity<Question>().HasData(
                // C# - Çoktan Seçmeli
                new Question { Id = 1, CategoryId = 1, QuestionText = "C#'ta değişken nasıl tanımlanır?", QuestionType = QuestionType.MultipleChoice, DifficultyLevel = DifficultyLevel.Easy },
                new Question { Id = 2, CategoryId = 1, QuestionText = "C#'ta LINQ nedir?", QuestionType = QuestionType.MultipleChoice, DifficultyLevel = DifficultyLevel.Medium },
                // C# - Doğru/Yanlış
                new Question { Id = 3, CategoryId = 1, QuestionText = "C# bir nesne yönelimli dildir.", QuestionType = QuestionType.TrueFalse, DifficultyLevel = DifficultyLevel.Easy },
                // C# - Boşluk Doldurma
                new Question { Id = 4, CategoryId = 1, QuestionText = "C#'ta bir listeyi tanımlamak için ____<int> list = new() kullanılır.", QuestionType = QuestionType.FillInTheBlank, DifficultyLevel = DifficultyLevel.Medium },
                // C# - Cümle Tamamlama
                new Question { Id = 5, CategoryId = 1, QuestionText = "C#'ta try bloğundan sonra gelen blok...", QuestionType = QuestionType.CompleteSentence, DifficultyLevel = DifficultyLevel.Easy },

                // Java - Çoktan Seçmeli
                new Question { Id = 6, CategoryId = 2, QuestionText = "Java'da interface nedir?", QuestionType = QuestionType.MultipleChoice, DifficultyLevel = DifficultyLevel.Medium },
                new Question { Id = 7, CategoryId = 2, QuestionText = "Java'da generic nedir?", QuestionType = QuestionType.MultipleChoice, DifficultyLevel = DifficultyLevel.Hard },
                // Java - Doğru/Yanlış
                new Question { Id = 8, CategoryId = 2, QuestionText = "Java'da çoklu kalıtım doğrudan desteklenir.", QuestionType = QuestionType.TrueFalse, DifficultyLevel = DifficultyLevel.Medium },
                // Java - Boşluk Doldurma
                new Question { Id = 9, CategoryId = 2, QuestionText = "Java'da sabit tanımlamak için ____ anahtar kelimesi kullanılır.", QuestionType = QuestionType.FillInTheBlank, DifficultyLevel = DifficultyLevel.Easy },
                // Java - Cümle Tamamlama
                new Question { Id = 10, CategoryId = 2, QuestionText = "Java'da bir sınıfın başka bir sınıftan türemesi için...", QuestionType = QuestionType.CompleteSentence, DifficultyLevel = DifficultyLevel.Easy },

                // Kotlin
                new Question { Id = 11, CategoryId = 3, QuestionText = "Kotlin'de değişken nasıl tanımlanır?", QuestionType = QuestionType.MultipleChoice, DifficultyLevel = DifficultyLevel.Easy },
                new Question { Id = 12, CategoryId = 3, QuestionText = "Kotlin'de null kontrolü nasıl yapılır?", QuestionType = QuestionType.MultipleChoice, DifficultyLevel = DifficultyLevel.Medium },
                new Question { Id = 13, CategoryId = 3, QuestionText = "Kotlin, Java ile %100 uyumludur.", QuestionType = QuestionType.TrueFalse, DifficultyLevel = DifficultyLevel.Easy },
                new Question { Id = 14, CategoryId = 3, QuestionText = "Kotlin'de değiştirilemez değişken tanımlamak için ____ kullanılır.", QuestionType = QuestionType.FillInTheBlank, DifficultyLevel = DifficultyLevel.Easy },

                // Python
                new Question { Id = 15, CategoryId = 4, QuestionText = "Python'da liste nasıl tanımlanır?", QuestionType = QuestionType.MultipleChoice, DifficultyLevel = DifficultyLevel.Easy },
                new Question { Id = 16, CategoryId = 4, QuestionText = "Python yorumlanan bir dildir.", QuestionType = QuestionType.TrueFalse, DifficultyLevel = DifficultyLevel.Easy },
                new Question { Id = 17, CategoryId = 4, QuestionText = "Python'da fonksiyon tanımlamak için ____ anahtar kelimesi kullanılır.", QuestionType = QuestionType.FillInTheBlank, DifficultyLevel = DifficultyLevel.Easy },

                // JavaScript
                new Question { Id = 18, CategoryId = 5, QuestionText = "JavaScript'te değişken tanımlamak için hangi anahtar kelimeler kullanılır?", QuestionType = QuestionType.MultipleChoice, DifficultyLevel = DifficultyLevel.Easy },
                new Question { Id = 19, CategoryId = 5, QuestionText = "JavaScript yalnızca tarayıcıda çalışır.", QuestionType = QuestionType.TrueFalse, DifficultyLevel = DifficultyLevel.Medium },
                new Question { Id = 20, CategoryId = 5, QuestionText = "JavaScript'te bir fonksiyon tanımlamak için ____ anahtar kelimesi kullanılır.", QuestionType = QuestionType.FillInTheBlank, DifficultyLevel = DifficultyLevel.Easy }
            );

            // Seed Options
            modelBuilder.Entity<Option>().HasData(
                // Soru 1 - C# değişken
                new Option { Id = 1, QuestionId = 1, OptionText = "int x = 5;", IsCorrect = true },
                new Option { Id = 2, QuestionId = 1, OptionText = "var x = 5;", IsCorrect = false },
                new Option { Id = 3, QuestionId = 1, OptionText = "let x = 5;", IsCorrect = false },
                new Option { Id = 4, QuestionId = 1, OptionText = "x := 5;", IsCorrect = false },

                // Soru 2 - LINQ
                new Option { Id = 5, QuestionId = 2, OptionText = "Veri sorgulama yöntemi", IsCorrect = true },
                new Option { Id = 6, QuestionId = 2, OptionText = "Yeni sınıf oluşturma", IsCorrect = false },
                new Option { Id = 7, QuestionId = 2, OptionText = "Exception handling", IsCorrect = false },
                new Option { Id = 8, QuestionId = 2, OptionText = "GUI tasarımı", IsCorrect = false },

                // Soru 3 - Doğru/Yanlış
                new Option { Id = 9, QuestionId = 3, OptionText = "Doğru", IsCorrect = true },
                new Option { Id = 10, QuestionId = 3, OptionText = "Yanlış", IsCorrect = false },

                // Soru 4 - Boşluk doldurma
                new Option { Id = 11, QuestionId = 4, OptionText = "List", IsCorrect = true },

                // Soru 5 - Cümle tamamlama
                new Option { Id = 12, QuestionId = 5, OptionText = "catch veya finally bloğu gelir", IsCorrect = true },
                new Option { Id = 13, QuestionId = 5, OptionText = "else bloğu gelir", IsCorrect = false },
                new Option { Id = 14, QuestionId = 5, OptionText = "void bloğu gelir", IsCorrect = false },

                // Soru 6 - Java interface
                new Option { Id = 15, QuestionId = 6, OptionText = "Sınıfların şablonu", IsCorrect = true },
                new Option { Id = 16, QuestionId = 6, OptionText = "Yalnızca değişken tutar", IsCorrect = false },
                new Option { Id = 17, QuestionId = 6, OptionText = "Exception türü", IsCorrect = false },
                new Option { Id = 18, QuestionId = 6, OptionText = "Sadece static metodlar içerir", IsCorrect = false },

                // Soru 7 - Java generic
                new Option { Id = 19, QuestionId = 7, OptionText = "Tip güvenli veri yapısı", IsCorrect = true },
                new Option { Id = 20, QuestionId = 7, OptionText = "Exception handling yöntemi", IsCorrect = false },
                new Option { Id = 21, QuestionId = 7, OptionText = "GUI tasarım yöntemi", IsCorrect = false },
                new Option { Id = 22, QuestionId = 7, OptionText = "Sadece class", IsCorrect = false },

                // Soru 8 - Java çoklu kalıtım
                new Option { Id = 23, QuestionId = 8, OptionText = "Doğru", IsCorrect = false },
                new Option { Id = 24, QuestionId = 8, OptionText = "Yanlış", IsCorrect = true },

                // Soru 9 - Java final
                new Option { Id = 25, QuestionId = 9, OptionText = "final", IsCorrect = true },

                // Soru 10 - Java extends
                new Option { Id = 26, QuestionId = 10, OptionText = "extends anahtar kelimesi kullanılır", IsCorrect = true },
                new Option { Id = 27, QuestionId = 10, OptionText = "implements anahtar kelimesi kullanılır", IsCorrect = false },
                new Option { Id = 28, QuestionId = 10, OptionText = "inherits anahtar kelimesi kullanılır", IsCorrect = false },

                // Soru 11 - Kotlin değişken
                new Option { Id = 29, QuestionId = 11, OptionText = "val x = 5", IsCorrect = true },
                new Option { Id = 30, QuestionId = 11, OptionText = "int x = 5", IsCorrect = false },
                new Option { Id = 31, QuestionId = 11, OptionText = "let x = 5", IsCorrect = false },
                new Option { Id = 32, QuestionId = 11, OptionText = "var x: Int = 5", IsCorrect = false },

                // Soru 12 - Kotlin null
                new Option { Id = 33, QuestionId = 12, OptionText = "?.", IsCorrect = true },
                new Option { Id = 34, QuestionId = 12, OptionText = "?", IsCorrect = false },
                new Option { Id = 35, QuestionId = 12, OptionText = "!!", IsCorrect = false },
                new Option { Id = 36, QuestionId = 12, OptionText = "null", IsCorrect = false },

                // Soru 13 - Kotlin Java uyumu
                new Option { Id = 37, QuestionId = 13, OptionText = "Doğru", IsCorrect = true },
                new Option { Id = 38, QuestionId = 13, OptionText = "Yanlış", IsCorrect = false },

                // Soru 14 - Kotlin val
                new Option { Id = 39, QuestionId = 14, OptionText = "val", IsCorrect = true },

                // Soru 15 - Python liste
                new Option { Id = 40, QuestionId = 15, OptionText = "list = []", IsCorrect = true },
                new Option { Id = 41, QuestionId = 15, OptionText = "list = {}", IsCorrect = false },
                new Option { Id = 42, QuestionId = 15, OptionText = "list = ()", IsCorrect = false },
                new Option { Id = 43, QuestionId = 15, OptionText = "list = <>", IsCorrect = false },

                // Soru 16 - Python yorumlanan
                new Option { Id = 44, QuestionId = 16, OptionText = "Doğru", IsCorrect = true },
                new Option { Id = 45, QuestionId = 16, OptionText = "Yanlış", IsCorrect = false },

                // Soru 17 - Python def
                new Option { Id = 46, QuestionId = 17, OptionText = "def", IsCorrect = true },

                // Soru 18 - JS değişken
                new Option { Id = 47, QuestionId = 18, OptionText = "var, let, const", IsCorrect = true },
                new Option { Id = 48, QuestionId = 18, OptionText = "int, string, bool", IsCorrect = false },
                new Option { Id = 49, QuestionId = 18, OptionText = "val, var", IsCorrect = false },
                new Option { Id = 50, QuestionId = 18, OptionText = "define, set", IsCorrect = false },

                // Soru 19 - JS tarayıcı
                new Option { Id = 51, QuestionId = 19, OptionText = "Doğru", IsCorrect = false },
                new Option { Id = 52, QuestionId = 19, OptionText = "Yanlış", IsCorrect = true },

                // Soru 20 - JS function
                new Option { Id = 53, QuestionId = 20, OptionText = "function", IsCorrect = true }
            );
        }
    }
}
