using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin thiết bị
    /// </summary>
    ///  Created by: PTTAM (07/03/2023)
    public class Equipment : BaseEntity
    {
        /// <summary>
        /// Khóa chính thiết bị
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [PropertyNameDisplay(propName: "Khóa chính thiết bị")]
        [DataLength(36)]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        public Guid EquipmentID { get; set; }

        /// <summary>
        /// Mã thiết bị
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(20)]
        [PropertyNameDisplay(propName: "Mã thiết bị")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public string EquipmentCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên thiết bị
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(255)]
        [PropertyNameDisplay(propName: "Tên thiết bị")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string EquipmentName { get; set; } = string.Empty;

        /// <summary>
        /// Khóa ngoại phòng ban
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(36)]
        [PropertyNameDisplay(propName: "Mã loại thiết bị")]
        [ForGetting]
        [Ambiguous]
        [ForBinding]
        [NotEmpty]

        public Guid EquipmentTypeID { get; set; }

    }
}
