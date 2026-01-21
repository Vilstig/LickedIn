namespace LickedIn.Data
{
    using Microsoft.AspNetCore.Identity;
    

    public static class MyIdentityDataInitializer
    {
        public static void SeedData(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            SeedRoles(roleManager);
            SeedUsers(userManager);
        }

        public static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync("HR").Result)
            {
                IdentityRole role = new()
                {
                    Name = "HR"
                };
                roleManager.CreateAsync(role).Wait();
            }
        }

        public static void SeedOneUser(UserManager<IdentityUser> userManager, string userName, string password, string? role = null)
        {
            if (userManager.FindByNameAsync(userName).Result == null)
            {
                IdentityUser user = new()
                {
                    UserName = userName,
                    Email = userName
                };

                IdentityResult result = userManager.CreateAsync(user, password).Result;

                if (result.Succeeded && role != null)
                {
                    userManager.AddToRoleAsync(user, role).Wait();
                }
            }
        }

        public static void SeedUsers(UserManager<IdentityUser> userManager)
        {
            SeedOneUser(userManager, "admin@localhost", "Admin123!", "HR");
        }
    }
}