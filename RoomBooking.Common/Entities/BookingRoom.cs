using RoomBooking.Common.AttributeCustom;
using RoomBooking.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    public class BookingRoom : BaseEntity
    {
        /// <summary>
        /// Khóa chính đặt phòng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(36)]
        [PropertyNameDisplay(propName: "Mã đặt phòng")]
        [ForGetting]
        [PrimaryKey]
        [ForBinding]
        [NotEmpty]
        [KeyDelete]
        public Guid BookingRoomID { get; set; }
        /// <summary>
        /// Khóa ngoại người dùng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(36)]
        [PropertyNameDisplay(propName: "Mã người dùng")]
        [ForGetting]
        [Ambiguous]
        [ForBinding]
        [NotEmpty]

        public Guid UserID { get; set; }
        /// <summary>
        /// Khóa ngoại phòng
        /// </summary>
        [DataLength(36)] 
        [ForGetting]
        [ForBinding]
        [Ambiguous]
        [NotEmpty]

        public Guid RoomID { get; set; }

 

        /// <summary>
        /// Chủ đề
        /// </summary>
        [ForGetting]
        [ForBinding]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Ngày bắt đầu 
        /// </summary>
        [ForGetting]
        [ForBinding]
        public DateTime StartDate { get; set; }


        /// <summary>
        /// Ngày kết thúc
        /// </summary>
        [ForGetting]
        [ForBinding]
        public DateTime EndDate { get; set; }
        [ForGetting]
        [ForBinding]
        public DateTime DateRequest { get; set; } = DateTime.Now;
        /// <summary>
        /// Nội dung
        /// </summary>
        [ForGetting]
        [ForBinding]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Lý do từ chối
        /// </summary>
        [ForGetting]
        [ForBinding]
        public string RefusalReason { get; set; } = string.Empty;
        [ForGetting]
        [ForBinding]
        public int YearPlan { get; set; }
        [ForGetting]
        [ForBinding]
        public int StatusBooking { get; set; } = (int)OptionRequest.Approve;

        public string Room { get; set; }
        public string DayOfWeek { get; set; }
        
        public string Week { get; set; }
        [ForGetting]
        [ForBinding]
        public string TimeSlots { get; set; }
        public string Building { get; set; }
        public string Time { get; set; }
        public string MorningFreePeriod { get; set; }
        public string AfternoonFreePeriod { get; set; }
        public string EveningFreePeriod { get; set; }

        public int Day { get; set; }
        public string SlotTime { get; set; }
        public int Times { get; set; }
     

        [ForGetting]
        public TimeSpan StartTime { get; set; }
    }

    public class BookingError
    {
        public string Error { get; set; }
        public string DescriptionError { get; set; }
    }
}
