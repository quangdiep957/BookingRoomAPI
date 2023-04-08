using RoomBooking.Common.AttributeCustom;
using RoomBooking.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    public class TimeBooking:BaseEntity
    {
        /// <summary>
        /// Khóa chính vai trò
        /// </summary>
        [ForGetting]
        public Guid TimeSlotID { get; set; }

        /// <summary>
        /// Khóa chính nhân viên
        /// </summary>
        [ForGetting]
        public Guid BookingRoomID { get; set; }

      
    }
}
