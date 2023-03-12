using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Services
{
    public class TokenService : BaseService<User>, ITokenService
    {
        public TokenService(IBaseRepository<User> repository) : base(repository)
        {
        }
        private List<string> _tokens = new List<string>();
        public string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes("this is a secret key"); // should be more secure in real application
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.Name, user.UserID.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _tokens.Add(tokenString);

            return tokenString;
        }

        public bool IsValidToken(string token)
        {
           
            var key = Encoding.ASCII.GetBytes("this is a secret key"); // should be more secure in real application
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return _tokens.Contains(token);
            }
            catch
            {
                return false;
            }
        }

        public void RevokeToken(string token)
        {
            _tokens.Remove(token);
        }
    }
}
