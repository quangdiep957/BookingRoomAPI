using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin kì học
    /// </summary>
    ///  Created by: PTTAM (07/03/2023)
    public class Semester : BaseEntity
    {
        /// <summary>
        /// Khóa chính kì học
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(36)]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public Guid SemesterID { get; set; }

        /// <summary>
        /// Mã kì học
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(20)]
        [PropertyNameDisplay(propName: "Mã kì học")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string SemesterCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên kì học
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(255)]
        [PropertyNameDisplay(propName: "Tên kì học")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string SemesterName { get; set; } = string.Empty;

        /// <summary>
        /// Ngày bắt đầu kì học
        /// </summary>
        [PropertyNameDisplay(propName: "Ngày bắt đầu")]
        [ForGetting]
        [ForBinding]
        [DateField]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc kì học
        /// </summary>
        [PropertyNameDisplay(propName: "Ngày kết thúc")]
        [ForGetting]
        [ForBinding]
        [DateField]
        public DateTime EndDate { get; set; }
    }
}
