using System.Text;
using ClosedXML.Excel;

namespace SqlDatabaseVectorSearch.ContentDecoders;

public class XlsxContentDecoder : IContentDecoder
{
    public Task<string> DecodeAsync(Stream stream, string contentType, CancellationToken cancellationToken = default)
    {
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

        return Task.FromResult(content.ToString());
    }
}
