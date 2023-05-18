using Newtonsoft.Json.Converters;
using RoomBooking.Common.AttributeCustom;
using RoomBooking.Common.Enum;
using RoomBooking.Common.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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

        /// <summary>
        /// Ngày gửi yêu cầu
        /// </summary>
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
        public string? RefusalReason { get; set; } = string.Empty;

        /// <summary>
        /// Năm học
        /// </summary>
        [ForGetting]
        [ForBinding]
        public int? YearPlan { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        [ForGetting]
        [ForBinding]
        public int? Quantity { get; set; }

        [ForGetting]
        [ForBinding]
        [JsonConverter(typeof(StringEnumConverter))]
        public int? StatusBooking { get; set; } = (int?)StatusBookingRoom.Pending;

        /// <summary>
        /// Tên ca 
        /// </summary>
        [ForGetting]
        [ForBinding]
        public string? TimeSlotName { get; set; } = string.Empty;
        public string? Room { get; set; }
        public string? DayOfWeek { get; set; }

        public string? Week { get; set; }
        [ForGetting]
        [ForBinding]
        public string? TimeSlots { get; set; }
        public string? Building { get; set; }
        public string? Time { get; set; }
        public string? MorningFreePeriod { get; set; }
        public string? AfternoonFreePeriod { get; set; }
        public string? EveningFreePeriod { get; set; }

        public int? Day { get; set; }
        public string? SlotTime { get; set; }
        public int? Times { get; set; }

        public string? FullName { get; set; }
        public string? BuildingName { get; set; }
        public string? RoomName { get; set; }
        public string? AvartarColor { get; set; }
        [ForGetting]
        public TimeSpan StartTime { get; set; }

        public string? AdminEmail  {get; set;}

        public Guid? AdminID { get; set; }

        public string? AdminName { get; set; }
        public string? SupporterEmail { get; set; }

        public Guid? SupporterID { get; set; }

        public string? SupporterName { get; set; }

        public string? BookingStatusColor
        {

            get
            {
                switch (StatusBooking)
                {
                    case (int?)StatusBookingRoom.Pending:
                        return Resource.PendingColor;
                    case (int?)StatusBookingRoom.Browse:
                        return Resource.BrowseColor;
                    case (int?)StatusBookingRoom.Miss:
                        return Resource.MissColor;
                    case (int?)StatusBookingRoom.Cancel:
                        return Resource.CancelColor;
                    default:
                        break;
                }
                return Resource.Pending;
            }
        }
        public string? BookingStatusName
        {

            get
            {
                switch (StatusBooking)
                {
                    case (int?)StatusBookingRoom.Pending:
                        return Resource.Pending;
                    case (int?)StatusBookingRoom.Browse:
                        return Resource.Browse;
                    case (int?)StatusBookingRoom.Miss:
                        return Resource.Miss;
                    case (int?)StatusBookingRoom.Cancel:
                        return Resource.Cancel;
                    default:
                        break;
                }
                return Resource.Pending;
            }
        }
    }

    public class BookingError
    {
        public string Error { get; set; }
        public string DescriptionError { get; set; }
    }
    public class ParamReport
    {
        public string FullName { get; set; }
        public string AdminName { get; set; }
        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public Guid BookingRoomID { get; set; }

        public DateTime StartDate { get; set; }

        public string RoomName { get; set; }
        public string EquipmentName { get; set; }

        //public string TimeSlotName { get; set; }


        public string BuildingName { get; set; }

        public string DateMiss { get; set; }
        public int Capacity { get; set; }

        public int StatusBooking { get; set; }
        public string Footer { get; set; }
        public string Header { get; set; }
        public string RefusalReason { get; set; }
        public string TimeSlotName { get; set; }
    }
}
