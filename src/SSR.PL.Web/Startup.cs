using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using SSR.PL.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SSR.PL.Web.Entities;
using System;
using Microsoft.AspNetCore.Identity;

namespace SSR.PL.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
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
            serviceCollection.ConfigureApplicationCookie(cookieAuthenticationOptions=>{
                cookieAuthenticationOptions.Cookie.Name = "SSR.PL.Web";
            });

            serviceCollection.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder applicationBuilder)
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