using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pcf.GivingToCustomer.Core.Abstractions.Gateways;
using Pcf.GivingToCustomer.Core.Configuration;
using Pcf.GivingToCustomer.DataAccess;
using Pcf.GivingToCustomer.Integration;
using Pcf.GivingToCustomer.IntegrationTests.Data;
using System;
using System.IO;
using System.Linq;

namespace Pcf.GivingToCustomer.IntegrationTests
{
    public class TestWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        private IConfiguration _configuration;
        private MongoDbSettings _mongoSettings;

        private void LoadConfiguration()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .Build();

            _mongoSettings = new MongoDbSettings();
            _configuration.GetSection("MongoDbSettings").Bind(_mongoSettings);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            LoadConfiguration();

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(MongoDbContext));

                if (descriptor != null)
                    services.Remove(descriptor);

                var settingsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(MongoDbSettings));

                if (settingsDescriptor != null)
                    services.Remove(settingsDescriptor);

                var testDatabaseName = $"{_mongoSettings.DatabaseName}_{DateTime.Now:yyyyMMddHHmmss}";
                var testMongoSettings = new MongoDbSettings
                {
                    ConnectionString = _mongoSettings.ConnectionString,
                    DatabaseName = testDatabaseName
                };

                services.AddSingleton(testMongoSettings);
                services.AddSingleton<MongoDbContext>();
                services.AddScoped<INotificationGateway, NotificationGateway>();
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<TestWebApplicationFactory<TStartup>>>();

                try
                {
                    var initializer = new MongoDbTestInitializer(dbContext);
                    initializer.InitializeDb();
                    logger.LogInformation("Test database initialized successfully");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error initializing test database");
                    throw;
                }
            }

            return host;
        }
    }
}