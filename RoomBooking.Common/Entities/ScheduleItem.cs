using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    public class ScheduleItem
    {
        public string Building { get; set; }
        public string Room { get; set; }
        public string DayOfWeek { get; set; }
        public string Time { get; set; }
        public string MorningFreePeriod { get; set; }
        public string AfternoonFreePeriod { get; set; }
        public string EveningFreePeriod { get; set; }
        public string Week { get; set; }
        public int Day { get; set; }
    }
    public class ScheduleConvert
    {
        public string BuildingCode { get; set; }
        public string RoomCode { get; set; }
        public string SlotTime { get; set; }
        public string DayOfWeek { get; set; }
        public int Time { get; set; }
        public string Week { get; set; }
    }
}
