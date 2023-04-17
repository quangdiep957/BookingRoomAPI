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
    public class TimeSlot : BaseEntity
    {
        /// <summary>
        /// Khóa chính thời gian
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
        /// Tên thời gian
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(255)]
        [PropertyNameDisplay(propName: "Tên thời gian")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public int TimeSlotName { get; set; }
        [ForGetting]
        [ForBinding]
        [NotEmpty]
        public TimeSpan StartTime { get; set; }
        [ForGetting]
        [ForBinding]
        [NotEmpty]
        public TimeSpan EndTime { get; set; }

    }
}
