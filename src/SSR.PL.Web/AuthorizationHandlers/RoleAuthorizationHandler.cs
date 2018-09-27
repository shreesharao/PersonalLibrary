using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using SSR.PL.Web.Requirements;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SSR.PL.Web.AuthorizationHandlers
{
    public class RoleAuthorizationHandler : AuthorizationHandler<RoleAuthorizationRequirement>
    {
        private readonly ILogger _logger;
        public RoleAuthorizationHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RoleAuthorizationHandler>();
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleAuthorizationRequirement roleAuthorizationRequirement)
        {
            if (roleAuthorizationRequirement == null)
            {
                throw new System.ArgumentNullException(nameof(roleAuthorizationRequirement));
            }

            //verify the requirement against the AuthorizationHandlerContext
            if (!context.User.HasClaim(claim => claim.Type == ClaimTypes.Role))
            {
                _logger.LogTrace($"User is not having Claim Type: {nameof(ClaimTypes.Role)}");
                return Task.CompletedTask;
            }

            if (context.User.FindFirst(claim => claim.Type == ClaimTypes.Role).Value == roleAuthorizationRequirement.Role)
            {
                _logger.LogTrace($"Authorization succeeded for the role:{roleAuthorizationRequirement.Role}");
                //mark the requirement as successfull
                context.Succeed(roleAuthorizationRequirement);
            }

            return Task.CompletedTask;
        }
    }
}
