
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
namespace ApiGateway.Security
{ 

    public class ClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity is not ClaimsIdentity identity)
                return Task.FromResult(principal);

            // Look for the URI-based role claim
            var uriRoleClaim = identity.FindFirst(ClaimTypes.Role);

            if (uriRoleClaim != null && !identity.HasClaim("role", uriRoleClaim.Value))
            {
                // Add simplified role claim
                identity.AddClaim(new Claim("role", uriRoleClaim.Value));
            }

            return Task.FromResult(principal);
        }
    }

}
