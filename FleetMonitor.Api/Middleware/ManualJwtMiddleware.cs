using System.Security.Claims;
using FleetMonitor.Api.Services;

namespace FleetMonitor.Api.Middleware {
    public class ManualJwtMiddleware {
        private readonly RequestDelegate _next;
        public ManualJwtMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx, JwtService jwt) {
            if(ctx.Request.Headers.TryGetValue("Authorization", out var header)) {
                var tok = header.ToString().Replace("Bearer ", "").Trim();
                var (valid, claims) = jwt.ValidateToken(tok);
                if(valid){
                    var identity = new ClaimsIdentity("ManualJwt");
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, claims["sub"].ToString()));
                    if(claims.TryGetValue("role", out var role))
                        identity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
                    ctx.User = new ClaimsPrincipal(identity);
                }
            }
            await _next(ctx);
        }
    }
}
