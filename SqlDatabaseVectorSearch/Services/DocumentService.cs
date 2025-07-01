using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using SqlDatabaseVectorSearch.DataAccessLayer;
using SqlDatabaseVectorSearch.Models;
using SqlDatabaseVectorSearch.TextChunkers;

namespace SqlDatabaseVectorSearch.Services;

public class DocumentService(ApplicationDbContext dbContext, IServiceProvider serviceProvider, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    public async Task<IEnumerable<Document>> GetAsync(CancellationToken cancellationToken = default)
    {
        var documents = await dbContext.Documents.OrderBy(d => d.Name)
            .Select(d => new Document(d.Id, d.Name, d.CreationDate, d.Chunks.Count))
            .ToListAsync(cancellationToken);

        return documents;
    }

    public async Task<IEnumerable<DocumentChunk>> GetChunksAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var documentChunks = await dbContext.DocumentChunks.Where(c => c.DocumentId == documentId).OrderBy(c => c.Index)
            .Select(c => new DocumentChunk(c.Id, c.Index, c.Content, null))
            .ToListAsync(cancellationToken);

        return documentChunks;
    }

    public async Task<DocumentChunk?> GetChunkEmbeddingAsync(Guid documentId, Guid documentChunkId, CancellationToken cancellationToken = default)
    {
        var documentChunk = await dbContext.DocumentChunks.Where(c => c.Id == documentChunkId && c.DocumentId == documentId)
            .Select(c => new DocumentChunk(c.Id, c.Index, c.Content, c.Embedding))
            .FirstOrDefaultAsync(cancellationToken);

        return documentChunk;
    }

    public Task DeleteAsync(Guid documentId, CancellationToken cancellationToken = default)
            => dbContext.Documents.Where(d => d.Id == documentId).ExecuteDeleteAsync(cancellationToken);

    public Task DeleteAsync(IEnumerable<Guid> documentIds, CancellationToken cancellationToken = default)
        => dbContext.Documents.Where(d => documentIds.Contains(d.Id)).ExecuteDeleteAsync(cancellationToken);

    public async Task CreateFromHtmlAsync(string title, string plainText, string articleNumber , CancellationToken cancellationToken = default)
    {
        var document = new DataAccessLayer.Entities.Document
        {
            Id = Guid.NewGuid(),
            Name = title,
            CreationDate = DateTimeOffset.UtcNow,
            Chunks = []
        };

        // Use the HTML chunker for splitting
        var chunker = serviceProvider.GetRequiredKeyedService<ITextChunker>("text/html");
        var paragraphs = chunker.Split(plainText);

        // Generate embeddings for all paragraphs
        var embeddings = await embeddingGenerator.GenerateAndZipAsync(paragraphs, cancellationToken: cancellationToken);

        int index = 0;
        foreach (var (content, embedding) in embeddings)
        {
            document.Chunks.Add(new DataAccessLayer.Entities.DocumentChunk
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Index = index++,
                Content = content.Trim(),
                Embedding = embedding.Vector.ToArray(),
                ArticleNumber = articleNumber,
                ArticleUrl = "https://ask.otago.ac.nz/knowledgebase/article/"+ articleNumber

            });
        }

        dbContext.Documents.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}