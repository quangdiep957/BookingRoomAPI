using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin lớp
    /// </summary>
    ///  Created by: bqdiep (07/03/2023)
    public class Class : BaseEntity
    {
        /// <summary>
        /// Khóa chính lớp
        /// </summary>
        ///  Created by: bqdiep (07/03/2023)
        [PropertyNameDisplay(propName: "Khóa chính lớp")]
        [DataLength(36)]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        [ForGetImportID]
        public Guid ClassID { get; set; }

        /// <summary>
        /// Mã lớp
        /// </summary>
        ///  Created by: bqdiep (07/03/2023)
        [DataLength(20)]
        [PropertyNameDisplay(propName: "Mã lớp")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]
        [Unique]
        public string ClassCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên lớp
        /// </summary>
        ///  Created by: bqdiep (07/03/2023)
        [DataLength(255)]
        [PropertyNameDisplay(propName: "Tên lớp")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]
        [ForGetImportName]
        public string ClassName { get; set; } = string.Empty;
        public string Description { get; set; }
        [ForGetting]
        [ForBinding]
        public int BudgerYear { get; set; }
        [ForGetting]
        [ForBinding]
        public int Quantity { get; set; }
    }
}
