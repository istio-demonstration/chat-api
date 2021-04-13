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

            if (users != null)
            {
                // Create Users
                foreach (var user in users)
                {
                    //using var hmac = new HMACSHA512();
                    //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Password"));

                    //user.PasswordSalt = hmac.Key;

                    await userManager.CreateAsync(user, "Password");
                }

                // Create Roles
                var roles = new List<AppRole>()
                {
                    new AppRole() {Name = "Member"},
                    new AppRole() {Name = "Admin"},
                    new AppRole() {Name = "Moderator"},
                };
                //roles.ForEach(async =>
                //{
                // will lead to an exception: A second operation started on this context before a previous operation completed
                //    // do something
                //});
                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(role);
                }

                foreach (var user in users)
                {
                    await userManager.AddToRoleAsync(user, "Member");
                }
             
            }

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
