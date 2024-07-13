namespace PDFExtractor.Core.Interfaces
{
    public interface IPDFGenerator
    {
        Task<string> GeneratePDFAsync(string inputPath, List<(int Start, int End)> pageRanges, string outputPath);
    }
}