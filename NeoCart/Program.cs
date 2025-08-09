
using Microsoft.EntityFrameworkCore;
using NeoCart.Infrastructure.Persistence.Contexts;
using NeoCart.Infrastructure.Services;
using NeoCommerce.Application.Contracts.Services;
using NeoCommerce.Infrastructure.Jobs;

namespace NeoCart
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
              options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));
            builder.Services.AddScoped<IEventPublisher>(sp => sp.GetRequiredService<EventPublisher>());
            builder.Services.AddHostedService<RabbitMQConsumer>();
            builder.Services.AddHostedService<RabbitMQDispatcher>();
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
