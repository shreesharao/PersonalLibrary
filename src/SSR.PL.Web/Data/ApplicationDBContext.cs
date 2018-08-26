using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SSR.PL.Web.Entities;
using System;

namespace SSR.PL.Web.Data
{
    public class ApplicationDBContext: IdentityDbContext<ApplicationUser<Guid>,IdentityRole<Guid>,Guid>
    {

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> dbContextOptions):base(dbContextOptions)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //call base class OnModelCreating to override the configuration here
            base.OnModelCreating(modelBuilder);

            //change Table name
            modelBuilder.Entity<ApplicationUser<Guid>>(buildAction =>
            {
                buildAction.ToTable("Users");
            });

            //change Column name
            modelBuilder.Entity<ApplicationUser<Guid>>(buildAction =>
            {
                buildAction.Property(user=>user.AlternatePhoneNumber).HasColumnName("AlternatePhoneNumber").HasMaxLength(10);
            });
        }
    }
}
