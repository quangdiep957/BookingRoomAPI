using RoomBooking.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.DAL.Interfaces
{
    public interface IRoomScheduleRepository : IBaseRepository<RoomSchedule>
    {
        //public static List<RoomSchedule> GetUsedRooms(DateTime bookingDate, int timeslot);
       // public static void GetRoomUsageForWeek(DateTime startDate, int weekCount);
    }
}
