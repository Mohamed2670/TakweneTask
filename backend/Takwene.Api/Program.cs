using Microsoft.EntityFrameworkCore;
using Takwene.Api.Data;
using Takwene.Api.Data.Seed;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection.")));
var app = builder.Build();
if (args.Contains("--seed-demo-user"))
{
    if (!app.Environment.IsDevelopment()) throw new InvalidOperationException("Demo seeding is development-only.");
    using var scope = app.Services.CreateScope();
    await DevelopmentUserSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>(), app.Configuration);
    return;
}
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
