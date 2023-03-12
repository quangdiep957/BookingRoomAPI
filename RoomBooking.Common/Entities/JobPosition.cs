using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin vị trí
    /// </summary>
    ///  Created by: PTTAM (07/03/2023)
    public class JobPosition : BaseEntity
    {
        /// <summary>
        /// Khóa chính vị trí
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [PropertyNameDisplay(propName: "Khóa chính vị trí")]
        [MISADataLength(36)]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        public Guid JobPositionID { get; set; }

        /// <summary>
        /// Mã vị trí
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(20)]
        [PropertyNameDisplay(propName: "Mã vị trí")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public string JobPositionCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên vị trí
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(255)]
        [PropertyNameDisplay(propName: "Tên vị trí")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string JobPositionName { get; set; } = string.Empty;
    }
}
