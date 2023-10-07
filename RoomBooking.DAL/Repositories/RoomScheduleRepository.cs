using Dapper;
using Microsoft.Extensions.Configuration;
using RoomBooking.Common.Entities;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace RoomBooking.DAL.Repositories
{
    public class RoomScheduleRepository : BaseRepository<RoomSchedule>, IRoomScheduleRepository
    {
        public RoomScheduleRepository(IConfiguration configuration) : base(configuration)
        {
        }
        public List<RoomSchedule> GetUsedRooms(DateTime bookingDate, int timeslot)
        {
            // Lấy danh sách các phòng đã được sử dụng cho ca sử dụng phòng này trong ngày bookingDate
            List<RoomSchedule> usedRooms = new List<RoomSchedule>();
            DynamicParameters param = new DynamicParameters();
            var sqlInsertUserRole = "SELECT r.RoomName,ts.TimeSlotName,br.DateBooking," +
                " b.BuildingName FROM room r  JOIN bookingroom br ON r.RoomID = br.RoomID  " +
                "JOIN timeslot ts ON br.TimeSlotID = ts.TimeSlotID JOIN building b ON b.BuildingID=r.BuildingID Where br.DateBooking=@DateBooking AND ts.TimeSlotName=@TimeSlotName";
            param.Add("@DateBooking", bookingDate);
            param.Add("@TimeSlotName", timeslot);
           
            var res = _sqlConnection.Query<RoomSchedule>(sqlInsertUserRole, param);
            
            return (List<RoomSchedule>)res;
            // TODO: Truy vấn vào database để lấy ra danh sách các phòng đã được sử dụng trong ngày bookingDate và timeslot

        }
        public void GetRoomUsageForWeek(DateTime startDate, int weekCount)
        {
            List<RoomSchedule> roomIds = new List<RoomSchedule>();

            for (int j = 0; j < 6; j++) // 6 ca sử dụng phòng trong một ngày
            {
                for (int i = 0; i < weekCount * 7; i++) // Tổng số ngày trong tuần
                {
                    DateTime bookingDate = startDate.AddDays(i);

                    // Lấy danh sách các phòng được sử dụng cho ca sử dụng phòng này
                    var usedRooms = GetUsedRooms(bookingDate, j + 1);

                    // Lấy danh sách các phòng chưa được sử dụng cho ca sử dụng phòng này
                    var unusedRooms = roomIds.Except(usedRooms);

                    Console.WriteLine($"Ngày {bookingDate.ToString("dd/MM/yyyy")}, Ca {j + 1}:");
                    Console.WriteLine($"Phòng được sử dụng: {string.Join(", ", usedRooms)}");
                    Console.WriteLine($"Phòng chưa được sử dụng: {string.Join(", ", unusedRooms)}");
                    Console.WriteLine();
                }
            }
        }
    }
}
