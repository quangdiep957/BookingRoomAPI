using System;
using System.Collections.Generic;
using System.Linq;


class Program
{
    public class RoomUsage
    {
        public DateTime BookingDate { get; set; }
        public int Timeslot { get; set; }
        public List<int> UsedRooms { get; set; }
        public List<int> UnusedRooms { get; set; }
    }

    //public static List<RoomUsage> GetRoomUsageForWeek(DateTime startDate, int weekCount)
    //{
    //    List<RoomUsage> roomUsages = new List<RoomUsage>();
    //    // phòng lấy từ db
    //    List<RoomSchedule> weeklySchedule = CreateWeeklySchedule(startDate, weekCount);
    //    List<int> roomIds = Enumerable.Range(1, 10).ToList(); // 10 phòng

    //    for (int j = 0; j < 6; j++) // 6 ca sử dụng phòng trong một ngày
    //    {
    //        for (int i = 0; i < weekCount * 7; i++) // Tổng số ngày trong tuần
    //        {
    //            DateTime bookingDate = startDate.AddDays(i);

    //            // Lấy danh sách các phòng được sử dụng cho ca sử dụng phòng này
    //            var usedRooms = weeklySchedule.Where(rs => rs.BookingDate == bookingDate && rs.Timeslot == j + 1).Select(rs => rs.RoomId).ToList();

    //            // Lấy danh sách các phòng chưa được sử dụng cho ca sử dụng phòng này
    //            var unusedRooms = roomIds.Except(usedRooms);

    //            RoomUsage roomUsage = new RoomUsage
    //            {
    //                BookingDate = bookingDate,
    //                Timeslot = j + 1,
    //                UsedRooms = usedRooms,
    //                UnusedRooms = unusedRooms.ToList()
    //            };

    //            roomUsages.Add(roomUsage);
    //        }
    //    }

    //    return roomUsages;
    //}
    public static List<int> GetUsedRooms(DateTime bookingDate, int timeslot)
    {
        // Lấy danh sách các phòng đã được sử dụng cho ca sử dụng phòng này trong ngày bookingDate
        List<int> usedRooms = new List<int>();

        // TODO: Truy vấn vào database để lấy ra danh sách các phòng đã được sử dụng trong ngày bookingDate và timeslot

        return usedRooms;
    }
    public static void GetRoomUsageForWeek(DateTime startDate, int weekCount)
    {
        List<int> roomIds = Enumerable.Range(1, 10).ToList(); // 10 phòng

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
