namespace PDFExtractor.Core.Interfaces
{
    public interface IErrorHandlingService
    {
        void HandleException(Exception ex, string contextMessage = null);
        Task HandleExceptionAsync(Exception ex, string contextMessage = null);
        Task<TResult> ExecuteWithErrorHandlingAsync<TResult>(Func<Task<TResult>> operation, string operationName);
        Task ExecuteWithErrorHandlingAsync(Func<Task> operation, string operationName);
    }
}