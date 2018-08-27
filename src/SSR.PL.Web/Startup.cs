using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using SSR.PL.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SSR.PL.Web.Entities;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.Google;
using System.IO;

namespace SSR.PL.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IHostingEnvironment hostingEnvironment)
        {
            var configurationBuilder = new ConfigurationBuilder();

            //because we are not using injected IConfiguration we need to do everything ourselves
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            if (hostingEnvironment.IsDevelopment())
            {
                configurationBuilder.AddUserSecrets("41c735c0-b7be-4823-a55e-1ea0ebc8d46d");
            }

            configurationBuilder.AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true);

            _configuration = configurationBuilder.Build();
        }

        public void ConfigureServices(IServiceCollection serviceCollection)
        {

            //Add DBContext
            serviceCollection.AddDbContext<ApplicationDBContext>(dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder.UseSqlServer(_configuration.GetConnectionString("SQLServerConnection"));
            });

            //Add Identity
            serviceCollection.AddDefaultIdentity<ApplicationUser<Guid>>().AddEntityFrameworkStores<ApplicationDBContext>();

            //add Google Authentication
            serviceCollection.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = _configuration["Google:ClientId"];
                googleOptions.ClientSecret = _configuration["Google:ClientSecret"];
                googleOptions.CallbackPath = "Profile/ExternalLogin";
            });

            //configure identity password policy, lockout, and cookie configuration.
            serviceCollection.Configure<IdentityOptions>(identityOptions =>
            {
                //lockout
                identityOptions.Lockout.MaxFailedAccessAttempts = 3;
                identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);

                //password
                identityOptions.Password.RequireDigit = false;

                //SignInOptions
                identityOptions.SignIn.RequireConfirmedEmail = false;

                //UserOptions
                identityOptions.User.RequireUniqueEmail = true;
            });

            //configure Cookies
            serviceCollection.ConfigureApplicationCookie(cookieAuthenticationOptions =>
            {
                cookieAuthenticationOptions.Cookie.Name = "SSR.PL.Web";
            });

            serviceCollection.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder applicationBuilder, IHostingEnvironment hostingEnvrionment)
        {
            applicationBuilder.UseStaticFiles();
            applicationBuilder.UseMvc(routeBuilder =>
            {
                routeBuilder.MapRoute(name: "default", template: "{controller=Profile}/{action=Login}/{id?}");
            });

            applicationBuilder.UseAuthentication();

        }

    }
}