using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Functions
{
    public class CommonFunction
    {
        /// <summary>
        /// Thực hiện validate Email
        /// </summary>
        /// <param name="email"></param>
        /// <returns>true: hợp lệ false: không hợp lệ</returns>
        ///  Created by: PTTAM (06/03/2023)
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return true;
            }
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
    }
}

