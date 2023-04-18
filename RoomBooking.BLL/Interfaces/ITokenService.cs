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
        /// <summary>
        /// Tạo token
        /// </summary>
        /// <param name="user"></param>
        /// PTTAM 02/04/2023
        public Task<Token> GenerateToken(User user);

        /// <summary>
        /// Validate token
        /// </summary>
        /// <param name="token"></param>
        ///  PTTAM 02/04/2023
        public Task<bool> ValidateToken(string token);

        /// <summary>
        /// Xóa token
        /// </summary>
        /// <param name="token"></param>
        /// PTTAM 02.04.2023
        public Task InvalidateToken(string token);

        /// <summary>
        /// Xóa token
        /// </summary>
        /// <param name="token"></param>
        /// PTTAM 02.04.2023

    }
}
