using Amp.Core.Extensions.Configuration;
using Amp.Core.Extensions.ServiceCollection;
using Amp.Core.Middleware.Authentication;
using Amp.Core.Middleware.Extensions;
using Amp.Facebook.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
// JWT — AddJwtBearer provides the JWKS signing keys via the Authority endpoint.
// JwtAuthenticationMiddleware (Amp.Core) handles validation + error responses;
// it needs these keys to be registered by the ASP.NET bearer handler.
// Config keys (from Secrets Manager in EKS, appsettings.Development.json locally):
//   Jwt:Authority   e.g. https://cognito-idp.us-east-1.amazonaws.com/us-east-1_abc123
//   Jwt:Audience    e.g. amp-facebook-api
// ---------------------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection(JwtOptions.Section);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.Authority = jwtSection["Authority"];
        opts.Audience = jwtSection["Audience"];
        opts.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    });

builder.Services.AddAmpCoreMiddleware(configureJwt: jwt => jwtSection.Bind(jwt));
builder.Services.AddCoreSwagger("Amp.Facebook.Api", "Facebook Graph API — page management");
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
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
