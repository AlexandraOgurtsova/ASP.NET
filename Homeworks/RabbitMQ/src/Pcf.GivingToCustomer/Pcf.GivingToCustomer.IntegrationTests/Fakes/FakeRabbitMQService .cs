using System;
using System.Threading.Tasks;
using Pcf.Common.RabbitMQ;

namespace Pcf.GivingToCustomer.IntegrationTests.Fakes
{
    public class FakeRabbitMQService : IRabbitMQService
    {
        public Task PublishAsync<T>(T message, string exchangeName, string routingKey)
        {
            return Task.CompletedTask;
        }

        public void Subscribe<T>(string queueName, string exchangeName, string routingKey, Func<T, Task> onMessage)
        {

        }

        public Task EnsureSetupAsync(string queueName, string exchangeName, string routingKey)
        {
            return Task.CompletedTask;
        }
    }
}