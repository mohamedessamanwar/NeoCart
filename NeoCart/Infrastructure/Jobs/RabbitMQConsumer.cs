using NeoCart.Features.Common.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NeoCommerce.Infrastructure.Jobs
{
    public class RabbitMQConsumer : BackgroundService
    {
        private RabbitMQ.Client.IConnection _connection; // Use RabbitMQ.Client.IConnection
        private IChannel _channel; // Use RabbitMQ.Client.IModel


        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
                _connection = await factory.CreateConnectionAsync(); // Correct RabbitMQ connection
                _channel = await _connection.CreateChannelAsync(); // Correct RabbitMQ channel
                await base.StartAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StartAsync] Exception: {ex.Message}");
                throw; // Re-throw the exception to ensure proper error handling
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel); // Correct RabbitMQ consumer
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                string? messageJson = Encoding.UTF8.GetString(body);
                try
                {
                    var message = GetBaseEvent(messageJson);
                    GetConsumerEvent(message).GetAwaiter().GetResult(); // Ensure the method is awaited properly               
                    await _channel.BasicAckAsync(ea.DeliveryTag, false); // Correct RabbitMQ acknowledgment
                }
                catch (Exception ex)
                {
                    await _channel.BasicRejectAsync(ea.DeliveryTag, false); // Correct RabbitMQ negative acknowledgment
                    Console.WriteLine($"[Consumer] Failed: {ex.Message}");
                }
            };
            // Correct RabbitMQ consume  .
            await _channel.BasicConsumeAsync(queue: "q1", autoAck: false, consumer: consumer); // Correct RabbitMQ consume

        }
        public BaseEvent GetBaseEvent(string payload)
        {
            // Deserialize the payload to JsonElement
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(payload);

            if (jsonElement.TryGetProperty("EventType", out var eventTypeProperty))
            {
                var eventType = eventTypeProperty.GetString();
                var nameSpace = "NeoCart.Features.Common.Events";
                var typeName = $"{nameSpace}.{eventType},NeoCart";
                var type = Type.GetType(typeName);

                if (type == null)
                {
                    throw new Exception($"Event type '{eventType}' not found in namespace '{nameSpace}'");
                }

                var baseEvent = (BaseEvent)JsonSerializer.Deserialize(payload, type)!;
                return baseEvent;
            }
            else
            {
                throw new Exception("EventType property not found in payload");
            }
        }
        public async Task GetConsumerEvent(BaseEvent baseEvent)
        {
            var nameSpace = "NeoCart.Infrastructure.Services";

            var typeName = baseEvent.EventType.Replace("Event", "Consumer");
            var fullTypeName = $"{nameSpace}.{typeName},NeoCommerce.Infrastructure";

            var type = Type.GetType(fullTypeName);
            if (type == null)
            {
                throw new Exception($"Consumer type '{typeName}' not found in namespace '{nameSpace}'");
            }

            // Specify the type explicitly to resolve CS0411
            var consumer = ActivatorUtilities.CreateInstance(null, type);

            var method = type.GetMethod("ConsumeAsync");
            if (method == null)
            {
                throw new Exception($"Method 'ConsumeAsync' not found in consumer type '{typeName}'");
            }

            var task = method.Invoke(consumer, new object[] { baseEvent });

            if (task is Task awaitedTask)
            {
                await awaitedTask;
            }
        }
    }
}
