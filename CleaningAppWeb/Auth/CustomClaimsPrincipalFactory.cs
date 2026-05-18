using CleaningAppWeb.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace CleaningAppWeb.Auth
{
    public class CustomClaimsPrincipalFactory(
        UserManager<User> userManager,
        IOptions<IdentityOptions> optionsAccessor
    ) : UserClaimsPrincipalFactory<User>(userManager, optionsAccessor)
    {
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));

            identity.AddClaim(new Claim("FirstName", user.FirstName));
            identity.AddClaim(new Claim("LastName", user.LastName));
            identity.AddClaim(new Claim("Patronymic", user.Patronymic));
            identity.AddClaim(new Claim("TelephoneNumber", user.TelephoneNumber));

            return identity;
        }
    }
}
