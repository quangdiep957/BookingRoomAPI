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
        /// Khóa ngoại người dùng
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        [DataLength(36)]
        [PropertyNameDisplay(propName: "Mã người dùng")]
        [ForGetting]
        [ForBinding]
        [NotEmpty]
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
        /// Khóa chính phòng
        /// </summary>
        [DataLength(36)]
        [ForGetting]
        [ForBinding]
        [Ambiguous]
        [NotEmpty]

        public Guid RoomID { get; set; }

        /// <summary>
        /// Khóa chính thời gian
        /// </summary>
        [DataLength(36)]
        [ForGetting]
        [ForBinding]
        [Ambiguous]
        [NotEmpty]

        public Guid TimeSlotID { get; set; }

        /// <summary>
        /// Khóa chính thời gian
        /// </summary>
        [DataLength(36)]
        [ForGetting]
        [ForBinding]
        [NotEmpty]

        public Guid WeekID { get; set; }

        /// <summary>
        /// Khóa chính kì học
        /// </summary> }

        /// <summary>
        /// Chủ đề
        /// </summary>
        [ForGetting]
        [ForBinding]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// BBC tới 
        /// </summary>
        [ForGetting]
        [ForBinding]
        public string CCTo { get; set; } = string.Empty;

        [ForGetting]
        [ForBinding]
        public DateTime DateBooking { get; set; }

        [ForGetting]
        [ForBinding]
        public DateTime DateRequest { get; set; } = DateTime.Now;
        /// <summary>
        /// Nội dung
        /// </summary>
        [ForGetting]
        [ForBinding]
        public string Description { get; set; } = string.Empty;
        [ForGetting]
        [ForBinding]
        public int YearPlan { get; set; }
        [ForGetting]
        [ForBinding]
        public int StatusBooking { get; set; } = (int)OptionRequest.Await;

        public string Room { get; set; }
        [ForGetting]
        [ForBinding]
        public string DayOfWeek { get; set; }
        [ForGetting]
        [ForBinding]
        public string Week { get; set; }
        public string Building { get; set; }
        public string Time { get; set; }
        public string MorningFreePeriod { get; set; }
        public string AfternoonFreePeriod { get; set; }
        public string EveningFreePeriod { get; set; }

        public int Day { get; set; }
        public string SlotTime { get; set; }
        public int Times { get; set; }
        public List<Guid> TimeSlots { get; set; }

    }

    public class BookingError
    {
        public string Error { get; set; }
        public string DescriptionError { get; set; }
    }
}
