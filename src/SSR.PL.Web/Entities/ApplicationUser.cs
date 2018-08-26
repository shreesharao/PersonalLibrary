using Microsoft.AspNetCore.Identity;
using System;

namespace SSR.PL.Web.Entities
{
    public class ApplicationUser<TKey>:IdentityUser<Guid>
    {
        [PersonalData]
        public string AlternatePhoneNumber { get; set; }

        public ApplicationUser(string userName):base(userName)
        {

        }
    }
}
