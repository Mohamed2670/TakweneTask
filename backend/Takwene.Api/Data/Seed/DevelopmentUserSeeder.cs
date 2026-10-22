using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Takwene.Api.Models;

namespace Takwene.Api.Data.Seed;

public static class DevelopmentUserSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config, bool resetPassword = false)
    {
        const string email = "demo@example.com";
        var user = await db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == email.ToUpperInvariant());
        if (user is not null && !resetPassword) return;

        var password = config["Seed:DemoPassword"];
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6 || password.Length > 256)
            throw new InvalidOperationException("Set Seed:DemoPassword to a password of 6 to 256 characters before seeding the development demo user.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        if (user is null)
        {
            user = new User { Name = "Demo Operator", Email = email, NormalizedEmail = email.ToUpperInvariant(), CreatedAt = DateTime.UtcNow };
            db.Users.Add(user);
        }
        else
        {
            // Explicit development reset; run with the API stopped.
            var now = DateTime.UtcNow;
            await db.RefreshTokens.Where(x => x.UserId == user.Id && x.RevokedAt == null)
                .ExecuteUpdateAsync(update => update.SetProperty(x => x.RevokedAt, now));
        }
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
