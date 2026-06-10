using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pcf.Common.RabbitMQ
{
    public class RabbitMQService : IRabbitMQService, IAsyncDisposable, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;  
        private readonly ILogger<RabbitMQService> _logger;
        private readonly RabbitMQSettings _settings;
        private bool _disposed;

        public RabbitMQService(RabbitMQSettings settings, ILogger<RabbitMQService> logger)
        {
            _settings = settings;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = settings.Host,
                UserName = settings.User,
                Password = settings.Password,
                Port = 5672,
                VirtualHost = "/"
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        }

        public async Task PublishAsync<T>(T message, string exchangeName, string routingKey)
        {
            try
            {
                await _channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: true);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                var properties = new BasicProperties
                {
                    Persistent = true
                };

                await _channel.BasicPublishAsync(exchangeName, routingKey, true, properties, body);
                _logger.LogInformation("Сообщение опубликовано: {Exchange}/{RoutingKey}", exchangeName, routingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при публикации сообщения");
                throw;
            }
        }

        public async void Subscribe<T>(string queueName, string exchangeName, string routingKey, Func<T, Task> onMessage)
        {
            try
            {
                await _channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: true);
                await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                await _channel.QueueBindAsync(queueName, exchangeName, routingKey, arguments: null);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (sender, args) =>
                {
                    try
                    {
                        var body = args.Body.ToArray();
                        var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));

                        if (message != null)
                        {
                            await onMessage(message);
                            await _channel.BasicAckAsync(args.DeliveryTag, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при обработке сообщения");
                        await _channel.BasicNackAsync(args.DeliveryTag, false, true);
                    }
                };

                await _channel.BasicConsumeAsync(queueName, false, consumer);
                _logger.LogInformation("Подписка на очередь {QueueName} активирована", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подписке на очередь {QueueName}", queueName);
                throw;
            }
        }

        public async Task EnsureSetupAsync(string queueName, string exchangeName, string routingKey)
        {
            try
            {
                await _channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: true);
                await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                await _channel.QueueBindAsync(queueName, exchangeName, routingKey, arguments: null);
                _logger.LogInformation("Очередь {QueueName} и обменник {ExchangeName} настроены", queueName, exchangeName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при настройке очереди {QueueName}", queueName);
                throw;
            }
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                try
                {
                    if (_channel != null)
                    {
                        await _channel.CloseAsync();
                        await _channel.DisposeAsync();
                    }

                    if (_connection != null)
                    {
                        await _connection.CloseAsync();
                        await _connection.DisposeAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Ошибка при закрытии соединения RabbitMQ");
                }

                _disposed = true;
            }
        }
    }
}