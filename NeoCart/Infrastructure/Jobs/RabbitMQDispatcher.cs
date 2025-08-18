using Microsoft.EntityFrameworkCore;
using NeoCart.Infrastructure.Persistence.Contexts;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;


namespace NeoCommerce.Infrastructure.Jobs
{
    public class RabbitMQDispatcher : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        public RabbitMQDispatcher(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, stoppingToken);
                    var messages = await db.OutboxMessages
                        .Where(x => !x.IsPublished)
                        .OrderBy(x => x.CreatedAt)
                        .Take(20) // batch processing
                        .ToListAsync(stoppingToken);

                    if (messages.Count > 0)
                    {
                        var factory = new ConnectionFactory { HostName = "localhost", Port = 5672 };
                        var _connection = await factory.CreateConnectionAsync(); // Correct RabbitMQ connection
                        var _channel = await _connection.CreateChannelAsync(); // Correct RabbitMQ channel
                                                                               // await  _channel.QueueDeclareAsync("databas-events", false, false, false, null);

                        foreach (var msg in messages)
                        {
                            // Deserialize the payload explicitly as JsonElement
                            var payload = JsonSerializer.Deserialize<JsonElement>(msg.Payload);

                            var body = Encoding.UTF8.GetBytes(
                                JsonSerializer.Serialize(payload)
                            );

                            // Replace the problematic line with the following:
                            var properties = new BasicProperties(); // Create an instance of BasicProperties if needed
                            await _channel.BasicPublishAsync(
                                "ex1", // exchange
                                "", // routingKey
                                false, // mandatory
                                properties, // basicProperties
                                body // body
                            );
                            // await _channel.BasicPublishAsync("database-events", "database-events", null, body);
                            msg.IsPublished = true;
                        }

                        await db.SaveChangesAsync(stoppingToken);
                    }

                    //  await tx.CommitAsync(stoppingToken);
                    await Task.Delay(2000, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OutboxDispatcher] Error: {ex.Message}");
                }
            }
        }
    }
}
