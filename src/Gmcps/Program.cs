using Gmcps;
using Gmcps.Configuration;
using Gmcps.Infrastructure.Security;
using Gmcps.Toolsets;
using ModelContextProtocol.Protocol;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

var envMappings = new Dictionary<string, string>
{
    ["GVM_USERNAME"] = "Gvm:Username",
    ["GVM_PASSWORD"] = "Gvm:Password",
    ["GVM_SOCKET_PATH"] = "Gvm:SocketPath",
    ["GVM_DEFAULT_PORT_LIST_ID"] = "Gvm:DefaultPortListId",
};

foreach (var (envVar, configKey) in envMappings)
{
    var value = Environment.GetEnvironmentVariable(envVar);

    if (!string.IsNullOrWhiteSpace(value))
    {
        builder.Configuration[configKey] = value;
    }
}

builder.Services.Configure<GvmOptions>(builder.Configuration.GetSection(GvmOptions.Section));
builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection(ServerOptions.Section));
builder.Services.Configure<StoreOptions>(builder.Configuration.GetSection(StoreOptions.Section));

builder.Services
    .AddGmcpsInfrastructure()
    .AddGmcpsToolsets()
    .AddGmcpsToolImplementations();

builder.Services.AddMcpServer()
    .WithRequestFilters(filters =>
        filters.AddCallToolFilter(next => async (request, cancellationToken) =>
        {
            var services = request.Services ?? throw new InvalidOperationException("Request service provider is unavailable.");
            var limiter = services.GetRequiredService<IRateLimiter>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("ToolRateLimiter");

            var toolName = request.Params?.Name ?? "unknown";
            var rateLimitKey = request.User?.Identity?.Name ?? "anonymous";

            if (limiter.TryAcquire(rateLimitKey))
            {
                return await next(request, cancellationToken);
            }

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

        }))
    .WithTools<AdministrationToolset>()
    .WithTools<ScansToolset>()
    .WithTools<ConfigurationToolset>()
    .WithTools<AssetsToolset>()
    .WithTools<SecurityInformationToolset>()
    .WithTools<ResilienceToolset>()
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
