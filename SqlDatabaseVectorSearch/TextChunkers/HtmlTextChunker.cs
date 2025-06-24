using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using SqlDatabaseVectorSearch.Services;
using SqlDatabaseVectorSearch.Settings;

namespace SqlDatabaseVectorSearch.TextChunkers;

public class HtmlTextChunker(TokenizerService tokenizerService, IOptions<AppSettings> appSettingsOptions) : ITextChunker
{
    private readonly AppSettings appSettings = appSettingsOptions.Value;

    public IList<string> Split(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var paragraphs = new List<string>();

        // Extract text from <p>, <h1>-<h6>, <li>, and fallback to body text
        foreach (var node in doc.DocumentNode.SelectNodes("//p|//h1|//h2|//h3|//h4|//h5|//h6|//li") ?? Enumerable.Empty<HtmlNode>())
        {
            var text = node.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(text))
                paragraphs.Add(text);
        }
        // Fallback: if no paragraphs found, use all text
        if (paragraphs.Count == 0)
        {
            var allText = doc.DocumentNode.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(allText))
                paragraphs.Add(allText);
        }

        // Optionally further split long paragraphs using your tokenizer
        var chunked = new List<string>();
        foreach (var para in paragraphs)
        {
            if (para.Length > 0)
            {
                // Use DefaultTextChunker logic for long paragraphs
                var lines = para.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                chunked.AddRange(lines);
            }
        }

        return chunked;
    }
}
