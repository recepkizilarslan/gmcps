using Gmcps.Configuration;
using Gmcps.Core;
using Gmcps.Domain.Interfaces;
using Gmcps.Domain.Scans.Reports.Inputs;
using Gmcps.Domain.Scans.Reports.Outputs;
using Gmcps.Domain.Scans.Tasks.Inputs;
using Gmcps.Domain.Scans.Tasks.Outputs;
using Gmcps.Infrastructure.Gmp;
using Gmcps.Infrastructure.Scans.Reports.GetReportSummary;
using Gmcps.Infrastructure.Scans.Tasks.GetTaskStatus;
using Gmcps.Infrastructure.Security.Core;
using Gmcps.Infrastructure.Security.Implementation;
using Gmcps.Infrastructure.Stores;
using Gmcps.Models;
using Gmcps.Toolsets;
using Gmcps.Tools;
using ModelContextProtocol.Protocol;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

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
builder.Services.AddTransient<ScansToolset>();

// Tool-specific implementations (pilot vertical slice)
builder.Services.AddTransient<IClient<GetTaskStatusClientRequest, GvmTask>, GetTaskStatusClient>();
builder.Services.AddTransient<IExample<GvmTask, GetTaskStatusOutput>, GetTaskStatusParser>();
builder.Services.AddTransient<ITool<GetTaskStatusInput, GetTaskStatusOutput>, GetTaskStatusTool>();

builder.Services.AddTransient<IClient<GetReportSummaryClientRequest, Report>, GetReportSummaryClient>();
builder.Services.AddTransient<IExample<Report, GetReportSummaryOutput>, GetReportSummaryParser>();
builder.Services.AddTransient<ITool<GetReportSummaryInput, GetReportSummaryOutput>, GetReportSummaryTool>();

// MCP Server - HTTP/SSE transport
builder.Services.AddMcpServer()
    .WithRequestFilters(filters =>
        filters.AddCallToolFilter(next => async (request, cancellationToken) =>
        {
            var services = request.Services ?? throw new InvalidOperationException("Request service provider is unavailable.");
            var limiter = services.GetRequiredService<IRateLimiter>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("ToolRateLimiter");

            var toolName = request.Params?.Name ?? "unknown";
            var rateLimitKey = request.User?.Identity?.Name ?? "anonymous";

            if (!limiter.TryAcquire(rateLimitKey))
            {
                logger.LogWarning(
                    "Rate limit exceeded for tool {ToolName} and key {RateLimitKey}",
                    toolName,
                    rateLimitKey);

                return new CallToolResult
                {
                    IsError = true,
                    Content = new List<ContentBlock>
                    {
                        new TextContentBlock
                        {
                            Text = "Rate limit exceeded. Please wait and try again."
                        }
                    }
                };
            }

            return await next(request, cancellationToken);
        }))
    .WithTools<LowLevelTools>()
    .WithTools<MetadataTools>()
    .WithTools<AnalyticsTools>()
    .WithTools<ScansToolset>()
    .WithHttpTransport();

var app = builder.Build();
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

startupLogger.LogInformation(
    "Initializing GMCPS server (socket: {SocketPath}, metadata: {MetadataPath}, policies: {PoliciesPath}, rateLimitPerMinute: {RateLimitPerMinute})",
    app.Configuration["Gvm:SocketPath"] ?? "unset",
    app.Configuration["Store:MetadataPath"] ?? "unset",
    app.Configuration["Store:PoliciesPath"] ?? "unset",
    app.Configuration["Server:RateLimitPerMinute"] ?? "unset");

app.Lifetime.ApplicationStarted.Register(() => startupLogger.LogInformation("GMCPS server started"));
app.Lifetime.ApplicationStopping.Register(() => startupLogger.LogInformation("GMCPS server stopping"));
app.Lifetime.ApplicationStopped.Register(() => startupLogger.LogInformation("GMCPS server stopped"));

app.MapMcp();

await app.RunAsync();
