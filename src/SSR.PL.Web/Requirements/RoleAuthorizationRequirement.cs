using Microsoft.AspNetCore.Authorization;

namespace SSR.PL.Web.Requirements
{
    public class RoleAuthorizationRequirement:IAuthorizationRequirement
    {
        public string Role { get; set; }

        public RoleAuthorizationRequirement(string role)
        {
            Role = role;
        }
    }
}
