using AspNetCoreRateLimit;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using NeoCart.Common.Middleware;
using NeoCart.Infrastructure.Persistence.Contexts;
using NeoCart.Infrastructure.Services;
using NeoCommerce.Application.Contracts.Services;
using NeoCommerce.Infrastructure.Jobs;
using Npgsql;
using Serilog;
using System.IO.Compression;

namespace NeoCart
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Database Configuration
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
              options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

            builder.Services.AddDbContext<AppDbPostgresContext>(opts =>
                opts.UseNpgsql(
                    "Host=localhost;Port=5435;Database=neo;Username=postgres;Password=pass;SSL Mode=Disable;Trust Server Certificate=true",
                    npg => npg.UseVector() // <- REQUIRED
                )
            );
            #endregion

            #region Logging Configuration
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.Debug();
            });
            #endregion

            #region Response Compression Configuration
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/json", "text/plain", "text/css", "text/javascript", "application/javascript" });
            });

            builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            builder.Services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.SmallestSize;
            });
            #endregion

            #region Rate Limiting Configuration
            builder.Services.AddOptions();
            builder.Services.AddMemoryCache();

            // Bind from appsettings.json
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
            builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));

            // Redis distributed cache for scaling across multiple instances
            builder.Services.AddStackExchangeRedisCache(opts =>
            {
                opts.Configuration = builder.Configuration["Redis:Configuration"];
                opts.InstanceName = builder.Configuration["Redis:InstanceName"];
            });

            // Distributed stores instead of in-memory
            builder.Services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
            builder.Services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();

            // Required services for AspNetCoreRateLimit
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            #endregion

            #region Forwarded Headers Configuration
            builder.Services.Configure<ForwardedHeadersOptions>(o =>
            {
                o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            #endregion

            #region Application Services Configuration
            builder.Services.AddScoped<IEventPublisher>(sp => sp.GetRequiredService<EventPublisher>());
            builder.Services.AddHostedService<RabbitMQConsumer>();
            builder.Services.AddHostedService<RabbitMQDispatcher>();
            #endregion

            #region API Configuration
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            #endregion

            var app = builder.Build();

            #region HTTP Request Pipeline Configuration
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseMiddleware<RequestLoggingAndValidationMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<TransactionMiddleware>();
            app.UseHttpsRedirection();
            app.UseForwardedHeaders();
            app.UseIpRateLimiting();
            app.UseResponseCompression();
            app.UseAuthorization();

            app.MapControllers();
            #endregion

            app.Run();
        }
    }
}