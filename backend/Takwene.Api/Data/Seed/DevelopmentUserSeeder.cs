using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Takwene.Api.Models;
namespace Takwene.Api.Data.Seed;
public static class DevelopmentUserSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config)
    {
        const string email = "demo@example.com";
        if (await db.Users.AnyAsync(x => x.NormalizedEmail == email.ToUpperInvariant())) return;
        var password = config["Seed:DemoPassword"];
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12 || password.Length > 256)
            throw new InvalidOperationException("Set Seed:DemoPassword to a password of 12 to 256 characters before seeding the demo user.");
        var user = new User { Name="Demo Operator", Email=email, NormalizedEmail=email.ToUpperInvariant(), CreatedAt=DateTime.UtcNow };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}

