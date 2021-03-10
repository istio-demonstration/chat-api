using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public  class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if(await userManager.Users.AnyAsync()) return;

            var userData = await System.IO.File.ReadAllTextAsync("../API/SeedingData/UserSeedData.json");
            var users =  JsonSerializer.Deserialize<List<AppUser>>(userData);

            foreach (var user in users)
            {
                //using var hmac = new HMACSHA512();
                //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Password"));

                //user.PasswordSalt = hmac.Key;

                await userManager.CreateAsync(user, "Password");
            }

            var roles = new List<AppRole>()
            {
                new AppRole(){Name = "Member"},
                new AppRole(){Name = "Admin"},
                new AppRole(){Name = "Moderator"},
            };

            roles.ForEach(async r =>
            {
                await roleManager.CreateAsync(r);
            });

            users.ForEach(async user =>
            {
                await userManager.AddToRoleAsync(user, "Member");

            });
            // admin
            var admin = new AppUser()
            {
                UserName = "Admin"
            };
            await userManager.CreateAsync(admin, "Password");
            await userManager.AddToRolesAsync(admin, new List<string>() {"Admin", "Moderator" });
            //await context.SaveChangesAsync();

        }
    }
}
