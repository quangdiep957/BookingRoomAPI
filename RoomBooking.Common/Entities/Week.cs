using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin tuần
    /// </summary>
    ///  Created by: PTTAM (07/03/2023)
    public class Week : BaseEntity
    {
        /// <summary>
        /// Khóa chính tuần
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(36)]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public Guid WeekrID { get; set; }

        /// <summary>
        /// Mã tuần
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(20)]
        [PropertyNameDisplay(propName: "Mã tuần")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string WeekrCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên tuần
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(255)]
        [PropertyNameDisplay(propName: "Tên tuần")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string WeekrName { get; set; } = string.Empty;

        /// <summary>
        /// Ngày bắt đầu tuần
        /// </summary>
        [PropertyNameDisplay(propName: "Ngày bắt đầu")]
        [ForGetting]
        [ForBinding]
        [DateField]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc tuần
        /// </summary>
        [PropertyNameDisplay(propName: "Ngày kết thúc")]
        [ForGetting]
        [ForBinding]
        [DateField]
        public DateTime EndDate { get; set; }
    }
}
