using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin phòng ban
    /// </summary>
    ///  Created by: PTTAM (07/03/2023)
    public class Department : BaseEntity
    {
        /// <summary>
        /// Khóa chính phòng ban
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [PropertyNameDisplay(propName: "Khóa chính phòng ban")]
        [DataLength(36)]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        public Guid DepartmentID { get; set; }

        /// <summary>
        /// Mã phòng ban
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(20)]
        [PropertyNameDisplay(propName: "Mã phòng ban")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public string DepartmentCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên phòng ban
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(255)]
        [PropertyNameDisplay(propName: "Tên phòng ban")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string DepartmentName { get; set; } = string.Empty;
    }
}
