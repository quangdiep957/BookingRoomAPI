using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin loại thiết bị
    /// </summary>
    ///  Created by: PTTAM (07/03/2023)
    public class EquipmentType : BaseEntity
    {
        /// <summary>
        /// Khóa chính loại thiết bị
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(36)]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public Guid EquipmentTypeID { get; set; }

        /// <summary>
        /// Mã loại thiết bị
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(20)]
        [PropertyNameDisplay(propName: "Mã loại thiết bị")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string EquipmentTypeCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên loại thiết bị
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(255)]
        [PropertyNameDisplay(propName: "Tên loại thiết bị")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string EquipmentTypeName { get; set; } = string.Empty;

    }
}
