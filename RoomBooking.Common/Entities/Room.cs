using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin phòng
    /// </summary>
    ///  Created by: PTTAM (07/03/2023)
    public class Room : BaseEntity
    {
        /// <summary>
        /// Khóa chính phòng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [PropertyNameDisplay(propName: "Khóa chính phòng")]
        [DataLength(36)]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        public Guid RoomID { get; set; }

        /// <summary>
        /// Mã phòng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(20)]
        [PropertyNameDisplay(propName: "Mã phòng")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public string RoomCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên phòng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(255)]
        [PropertyNameDisplay(propName: "Tên phòng")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string RoomName { get; set; } = string.Empty;

        /// <summary>
        /// Sức chứa
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(255)]
        [PropertyNameDisplay(propName: "Sức chứa")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string Capacity { get; set; } = string.Empty;

        /// <summary>
        /// Khóa ngoại tòa nhà
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(36)]
        [PropertyNameDisplay(propName: "Mã tòa nhà")]
        [ForGetting]
        [Ambiguous]
        [ForBinding]
        [NotEmpty]

        public Guid BuildingID { get; set; }

        /// <summary>
        /// Khóa ngoại loại phòng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(36)]
        [PropertyNameDisplay(propName: "Mã loại phòng")]
        [ForGetting]
        [Ambiguous]
        [ForBinding]
        [NotEmpty]

        public Guid RoomTypeID { get; set; }

        [ForGetting]
        [ForBinding]
        public int RoomStatus { get; set; }

        [ForBinding]
        [ForGetting]
        [DataLength(36)]
        public Guid UserID { get; set; }
    }
}
