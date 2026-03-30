using CodeOrbit.Domain.Entities;
using CodeOrbit.Domain.Enums;
using CodeOrbit.Infrastructure.Data;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CodeOrbit.Tests.Integration
{
    public class IntegrationTestHelpers
    {
        public static async Task<User> SeedUserAsync(
     IServiceScope scope,
     string username = "testuser",
     string email = "test@test.com")
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!", workFactor: 4)
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }
        public static async Task<(Category category,List<Question> questions)> SeedCategoryWithQuestionsAsync(
            IServiceScope scope,
            string categoryName="C#",
            int QuestionCount= 5,
            DifficultyLevel difficulty=DifficultyLevel.Easy
            )
        {
            var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var category=new Category
            {
                Name=categoryName,
                Language=ProgrammingLanguage.CSharp
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            var questions=new List<Question>();
            for(int i=0;i<QuestionCount;i++)
            {
                var question=new Question
                {
                    CategoryId=category.Id,
                    QuestionText=$"Sample question {i+1}?",
                    QuestionType=QuestionType.MultipleChoice,
                    DifficultyLevel=difficulty,
                    Options = new List<Option>
                    {
                        new Option { OptionText = "A şıkkı", IsCorrect = true },
                        new Option { OptionText = "B şıkkı", IsCorrect = false },
                        new Option { OptionText = "C şıkkı", IsCorrect = false },
                        new Option { OptionText = "D şıkkı", IsCorrect = false }
                    }
                };
                questions.Add(question);
                db.Questions.Add(question);
            }
            await db.SaveChangesAsync();
            return (category,questions);
        }
        public static async Task<string> GetTokenAsync(
      HttpClient client,
      string email = "test@test.com",
      string password = "Password123!")
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", new
            {
                Email = email,
                Password = password
            });

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return result!.Token;
        }
        public static void SetAuthHeader(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        private record TokenResponse(string Token);
    }
}


