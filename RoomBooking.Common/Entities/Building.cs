using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin tòa nhà
    /// </summary>
    ///  Created by: PTTAM (07/03/2023)
    public class Building : BaseEntity
    {
        /// <summary>
        /// Khóa chính tòa nhà
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(36)]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public Guid BuildingID { get; set; } 

        /// <summary>
        /// Mã tòa nhà
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(20)]
        [PropertyNameDisplay(propName: "Mã tòa nhà")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string BuildingCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên tòa nhà
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(255)]
        [PropertyNameDisplay(propName: "Tên tòa nhà")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string BuildingName { get; set; } = string.Empty;

    }
}
