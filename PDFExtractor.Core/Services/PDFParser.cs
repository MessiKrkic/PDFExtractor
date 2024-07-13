using iText.Kernel.Pdf;
using PDFExtractor.Core.Interfaces;
using PDFExtractor.Core.Models;
using Microsoft.Extensions.Logging;

namespace PDFExtractor.Core.Services
{
    public class PDFParser : IDocumentParser
    {
        private readonly IErrorHandlingService _errorHandler;
        private readonly ILogger<PDFParser> _logger;

        public PDFParser(IErrorHandlingService errorHandler, ILogger<PDFParser> logger)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ParsedDocument> ParseAsync(string filePath, IProgress<int> progress)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                _logger.LogInformation($"Parsing PDF: {filePath}");

                return await Task.Run(() =>
                {
                    using var pdfReader = new PdfReader(filePath);
                    using var pdfDocument = new PdfDocument(pdfReader);

                    var pageCount = pdfDocument.GetNumberOfPages();
                    var title = ExtractTitle(pdfDocument) ?? System.IO.Path.GetFileNameWithoutExtension(filePath);

                    for (int i = 1; i <= pageCount; i++)
                    {
                        progress.Report(i * 100 / pageCount);
                    }

                    return new ParsedDocument
                    {
                        Title = title,
                        PageCount = pageCount,
                        FilePath = filePath
                    };
                });
            }, $"Parsing PDF: {filePath}");
        }

        private string? ExtractTitle(PdfDocument pdfDocument)
        {
            var info = pdfDocument.GetDocumentInfo();
            return !string.IsNullOrWhiteSpace(info.GetTitle()) ? info.GetTitle() : null;
        }
    }
}