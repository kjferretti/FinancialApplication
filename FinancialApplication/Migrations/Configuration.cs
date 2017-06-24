using FinancialApplication.Models;
using FinancialApplication.Models.CodeFirst;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace FinancialApplication.Migrations
{
    public class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ApplicationDbContext db)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            if (!db.Roles.Any(r => r.Name == "Administrator"))
            {
                roleManager.Create(new IdentityRole { Name = "Administrator" });
            }

            if (!db.Roles.Any(r => r.Name == "Member"))
            {
                roleManager.Create(new IdentityRole { Name = "Member" });
            }

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            if (!db.Users.Any(u => u.Email == "kjferretti@gmail.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "kjferretti@gmail.com",
                    Email = "kjferretti@gmail.com",
                    FirstName = "Kevin",
                    LastName = "Ferretti",
                }, "password");
            }

            var userId = userManager.FindByEmail("kjferretti@gmail.com").Id;
            userManager.AddToRole(userId, "Administrator");

            if (!db.Users.Any(u => u.Email == "JohnPatton@testuser.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "JohnPatton@testuser.com",
                    Email = "JohnPatton@testuser.com",
                    FirstName = "John",
                    LastName = "Patton",
                }, "password");
            }

            userId = userManager.FindByEmail("JohnPatton@testuser.com").Id;
            userManager.AddToRole(userId, "Member");

            if (!db.Users.Any(u => u.Email == "GeorgeEaston@testuser.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "GeorgeEaston@testuser.com",
                    Email = "GeorgeEaston@testuser.com",
                    FirstName = "George",
                    LastName = "Easton",
                }, "password");
            }

            userId = userManager.FindByEmail("GeorgeEaston@testuser.com").Id;
            userManager.AddToRole(userId, "Member");


            if (!db.Users.Any(u => u.Email == "JosephPeeples@testuser.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "JosephPeeples@testuser.com",
                    Email = "JosephPeeples@testuser.com",
                    FirstName = "Joseph",
                    LastName = "Peeples",
                }, "password");
            }

            userId = userManager.FindByEmail("JosephPeeples@testuser.com").Id;
            userManager.AddToRole(userId, "Member");

            if (!db.Users.Any(u => u.Email == "MichaelMcKenzie@testuser.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "MichaelMcKenzie@testuser.com",
                    Email = "MichaelMcKenzie@testuser.com",
                    FirstName = "Michael",
                    LastName = "McKenzie",
                }, "password");
            }

            userId = userManager.FindByEmail("MichaelMcKenzie@testuser.com").Id;
            userManager.AddToRole(userId, "Member");

            if (!db.Users.Any(u => u.Email == "guest@guest.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "guest@guest.com",
                    Email = "guest@guest.com",
                    FirstName = "Guest",
                    LastName = "User",
                }, "password");
            }
        }
    }
}