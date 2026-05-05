using System;
using System.Collections.Generic;
using backend.Data;
using backend.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using BackendProgram = global::Program;

namespace Backend.Tests
{
    public sealed class IntegrationWebApplicationFactory : WebApplicationFactory<BackendProgram>
    {
        public IntegrationWebApplicationFactory()
        {
            FaceServiceMock = new Mock<IAzureFaceService>();
            DatabaseName = Guid.NewGuid().ToString("N");
        }

        public Mock<IAzureFaceService> FaceServiceMock { get; }

        private string DatabaseName { get; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JWT:SigningKey"] = "integration-test-signing-key-1234567890",
                    ["JWT:Issuer"] = "integration-tests",
                    ["JWT:Audience"] = "integration-tests",
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=integration-tests;Username=test;Password=test"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ApplicationDBContext>();
                services.RemoveAll<DbContextOptions<ApplicationDBContext>>();
                services.RemoveAll<IAzureFaceService>();

                services.AddDbContext<ApplicationDBContext>(options =>
                    options.UseInMemoryDatabase(DatabaseName));

                services.AddSingleton<IAzureFaceService>(FaceServiceMock.Object);
            });
        }
    }
}