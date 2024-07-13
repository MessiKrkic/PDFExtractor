using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PDFExtractor.Core.Interfaces;
using PDFExtractor.UI.Commands;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace PDFExtractor.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IDocumentParser _documentParser;
        private readonly IPDFGenerator _pdfGenerator;
        private readonly IErrorHandlingService _errorHandler;
        private readonly ILogger<MainViewModel> _logger;
        private string _selectedFilePath;
        private string _pageRangeInput;
        private bool _isProcessing;
        private string _statusMessage;
        private int PageCount;
        private int _parsingProgress;
        public int ParsingProgress
        {
            get => _parsingProgress;
            set => SetProperty(ref _parsingProgress, value);
        }

        public MainViewModel(
            IDocumentParser documentParser,
            IPDFGenerator pdfGenerator,
            IErrorHandlingService errorHandler,
            ILogger<MainViewModel> logger)
        {
            _documentParser = documentParser ?? throw new ArgumentNullException(nameof(documentParser));
            _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SelectFileCommand = new RelayCommand(SelectFile);
            GeneratePDFCommand = new RelayCommand(GeneratePDF, CanGeneratePDF);

        }

        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set => SetProperty(ref _selectedFilePath, value);
        }

        public string PageRangeInput
        {
            get => _pageRangeInput;
            set => SetProperty(ref _pageRangeInput, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand SelectFileCommand { get; }
        public ICommand GeneratePDFCommand { get; }

        private async void SelectFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePath = openFileDialog.FileName;
                await ParseDocument();
            }
        }

        private async Task ParseDocument()
        {
            IsProcessing = true;
            ParsingProgress = 0;
            StatusMessage = "Parsing document...";
            try
            {
                var progress = new Progress<int>(value =>
                {
                    ParsingProgress = value;
                });

                var parsedDocument = await _documentParser.ParseAsync(SelectedFilePath, progress);
                StatusMessage = $"Document parsed successfully. Total pages: {parsedDocument.PageCount}";
                PageCount = parsedDocument.PageCount;
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleExceptionAsync(ex, "Error parsing document");
                StatusMessage = "Error parsing document. Please try again.";
            }
            finally
            {
                IsProcessing = false;
                ParsingProgress = 0;
            }
        }

        private async void GeneratePDF()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsProcessing = true;
                StatusMessage = "Generating PDF...";
                try
                {
                    if (ValidatePageRange(PageRangeInput, PageCount, out var pageRanges, out var errorMessage))
                    {
                        var outputPath = await _pdfGenerator.GeneratePDFAsync(SelectedFilePath, pageRanges, saveFileDialog.FileName);
                        StatusMessage = $"PDF generated successfully: {outputPath}";
                    }
                    else
                    {
                        StatusMessage = $"Invalid page range: {errorMessage}";
                    }
                }
                catch (Exception ex)
                {
                    await _errorHandler.HandleExceptionAsync(ex, "Error generating PDF");
                    StatusMessage = "Error generating PDF. Please try again.";
                }
                finally
                {
                    IsProcessing = false;
                }
            }
        }

        private bool ValidatePageRange(string input, int totalPages, out List<(int Start, int End)> validRanges, out string errorMessage)
        {
            validRanges = new List<(int Start, int End)>();
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                return true;
            }

            var rangeRegex = new Regex(@"^\s*(\d+)(?:\s*-\s*(\d+))?\s*$");
            var ranges = input.Split(',');

            foreach (var range in ranges)
            {
                var match = rangeRegex.Match(range);
                if (!match.Success)
                {
                    errorMessage = $"Invalid range format: {range}";
                    return false;
                }

                int start = int.Parse(match.Groups[1].Value);
                int end = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : start;

                if (start > end || start < 1 || end > totalPages)
                {
                    errorMessage = $"Invalid page range: {start}-{end}. Total pages: {totalPages}";
                    return false;
                }

                validRanges.Add((start, end));
            }

            return true;
        }

        private bool CanGeneratePDF()
        {
            return !string.IsNullOrWhiteSpace(SelectedFilePath) && !string.IsNullOrWhiteSpace(PageRangeInput);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}