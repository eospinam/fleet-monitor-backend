using FleetMonitor.Api.Services;
using Xunit;
using FluentAssertions;

public class JwtServiceTests
{
    [Fact]
    public void CreateToken_ShouldReturn_ValidToken()
    {
        
        var secret = "EstaEsUnaClaveSuperSecretaDeAlMenos32Caracteres";
        var jwt = new JwtService(secret);

        
        var token = jwt.CreateToken("123", "admin", TimeSpan.FromHours(1));
        var (valid, claims) = jwt.ValidateToken(token);

        
        valid.Should().BeTrue();
        claims["sub"].Should().Be("123");
        claims["role"].Should().Be("admin");
    }
}
