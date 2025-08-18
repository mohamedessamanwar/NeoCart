using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace NeoCart.Infrastructure.Persistence.Interceptor
{
    public class CustomInterceptor : DbCommandInterceptor
    {
        private readonly ILogger<CustomInterceptor> _logger;

        public CustomInterceptor(ILogger<CustomInterceptor> logger)
        {
            _logger = logger;
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            LogCommand(command, "Executing");
            return base.ReaderExecuting(command, eventData, result);
        }

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            LogCommand(command, "Executing Async");
            return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            LogCommandExecuted(command, eventData);
            return base.ReaderExecuted(command, eventData, result);
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            LogCommandExecuted(command, eventData);
            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override void CommandFailed(
            DbCommand command,
            CommandErrorEventData eventData)
        {
            _logger.LogError(eventData.Exception,
                "Command failed: {CommandText} | Duration: {Duration}ms",
                command.CommandText,
                eventData.Duration.TotalMilliseconds);

            base.CommandFailed(command, eventData);
        }

        public override async Task CommandFailedAsync(
            DbCommand command,
            CommandErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            _logger.LogError(eventData.Exception,
                "Command failed async: {CommandText} | Duration: {Duration}ms",
                command.CommandText,
                eventData.Duration.TotalMilliseconds);

            await base.CommandFailedAsync(command, eventData, cancellationToken);
        }

        private void LogCommand(DbCommand command, string action)
        {
            _logger.LogInformation(
                "{Action} command: {CommandText}",
                action,
                command.CommandText);
        }

        private void LogCommandExecuted(DbCommand command, CommandExecutedEventData eventData)
        {
            _logger.LogInformation(
                "Command executed: {CommandText} | Duration: {Duration}ms",
                command.CommandText,
                eventData.Duration.TotalMilliseconds);
        }
    }
}
