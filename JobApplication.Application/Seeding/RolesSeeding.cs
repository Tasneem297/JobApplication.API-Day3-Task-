using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobApplication.Application.Seeding;
public class RolesSeeding
{
    public static async Task SeedRolesAsync(
       RoleManager<IdentityRole> roleManager)
    {
        string[] roles =
        {
            "Applicant",
            "Recruiter"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(role));
            }
        }
    }
}
