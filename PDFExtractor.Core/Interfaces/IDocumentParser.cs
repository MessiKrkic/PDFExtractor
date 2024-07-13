using PDFExtractor.Core.Models;

namespace PDFExtractor.Core.Interfaces
{
    public interface IDocumentParser
    {
        Task<ParsedDocument> ParseAsync(string filePath, IProgress<int> progress);
    }
}