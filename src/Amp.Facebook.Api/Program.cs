using Amp.Core.Extensions.Configuration;
using Amp.Core.Extensions.ServiceCollection;
using Amp.Core.Extensions.Versioning;
using Amp.Core.Middleware.Extensions;
using Amp.Facebook.Api.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// AWS Secrets Manager — Amp.Core.Extensions
// Credentials via IRSA (EKS pod annotation); no static keys anywhere.
// AWS_REGION and AWS_SECRET_NAME are injected by Helm as environment variables.
// ---------------------------------------------------------------------------
var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION")
    ?? builder.Configuration["Aws:Region"]
    ?? "us-east-1";

var secretName = Environment.GetEnvironmentVariable("AWS_SECRET_NAME")
    ?? builder.Configuration["Aws:SecretsManager:SecretName"]
    ?? "amp-facebook-api/config";

if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddAwsSecretsManager(secretName, awsRegion);
}
else
{
    // Locally, skip AWS Secrets Manager — use appsettings.Development.json / user-secrets instead.
    Console.WriteLine("[Config] Development environment: skipping AWS Secrets Manager.");
}

// ---------------------------------------------------------------------------
// Serilog — sinks configured via appsettings / Secrets Manager (CloudWatch)
// ---------------------------------------------------------------------------
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .Enrich.WithProperty("Application", "Amp.Facebook.Api")
      .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName));

// ---------------------------------------------------------------------------
// Amp.Core infrastructure — Amp.Core.Extensions + Amp.Core.Middleware
// Registers: HttpContextAccessor, MemoryCache, HealthChecks, API versioning,
//            CorrelationId accessor, JWT options, ExceptionHandling pipeline.
// ---------------------------------------------------------------------------
builder.Services.AddCoreServices(builder.Configuration, "Amp.Facebook.Api");

// ---------------------------------------------------------------------------
// Amp.Core middleware — reads Jwt:Authority / Jwt:Audience / Jwt:ClientId from
// config automatically (Secrets Manager in EKS, appsettings.Development.json locally)
// ---------------------------------------------------------------------------
builder.Services.AddAmpCoreMiddleware(builder.Configuration);

// Required by AddCoreSwagger — ApiVersionMetadataFilter resolves this from DI.
// No deprecated versions in this API; register an empty policy.
builder.Services.AddSingleton(new ApiVersionSunsetPolicy());

builder.Services.AddCoreSwagger(o =>
{
    o.Title       = "Amp.Facebook.Api";
    o.Description = "Facebook Graph API — page management";
});
builder.Services.AddAwsMetricsExporter("Amp.Facebook.Api");

// ---------------------------------------------------------------------------
// Facebook Graph API — named HttpClient
// Base URL may be overridden via "Facebook:GraphApiBaseUrl" in Secrets Manager.
// Tokens are supplied per-request by the service layer — never stored here.
// ---------------------------------------------------------------------------
var graphBaseUrl = builder.Configuration["Facebook:GraphApiBaseUrl"]
    ?? "https://graph.facebook.com/v25.0/";

builder.Services.AddHttpClient("FacebookGraph", client =>
{
    client.BaseAddress = new Uri(graphBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<IFacebookService, FacebookService>();

// ---------------------------------------------------------------------------
// MVC
// ---------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseAmpCoreSwagger();

// Amp.Core pipeline: ExceptionHandling → HealthCheck → CorrelationId → RequestLogging
app.UseAmpCoreMiddleware(opts => opts.EnableJwtAuthentication = true);

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
