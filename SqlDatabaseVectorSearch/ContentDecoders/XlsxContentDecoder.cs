using System.Text;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.Extensions.DependencyInjection;
using SqlDatabaseVectorSearch.TextChunkers;

namespace SqlDatabaseVectorSearch.ContentDecoders;

public class XlsxContentDecoder(IServiceProvider serviceProvider) : IContentDecoder
{
    public Task<IEnumerable<Chunk>> DecodeAsync(Stream stream, string contentType, CancellationToken cancellationToken = default)
    {
        var textChunker = serviceProvider.GetRequiredKeyedService<ITextChunker>(contentType);

        var content = new StringBuilder();
        using var workbook = new XLWorkbook(stream);
        foreach (var worksheet in workbook.Worksheets)
        {
            foreach (var row in worksheet.RowsUsed())
            {
                var rowValues = row.CellsUsed().Select(cell => cell.GetValue<string>());
                content.AppendLine(string.Join("\t", rowValues));
            }
        }

        var paragraphs = textChunker.Split(content.ToString().Trim());

        // Pages do not exist in the OpenXML format until they are rendered by a word processor.
        // See https://stackoverflow.com/questions/43700252/how-to-get-page-numbers-based-on-openxmlelement for more details.
        // Therefore, we will not assign a page number.
        return Task.FromResult(paragraphs.Select((text, index) => new Chunk(null, index, text)).ToList().AsEnumerable());
    }
}
