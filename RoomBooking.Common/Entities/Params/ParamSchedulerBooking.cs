using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities.Params
{
    public class ParamSchedulerBooking
    {
        public List<SchedulerBooking> bookings;
        public List<Room> rooms;
        public int CurrentPage { get; set; }
        public int EndRecord { get; set; }
        public int StartRecord { get; set; }
        public int TotalPage { get; set; }
        public int TotalRecord { get; set; }
        public int TotalPages { get; set; }
    }
}
