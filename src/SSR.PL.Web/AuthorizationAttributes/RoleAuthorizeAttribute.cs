using Microsoft.AspNetCore.Authorization;

namespace SSR.PL.Web.AuthorizationAttributes
{
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private const string POLICY_PREFIX = "Role";
        public string Role
        {
            get
            {
                return Policy.Substring(POLICY_PREFIX.Length);
            }
            set
            {
                Policy = $"{POLICY_PREFIX}{value}";
            }
        }
        public RoleAuthorizeAttribute(string role)
        {
            Role = role;
        }
    }
}
