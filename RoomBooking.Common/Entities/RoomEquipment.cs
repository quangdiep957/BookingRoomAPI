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
        [ForBinding]
        [KeyDelete]
        public Guid RoomID { get; set; }

        /// <summary>
        /// Khóa chính nhân viên
        /// </summary>
        [ForBinding]
        [ForGetting]
        public Guid EquipmentID { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>

        public int Quatily { get; set; }

        /// <summary>
        /// Tên vai trò
        /// </summary>
      
        public string EquipmentName { get; set; }

        /// <summary>
        /// Trạng thái khi sửa
        /// </summary>
      
        public UpdateMode State { get; set; }
    }
}
