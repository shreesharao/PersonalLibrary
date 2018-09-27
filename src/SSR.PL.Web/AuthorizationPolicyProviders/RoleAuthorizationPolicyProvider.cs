using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSR.PL.Web.Requirements;
using System.Threading.Tasks;

namespace SSR.PL.Web.AuthorizationPolicyProviders
{
    public class RoleAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        const string POLICY_PREFIX = "Role";
        private readonly ILogger _logger;
        private readonly DefaultAuthorizationPolicyProvider _defaultAuthorizationPolicyProvider;

        public RoleAuthorizationPolicyProvider(ILoggerFactory loggerFactory, IOptions<AuthorizationOptions> authorizationOptions)
        {
            _defaultAuthorizationPolicyProvider = new DefaultAuthorizationPolicyProvider(authorizationOptions);
            _logger = loggerFactory.CreateLogger<RoleAuthorizationPolicyProvider>();
        }

        /// <summary>
        /// Gets the default authorization policy.
        /// </summary>
        /// <returns>The default authorization policy.</returns>
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return _defaultAuthorizationPolicyProvider.GetDefaultPolicyAsync();
        }

        /// <summary>
        /// Gets a <see cref="AuthorizationPolicy"/> from the given <paramref name="policyName"/>
        /// </summary>
        /// <param name="policyName">The policy name to retrieve.</param>
        /// <returns>The named <see cref="AuthorizationPolicy"/>.</returns>
        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            _logger.LogTrace($"PolicyName:{policyName}");
            if (policyName.StartsWith(POLICY_PREFIX))
            {
                var role = policyName.Substring(POLICY_PREFIX.Length);
                var policy = new AuthorizationPolicyBuilder();
                policy.Requirements.Add(new RoleAuthorizationRequirement(role));
                return Task.FromResult(policy.Build());
            }

            return _defaultAuthorizationPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
