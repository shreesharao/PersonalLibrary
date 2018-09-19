using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SSR.PL.Web.Requirements;
using System.Security.Claims;

namespace SSR.PL.Web.AuthorizationHandlers
{
    public class RoleAuthorizationHandler : AuthorizationHandler<RoleAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleAuthorizationRequirement roleAuthorizationRequirement)
        {
            if (roleAuthorizationRequirement == null)
            {
                throw new System.ArgumentNullException(nameof(roleAuthorizationRequirement));
            }

            //verify the requirement against the AuthorizationHandlerContext
            if (!context.User.HasClaim(claim => claim.Type == ClaimTypes.Role))
            {
                return Task.CompletedTask;
            }

            if (context.User.FindFirst(claim => claim.Type == ClaimTypes.Role).Value == roleAuthorizationRequirement.Role)
            {
                //mark the requirement as successfull
                context.Succeed(roleAuthorizationRequirement);
            }

            return Task.CompletedTask;
        }
    }
}
