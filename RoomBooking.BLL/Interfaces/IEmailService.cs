using RoomBooking.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email
        /// </summary>
        /// <param name="emailData"></param>
        /// <param name="emailSettings"></param>
        /// <returns></returns>
        bool SendEmail(EmailData emailData);
    }
}
