using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    public class Token
    {
        public string Value { get; set; }
        public DateTime Expiration { get; set; }
        public User User { get; set; }


    }
}
