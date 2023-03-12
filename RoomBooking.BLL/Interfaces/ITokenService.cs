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
        public string GenerateJwtToken(User user);
        public void RevokeToken(string token);
        public bool IsValidToken(string token);
    }
}
