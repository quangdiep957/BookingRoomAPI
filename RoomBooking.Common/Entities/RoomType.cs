using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin loại phòng
    /// </summary>
    ///  Created by: PTTAM (07/03/2023)
    public class RoomType : BaseEntity
    {
        /// <summary>
        /// Khóa chính loại phòng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(36)]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public Guid RoomTypeID { get; set; }

        /// <summary>
        /// Mã loại phòng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(20)]
        [PropertyNameDisplay(propName: "Mã loại phòng")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string RoomTypeCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên loại phòng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(255)]
        [PropertyNameDisplay(propName: "Tên loại phòng")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string RoomTypeName { get; set; } = string.Empty;

    }
}
