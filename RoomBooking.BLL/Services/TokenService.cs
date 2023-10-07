using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections;
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
        private readonly IMemoryCache _cache;
      //  private readonly TimeSpan _expirationTime;
        public TokenService(IBaseRepository<User> repository,IMemoryCache cache) : base(repository)
        {
            _cache = cache;
         //   _expirationTime = expirationTime;
        }


        //public TokenService(IMemoryCache cache, IUserRepository userRepository, TimeSpan expirationTime)
        //{
        //    _cache = cache;
        //    _userRepository = userRepository;
        //    _expirationTime = expirationTime;
        //}

        public async Task<Token> GenerateToken(User model)
        {
            var key = Encoding.ASCII.GetBytes("this is a secret key"); // should be more secure in real application
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.Name, model.UserCode)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            var newtoken = new Token
            {
                Value = tokenString,
                Expiration = DateTime.UtcNow.AddHours(1),
                User=model
            };

            // Lưu token vào cache
            
            _cache.Set( model.UserCode, newtoken.Value, newtoken.Expiration);
            _cache.Set("userCache", model);
            return newtoken;
        }

        public async Task<bool> ValidateToken(string tokenValue)
        {
            if (string.IsNullOrEmpty(tokenValue))
            {
                return false;
            }

            return _cache.TryGetValue(tokenValue, out _);
        }

        public async Task InvalidateToken(string tokenValue)
        {
            if (!string.IsNullOrEmpty(tokenValue))
            {
                _cache.Remove(tokenValue);
            }
        }

    }
}
