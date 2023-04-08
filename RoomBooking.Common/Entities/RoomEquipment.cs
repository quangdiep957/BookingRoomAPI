using RoomBooking.Common.AttributeCustom;
using RoomBooking.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    public class RoomEquipment:BaseEntity
    {
        /// <summary>
        /// Khóa chính vai trò
        /// </summary>
        [ForGetting]
        public Guid RoomID { get; set; }

        /// <summary>
        /// Khóa chính nhân viên
        /// </summary>
        [ForGetting]
        public Guid EquipmentID { get; set; }

 
    }
}
