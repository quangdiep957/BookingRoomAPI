using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities.Params
{
    public class BookingRoomParam
    {
        public BookingRoom booking;
        public Guid userID;
        public Guid bookingRoomID;
        public int option;
        public string refusalReason;
        public string FullName;
    }
}
