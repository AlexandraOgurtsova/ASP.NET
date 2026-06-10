using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pcf.Administration.Core.Services;
using Pcf.Common.Events;
using Pcf.Common.RabbitMQ;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pcf.Administration.WebHost.Consumers
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
                "administration.promocode.queue",
                "promocode.exchange",
                "promocode.issued",
                async (message) =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var employeeService = scope.ServiceProvider.GetRequiredService<IEmployeePromoCodeService>();

                    if (message.PartnerManagerId.HasValue)
                    {
                        _logger.LogInformation("Обработка события: Обновление промокодов для менеджера {ManagerId}",
                            message.PartnerManagerId);
                        await employeeService.UpdateEmployeeAppliedPromocodesAsync(message.PartnerManagerId.Value);
                    }
                });

            return Task.CompletedTask;
        }
    }
}