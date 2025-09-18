using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FleetMonitor.Api.Services
{
    public class ManualJwtHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly JwtService _jwt;

        public ManualJwtHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            JwtService jwt)
            : base(options, logger, encoder, clock)
        {
            _jwt = jwt;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
            var (valid, claims) = _jwt.ValidateToken(token);

            if (!valid)
                return Task.FromResult(AuthenticateResult.Fail("Invalid Token"));

            var identity = new ClaimsIdentity("ManualJwt");
            foreach (var kv in claims)
            {
                if (kv.Key == "role")
                {
                    // Mapear a ClaimTypes.Role para que [Authorize(Roles="admin")] lo reconozca
                    identity.AddClaim(new Claim(ClaimTypes.Role, kv.Value?.ToString() ?? ""));
                }
                else if (kv.Key == "sub")
                {
                    // Mapear el "sub" a NameIdentifier para poder usar User.Identity.Name
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, kv.Value?.ToString() ?? ""));
                }
                else
                {
                    identity.AddClaim(new Claim(kv.Key, kv.Value?.ToString() ?? ""));
                }
            }

            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "ManualJwt");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
