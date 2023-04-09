using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin vai trò
    /// </summary>
    ///  Created by: PTTAM (07/03/2023)
    public class Role : BaseEntity
    {
        /// <summary>
        /// Khóa chính vai trò
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(36)]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public Guid RoleID { get; set; }

        /// <summary>
        /// Mã vai trò
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(20)]
        [PropertyNameDisplay(propName: "Mã vai trò")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string RoleCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên vai trò
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(255)]
        [PropertyNameDisplay(propName: "Tên vai trò")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Giá trị vai trò
        /// 1- chuyên viên, giảng viên
        /// 2 - Admin
        /// 3- Phụ trách phòng-
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [PropertyNameDisplay(propName: "Giá trị vai trò")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public int RoleValue{ get; set; }

    }
}
