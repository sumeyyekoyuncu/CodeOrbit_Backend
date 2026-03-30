using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CodeOrbit.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
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