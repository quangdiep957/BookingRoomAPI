using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    public class RoomSchedule
    {
        public Guid RoomID { get; set; }
        public DateTime BookingDate { get; set; }
        public int Timeslot { get; set; }
       public string BuildingName { get; set; }
        public string RoomName { get; set; }
    }

}
