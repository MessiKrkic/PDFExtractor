using iText.Kernel.Pdf;
using Microsoft.Extensions.Logging;
using PDFExtractor.Core.Interfaces;

namespace PDFExtractor.Core.Services
{
    public class PDFGenerator : IPDFGenerator
    {
        private readonly IErrorHandlingService _errorHandler;
        private readonly ILogger<PDFParser> _logger;

        public PDFGenerator(IErrorHandlingService errorHandler, ILogger<PDFParser> logger)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GeneratePDFAsync(string inputPath, List<(int Start, int End)> pageRanges, string outputPath)
        {
            return await _errorHandler.ExecuteWithErrorHandlingAsync(async () =>
            {
                _logger.LogInformation($"Generating PDF: {outputPath}");

                await Task.Run(() =>
                {
                    using var reader = new PdfReader(inputPath);
                    using var writer = new PdfWriter(outputPath);
                    using var sourcePdf = new PdfDocument(reader);
                    using var outputPdf = new PdfDocument(writer);

                    int totalPages = sourcePdf.GetNumberOfPages();
                    foreach (var range in pageRanges.OrderBy(r => r.Start))
                    {
                        for (int i = range.Start; i <= range.End && i <= totalPages; i++)
                        {
                            if (i > 0)
                            {
                                outputPdf.AddPage(sourcePdf.GetPage(i).CopyTo(outputPdf));
                            }
                            else
                            {
                                _logger.LogWarning($"Skipping invalid page number: {i}");
                            }
                        }
                    }

                    // Add metadata
                    var info = outputPdf.GetDocumentInfo();
                    info.SetTitle($"Extracted from: {Path.GetFileName(inputPath)}");
                    info.SetCreator("PDF Extractor Tool");
                });

                _logger.LogInformation($"PDF generated successfully: {outputPath}");
                return outputPath;
            }, "Generating PDF");
        }
    }
}