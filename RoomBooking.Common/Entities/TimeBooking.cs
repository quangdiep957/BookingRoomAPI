using RoomBooking.Common.AttributeCustom;
using RoomBooking.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin tòa nhà
    /// </summary>
    ///  Created by: PTTAM (07/03/2023)
    public class TimeBooking : BaseEntity
    {
        /// <summary>
        /// Khóa chính tòa nhà
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(36)]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public Guid TimeSlotID { get; set; }
        /// <summary>
        /// Khóa chính tòa nhà
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(36)]
        [ForGetting]
        [ForBinding]
        [NotEmpty]
        [Unique]
        [KeyDelete]
        public Guid BookingRoomID { get; set; }


    }
}
