using RoomBooking.Common.AttributeCustom;
using RoomBooking.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    public class UserRole:BaseEntity
    {
        /// <summary>
        /// Khóa chính vai trò
        /// </summary>
        [ForGetting]
        public Guid RoleID { get; set; }

        /// <summary>
        /// Khóa chính nhân viên
        /// </summary>
        [ForGetting]
        public Guid UserID { get; set; }

        /// <summary>
        /// Tên vai trò
        /// </summary>
        [ForGetting]
        public string RoleName { get; set; }

        /// <summary>
        /// Trạng thái khi sửa
        /// </summary>
        [ForGetting]
        public UpdateMode State { get; set; }
    }
}
