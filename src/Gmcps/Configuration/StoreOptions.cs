namespace Gmcps.Configuration;

public sealed class StoreOptions
{
    public const string Section = "Store";

    public string MetadataPath { get; set; } = "data/metadata.db";
    
    public string PoliciesPath { get; set; } = "data/policies.json";
}
