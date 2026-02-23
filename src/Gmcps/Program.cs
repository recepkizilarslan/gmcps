using Gmcps.Domain.Configuration;
using Gmcps.Domain.Interfaces;
using Gmcps.Infrastructure.Gmp;
using Gmcps.Infrastructure.Security;
using Gmcps.Infrastructure.Stores;
using Gmcps.Tools;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
var envMappings = new Dictionary<string, string>
{
    ["GVM_USERNAME"] = "Gvm:Username",
    ["GVM_PASSWORD"] = "Gvm:Password",
    ["GVM_SOCKET_PATH"] = "Gvm:SocketPath",
};

foreach (var (envVar, configKey) in envMappings)
{
    var value = Environment.GetEnvironmentVariable(envVar);
    if (!string.IsNullOrWhiteSpace(value))
    {
        builder.Configuration[configKey] = value;
    }
}

// Configuration
builder.Services.Configure<GvmOptions>(builder.Configuration.GetSection(GvmOptions.Section));
builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection(ServerOptions.Section));
builder.Services.Configure<StoreOptions>(builder.Configuration.GetSection(StoreOptions.Section));

// Infrastructure - direct Unix socket integration with gvmd
builder.Services.AddSingleton<IGmpClient, GmpUnixSocketClient>();

builder.Services.AddSingleton<ITargetMetadataStore, SqliteTargetMetadataStore>();
builder.Services.AddSingleton<ICompliancePolicyStore, JsonCompliancePolicyStore>();
builder.Services.AddSingleton<IRateLimiter, TokenBucketRateLimiter>();

// Tool handlers
builder.Services.AddTransient<LowLevelTools>();
builder.Services.AddTransient<MetadataTools>();
builder.Services.AddTransient<AnalyticsTools>();

// MCP Server - HTTP/SSE transport
builder.Services.AddMcpServer()
    .WithTools<LowLevelTools>()
    .WithTools<MetadataTools>()
    .WithTools<AnalyticsTools>()
    .WithHttpTransport();

var app = builder.Build();

app.MapMcp();

await app.RunAsync();
