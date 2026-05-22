using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureDocumentPortal.Api.Auth;
using SecureDocumentPortal.Api.Data;
using SecureDocumentPortal.Api.Services;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

var dbProvider = builder.Configuration["Database:Provider"] ?? "Postgres";
builder.Services.AddDbContext<AppDbContext>(opts =>
{
    if (string.Equals(dbProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        opts.UseSqlite(builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=portal.db");
    else
        opts.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
});

var redisConn = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConn))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        ConnectionMultiplexer.Connect(redisConn));
}

var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy(Policies.Admin, p => p.RequireRole(Roles.Admin));
    opts.AddPolicy(Policies.Reviewer, p => p.RequireRole(Roles.Admin, Roles.Reviewer));
    opts.AddPolicy(Policies.Uploader, p => p.RequireRole(Roles.Admin, Roles.Reviewer, Roles.Uploader));
});

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();
builder.Services.AddScoped<IUserService, UserService>();
var blobProvider = builder.Configuration["Blob:Provider"] ?? "Azure";
if (string.Equals(blobProvider, "Local", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddSingleton<IBlobStorage, LocalFileBlobStorage>();
else
    builder.Services.AddSingleton<IBlobStorage, AzureBlobStorage>();

builder.Services.AddCors(opts => opts.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Local")
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/healthz");
app.MapHealthChecks("/readyz");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsSqlite())
        db.Database.EnsureCreated();
    else
        db.Database.Migrate();
    DbSeeder.Seed(db);
}

app.Run();
