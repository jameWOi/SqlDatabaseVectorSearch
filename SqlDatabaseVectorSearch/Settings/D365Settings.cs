namespace SqlDatabaseVectorSearch.Settings;

public class D365Settings
{
    public required string Url { get; init; }
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string TenantId { get; init; }
}