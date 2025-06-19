using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace CCFlockCLI.Services.JWT
{
    public static class JWTokenDecoder
    {
        public static string TokenDecoder(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var res = jwtToken.Claims.Select(u => new { u.Type, u.Value, u.Issuer }).Where(u => u.Type == "unique_name" || u.Type == "sub").ToList();
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(res, options);
        }
    }
}
namespace CCFlockCLI.Services.JWT
{
    public static class JWTokenGenerator
    {
        public static JwtSecurityToken? JWT { get; set; } = null;
        public static string TokenGenerator(string id, string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("7bATceklEQce5y28HMFU1YwYJcBna4Jz");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.UniqueName, username),
                    new Claim(JwtRegisteredClaimNames.Sub, id),
                }
                ),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = "ccflock",
                Audience = "ccflock-client",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            JWT = token;

            return tokenString;
        }
    }
}