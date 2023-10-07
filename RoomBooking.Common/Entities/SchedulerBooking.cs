using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    public class SchedulerBooking
    {
        public Guid BookingRoomID { get; set; }
        public Guid RoomID { get; set; }
        public Guid TimeSlotID { get; set; }
        public  string FullName { get; set; }
        public  string RoomName { get; set; }
        public int Quantity { get; set; }
        public int RoomStatus { get; set; }

        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public string AvartarColor { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string TimeSlots { get; set; }
        public string TimeSlotName { get; set; }
        public string endDate { get; set; }
        public string startDate { get; set; }
        public int StatusBooking { get;set; }
    }
}
