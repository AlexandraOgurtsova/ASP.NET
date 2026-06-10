using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pcf.Common.RabbitMQ;
using Pcf.GivingToCustomer.Core.Abstractions.Gateways;
using Pcf.GivingToCustomer.DataAccess;
using Pcf.GivingToCustomer.IntegrationTests.Data;
using Pcf.GivingToCustomer.IntegrationTests.Fakes;

namespace Pcf.GivingToCustomer.IntegrationTests
{
    public class TestWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<DataContext>));
                if (dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);

                var rabbitMQDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IRabbitMQService));
                if (rabbitMQDescriptor != null)
                    services.Remove(rabbitMQDescriptor);

                var hostedServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
                         d.ImplementationType == typeof(Pcf.GivingToCustomer.WebHost.Consumers.PromoCodeIssuedConsumer));
                if (hostedServiceDescriptor != null)
                    services.Remove(hostedServiceDescriptor);

                services.AddScoped<INotificationGateway, FakeNotificationGateway>();
                services.AddSingleton<IRabbitMQService, FakeRabbitMQService>(); 

                services.AddDbContext<DataContext>(x =>
                {
                    x.UseSqlite("Filename=TestPromoCodeFactoryDb.sqlite");
                    x.UseSnakeCaseNamingConvention();
                    x.UseLazyLoadingProxies();
                });

                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var dbContext = scopedServices.GetRequiredService<DataContext>();
                var logger = scopedServices
                    .GetRequiredService<ILogger<TestWebApplicationFactory<TStartup>>>();

                try
                {
                    new EfTestDbInitializer(dbContext).InitializeDb();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Проблема во время заполнения тестовой базы. " +
                                        "Ошибка: {Message}", ex.Message);
                }
            });
        }
    }
}