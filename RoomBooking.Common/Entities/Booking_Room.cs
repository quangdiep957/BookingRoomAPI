using RoomBooking.Common.AttributeCustom;
using RoomBooking.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    public class Booking_Room:BaseEntity
    {
        /// <summary>
        /// Khóa ngoại người dùng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [MISADataLength(36)]
        [PropertyNameDisplay(propName: "Mã người dùng")]
        [ForGetting]
        [Ambiguous]
        [ForBinding]
        [NotEmpty]

        public Guid UserID { get; set; }
        /// <summary>
        /// Khóa chính phòng
        /// </summary>
        [ForGetting]
        public Guid RoomID { get; set; }

        /// <summary>
        /// Khóa chính thời gian
        /// </summary>
        [ForGetting]
        public Guid TimeSlotID { get; set; }

        /// <summary>
        /// Khóa chính kì học
        /// </summary>
        [ForGetting]
        public Guid SemesterID { get; set; }

        /// <summary>
        /// Chủ đề
        /// </summary>
        [ForGetting]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// BBC tới 
        /// </summary>
        [ForGetting]
        public string AsignTo { get; set; } = string.Empty;


        /// <summary>
        /// Nội dung
        /// </summary>
        [ForGetting]
        public string Description { get; set; } = string.Empty;

    }
}
