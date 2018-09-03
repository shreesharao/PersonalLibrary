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
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IO;
using SSR.PL.Web.Options;
using SSR.PL.Web.Services.Abstractions;
using SSR.PL.Web.Services.Implementations;

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
#pragma warning disable S125 // Sections of code should not be commented out
                //this option can only be used when mvc middleware is called before authentication
                //googleOptions.CallbackPath = _configuration["Google:CallbackPath"]; 
#pragma warning restore S125 // Sections of code should not be commented out
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, cookieAuthenticationOptions =>
             {

                 cookieAuthenticationOptions.Cookie.Name = "SSR.PL.Web";
                 cookieAuthenticationOptions.Cookie.HttpOnly = true;
                 cookieAuthenticationOptions.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
             });

#pragma warning disable S125 // Sections of code should not be commented out
            //ConfigureApplicationCookie is used to tweak the Identity cookie settings. Not required when using cookies without identity.
            //serviceCollection.ConfigureApplicationCookie(cookieAuthenticationOptions =>
            //{

            //    cookieAuthenticationOptions.Cookie.Name = "SSR.PL.Web";
            //    cookieAuthenticationOptions.Cookie.HttpOnly = true;
            //    cookieAuthenticationOptions.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            //});
#pragma warning restore S125 // Sections of code should not be commented out

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

            serviceCollection.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            

            //configure options
            serviceCollection.Configure<SendGridOptions>(_configuration.GetSection("SendGrid"));

            //add email servicce to IoC container
            serviceCollection.AddSingleton<IApplicationEmailSender, ApplicationEmailSender>();
        }

        public void Configure(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseStaticFiles();

            //if i use Authentication middleware before MVC middleware then i have to use default call back for google. i.e /signin-google.
            //In this case i can not use googleOptions.CallbackPath property
            applicationBuilder.UseAuthentication();

            applicationBuilder.UseMvc(routeBuilder =>
            {
                routeBuilder.MapRoute(name: "default", template: "{controller=Profile}/{action=Login}/{id?}");
            });


        }

    }
}