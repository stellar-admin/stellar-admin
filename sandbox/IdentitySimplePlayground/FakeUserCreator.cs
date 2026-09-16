using Bogus;
using IdentitySimplePlayground.Data;
using Microsoft.AspNetCore.Identity;

namespace IdentitySimplePlayground;

public static class FakeUserCreator
{
    public static async Task CreateFakeUsers(WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var seed = new Faker<FakeUser>()
                .RuleFor(u => u.Email, f => f.Person.Email)
                .RuleFor(u => u.Password, f => f.Random.AlphaNumeric(10))
                .RuleFor(u => u.Role, f => f.PickRandom(roles))
                .RuleFor(u => u.EmailConfirmed, f => f.Random.Bool())
                .Generate(50);

            foreach (var fakeUser in seed)
            {
                var user = new ApplicationUser
                {
                    UserName = fakeUser.Email,
                    Email = fakeUser.Email,
                    EmailConfirmed = fakeUser.EmailConfirmed,
                };

                var result = await userManager.CreateAsync(user, fakeUser.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, fakeUser.Role);
                }
                else
                {
                    throw new Exception(
                        string.Join("; ", result.Errors.Select(e => e.Description))
                    );
                }
            }
        }
    }

    private class FakeUser
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
        public required string Role { get; init; }
        public required bool EmailConfirmed { get; init; }
    }
}
