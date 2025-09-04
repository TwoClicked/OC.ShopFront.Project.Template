using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OC.LUAC.ObjectLayer.Accounts;

namespace OC.LUAC.ApiLayer.Auth
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _opt;
        private readonly SymmetricSecurityKey _key;

        public JwtTokenService(IOptions<JwtOptions> opt)
        {
            _opt = opt.Value;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        }

        public string CreateCustomerToken(Customer c)
        {
            var claims = new List<Claim>
            {
                // Standard "sub" is the email
                new Claim(JwtRegisteredClaimNames.Sub, c.Email ?? $"customer-{c.Id}"),

                // Only use this custom claim for numeric IDs
                new Claim("customerId", c.Id.ToString()),

                // Role claim for auth
                new Claim(ClaimTypes.Role, "Customer")
            };

            return CreateToken(claims);
        }

        public string CreateAdminToken(string email)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim("adminEmail", email), // explicit admin identifier
                new Claim("role", "Admin")
            };

            return CreateToken(claims);
        }

        private string CreateToken(IEnumerable<Claim> claims)
        {
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_opt.AccessTokenMinutes),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
