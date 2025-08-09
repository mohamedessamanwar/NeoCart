using NeoCart.Infrastructure.Persistence.Contexts;
using NeoCart.Infrastructure.Persistence.Entities;
using NeoCommerce.Application.Contracts.Services;
using System.Text.Json;


namespace NeoCart.Infrastructure.Services
{
    public class EventPublisher : IEventPublisher
    {
        private readonly ApplicationDbContext _db;
        public EventPublisher(ApplicationDbContext db) => _db = db;

        public async Task EnqueueAsync(string eventType, object payload)
        {
            var msg = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                Payload = JsonSerializer.Serialize(payload),
                IsPublished = false
            };
            await _db.OutboxMessages.AddAsync(msg);
            await _db.SaveChangesAsync();
        }
    }
}
