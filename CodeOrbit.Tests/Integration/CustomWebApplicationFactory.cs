using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CodeOrbit.Infrastructure.Data;

namespace CodeOrbit.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Mevcut DbContext'i kaldır
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // In-Memory database ekle
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));

                // Mevcut authentication'ı kaldır
                var descriptors = services.Where(d =>
                    d.ServiceType == typeof(IAuthenticationSchemeProvider) ||
                    d.ServiceType == typeof(IAuthenticationHandlerProvider))
                    .ToList();
                foreach (var d in descriptors)
                    services.Remove(d);

                // Test authentication scheme ekle
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test", options => { });
            });
        }
    }
}