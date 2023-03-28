using RoomBooking.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Interfaces
{
    public interface ITokenService : IBaseService<User>
    {
        public Task<Token> GenerateToken(User user);
        public Task<bool> ValidateToken(string token);
        public Task InvalidateToken(string token);
    }
}
