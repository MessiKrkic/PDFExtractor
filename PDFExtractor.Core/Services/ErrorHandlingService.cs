using Microsoft.Extensions.Logging;
using PDFExtractor.Core.Interfaces;

namespace PDFExtractor.Core.Services
{
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;

        public ErrorHandlingService(ILogger<ErrorHandlingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void HandleException(Exception ex, string contextMessage = null)
        {
            _logger.LogError(ex, contextMessage ?? "An unexpected error occurred");
        }

        public Task HandleExceptionAsync(Exception ex, string contextMessage = null)
        {
            HandleException(ex, contextMessage);
            return Task.CompletedTask;
        }

        public async Task<TResult> ExecuteWithErrorHandlingAsync<TResult>(Func<Task<TResult>> operation, string operationName)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                HandleException(ex, $"Error occurred during {operationName}");
                throw;
            }
        }

        public async Task ExecuteWithErrorHandlingAsync(Func<Task> operation, string operationName)
        {
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                HandleException(ex, $"Error occurred during {operationName}");
                throw;
            }
        }
    }
}