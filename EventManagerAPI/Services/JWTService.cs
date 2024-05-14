using Domain.DTOs.UserDTO;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventManagerAPI.Services
{
    public class JWTService : IJWTService
    {
        private readonly AppSettingConfiguration _appSettingConfiguration;

        public JWTService(AppSettingConfiguration appSettingConfiguration)
        {
            _appSettingConfiguration = appSettingConfiguration;
        }

        public string GenerateToken(UserForRead userForReturn)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userForReturn.Username),
                new Claim(ClaimTypes.Name, userForReturn.Email),
                new Claim(ClaimTypes.Role, userForReturn.Role),
                new Claim("Id", userForReturn.Id.ToString())
            };

            byte[] secretInBytes = Encoding.UTF8.GetBytes(_appSettingConfiguration.JWTSection.SecretKey);
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(secretInBytes);
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _appSettingConfiguration.JWTSection.Issuer,
                audience: _appSettingConfiguration.JWTSection.Audience,
                expires: DateTime.UtcNow.AddMinutes(_appSettingConfiguration.JWTSection.ExpiresInMinutes),
                claims: claims,
                signingCredentials: credentials
            );

            JwtSecurityTokenHandler jwtTokenHandler = new JwtSecurityTokenHandler();
            string jwtToken = jwtTokenHandler.WriteToken(token);
            return jwtToken;
        }
    }
}
