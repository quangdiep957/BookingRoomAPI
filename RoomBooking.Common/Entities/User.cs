using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin người dùng
    /// </summary>
    /// Created by: PTTAM (07/03/2023)
    public class User : BaseEntity
    {
        /// <summary>
        /// Khóa chính người dùng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(36)]
        [PrimaryKey]
        [ForGetting]
        [ForBinding]
        [PropertyNameDisplay(propName: "Khóa chính người dùng")]
        public Guid UserID { get; set; }

        /// <summary>
        /// Mã người dùng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(20)]
        [PropertyNameDisplay(propName: "Mã người dùng")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public string UserCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên người dùng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(100)]
        [PropertyNameDisplay(propName: "Tên người dùng")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(255)]
        [PropertyNameDisplay(propName: "Địa chỉ")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(255)]
        [PropertyNameDisplay(propName: "Mật khẩu")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(100)]
        [PropertyNameDisplay(propName: "Email")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Khóa ngoại phòng ban
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(36)]
        [PropertyNameDisplay(propName: "Mã phòng ban")]
        [ForGetting]
        [Ambiguous]
        [ForBinding]
        [NotEmpty]

        public Guid DepartmentID { get; set; }


        /// <summary>
        /// Màu avartar người dùng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [ForGetting]
        [ForBinding]

        public string AvartarColor { get; set; } = string.Empty;

        /// <summary>
        /// Tên vai trò của nhân viên
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [ForBinding]
        public string RoleNames { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(100)]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public int isAdmin { get; set; } = 0;

        /// <summary>
        /// Danh sách vai trò của nhân viên
        /// </summary>
        public List<User_Role> UserRoles { get; set; }



        /// <summary>
        /// Tên phòng ban
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        public string? DepartmentName { get; set; }

  

    }
}
