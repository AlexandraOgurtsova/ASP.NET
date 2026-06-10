using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pcf.Common.Events;
using Pcf.Common.RabbitMQ;
using Pcf.GivingToCustomer.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.WebHost.Consumers
{
    public class PromoCodeIssuedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly ILogger<PromoCodeIssuedConsumer> _logger;

        public PromoCodeIssuedConsumer(
            IServiceProvider serviceProvider,
            IRabbitMQService rabbitMQService,
            ILogger<PromoCodeIssuedConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _rabbitMQService.Subscribe<PromoCodeIssuedEvent>(
                "givingtocustomer.promocode.queue",
                "promocode.exchange",
                "promocode.issued",
                async (message) =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var distributionService = scope.ServiceProvider.GetRequiredService<IPromoCodeDistributionService>();

                    _logger.LogInformation("Обработка события: Выдача промокода {PromoCode} клиентам", message.PromoCode);

                    await distributionService.DistributePromoCodeToCustomersAsync(
                        message.PromoCodeId,
                        message.PromoCode,
                        message.ServiceInfo,
                        message.PreferenceId,
                        message.PartnerId,
                        message.BeginDate,
                        message.EndDate);
                });

            return Task.CompletedTask;
        }
    }
}