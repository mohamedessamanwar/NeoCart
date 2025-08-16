using NeoCart.Infrastructure.Persistence.Contexts;

namespace NeoCart.Common.Middleware
{
    public class TransactionMiddleware
    {
        private readonly RequestDelegate _next;

        public TransactionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // Only apply transaction for POST and DELETE operations
            if (!ShouldUseTransaction(context.Request.Method))
            {
                await _next(context);
                return;
            }

            if (dbContext.Database.CurrentTransaction != null)
            {
                await _next(context);
                return;
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                await _next(context);
                // Check for ResultSuccess in context items
                if (context.Items.TryGetValue("ResultSuccess", out var resultFlag)
                           && resultFlag is bool isSuccess
                           && !isSuccess)
                {
                    await transaction.RollbackAsync();
                    return;
                }

                // Fallback: if HTTP status indicates error
                if (context.Response.StatusCode >= 400)
                {
                    await transaction.RollbackAsync();
                    return;
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }

        private static bool ShouldUseTransaction(string method)
        {
            return method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("DELETE", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("PUT", StringComparison.OrdinalIgnoreCase);
        }
    }
}
