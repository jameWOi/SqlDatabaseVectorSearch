using System;

namespace SqlDatabaseVectorSearch.DataAccessLayer.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string[] Values { get; set; } = [];
    public float[] Embedding { get; set; } = [];
    public Guid DocumentId { get; set; }
}
