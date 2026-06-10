using System;
using System.Threading.Tasks;

namespace Pcf.Common.RabbitMQ
{
    public interface IRabbitMQService
    {
        /// <summary>
        /// Публикация сообщения в очередь
        /// </summary>
        Task PublishAsync<T>(T message, string exchangeName, string routingKey);

        /// <summary>
        /// Подписка на очередь
        /// </summary>
        void Subscribe<T>(string queueName, string exchangeName, string routingKey, Func<T, Task> onMessage);

        /// <summary>
        /// Создание очереди и обменника
        /// </summary>
        Task EnsureSetupAsync(string queueName, string exchangeName, string routingKey);
    }
}