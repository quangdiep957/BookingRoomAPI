using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin thời gian
    /// </summary>
    ///  Created by: PTTAM (07/03/2023)
    public class Time_Slot : BaseEntity
    {
        /// <summary>
        /// Khóa chính thời gian
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(36)]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public Guid TimeSlotID { get; set; }

        /// <summary>
        /// Tên thời gian
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(255)]
        [PropertyNameDisplay(propName: "Tên thời gian")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string TimeSlotName { get; set; } = string.Empty;

    }
}
