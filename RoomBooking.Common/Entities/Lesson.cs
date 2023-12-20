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
    public class Lesson : BaseEntity
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
        public Guid LessonID { get; set; }

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
        public int LessonCode { get; set; }

        /// <summary>
        /// Tên lớp
        /// </summary>
        ///  Created by: bqdiep (07/03/2023)
        [DataLength(255)]
        [PropertyNameDisplay(propName: "Tên lớp")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public string LessonName { get; set; }
        [ForGetting]
        [ForBinding]
        public TimeSpan StartTime { get; set; }
        [ForGetting]
        [ForBinding]
        public TimeSpan EndTime { get; set; }
    }
}
