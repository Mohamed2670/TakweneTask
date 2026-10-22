using AutoMapper;
using Takwene.Api.Profiles;
using Takwene.Api.Middleware;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Takwene.Api.Data;
using Takwene.Api.Data.Seed;
using Takwene.Api.Models;
using Takwene.Api.Options;
using Takwene.Api.Repositories;
using Takwene.Api.Repositories.Interfaces;
using Takwene.Api.Services;
using Takwene.Api.Services.Interfaces;
using Takwene.Api.Validation;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues:false)));
builder.Services.AddProblemDetails();
builder.Services.AddApplicationMappings(builder.Configuration);
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection.")));
builder.Services.AddOptions<JwtOptions>().BindConfiguration(JwtOptions.SectionName).ValidateDataAnnotations().ValidateOnStart();
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (origins.Length == 0 || origins.Any(origin => !Uri.TryCreate(origin, UriKind.Absolute, out var uri) || (uri.Scheme != "https" && uri.Scheme != "http") || uri.GetLeftPart(UriPartial.Authority) != origin))
    throw new InvalidOperationException("Configure exact Cors:AllowedOrigins without paths or wildcards.");
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy.WithOrigins(origins).WithMethods("GET","POST","PATCH").WithHeaders("Content-Type","Authorization","X-CSRF").AllowCredentials()));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme).Configure<IOptions<JwtOptions>>((options, settings) =>
{
    var jwt = settings.Value;
    options.MapInboundClaims = false;
    options.IncludeErrorDetails = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer=true, ValidIssuer=jwt.Issuer, ValidateAudience=true, ValidAudience=jwt.Audience,
        ValidateIssuerSigningKey=true, IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
        ValidateLifetime=true, RequireExpirationTime=true, RequireSignedTokens=true, ClockSkew=TimeSpan.Zero,
        ValidAlgorithms=[SecurityAlgorithms.HmacSha256]
    };
});
builder.Services.AddAuthorization();
builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
builder.Services.AddScoped<ITrackRepository, TrackRepository>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<AuthRequestFilter>();
var app = builder.Build();
app.Services.GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
if (args.Contains("--seed-demo-user") || args.Contains("--reset-demo-password"))
{
    if (!app.Environment.IsDevelopment()) throw new InvalidOperationException("Demo seeding is development-only.");
    using var scope = app.Services.CreateScope();
    await DevelopmentUserSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>(), app.Configuration, resetPassword: args.Contains("--reset-demo-password"));
    return;
}
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
