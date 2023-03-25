using MySqlConnector;
using RoomBooking.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.DAL.Interfaces
{
    public interface IBookingRoomRepository : IBaseRepository<BookingRoom>
    {
        public Task<object> GetPaging(int pageSize, int pageIndex, int type, string listDate, MySqlConnection cnn, string? keyWord, Guid? roomID, Guid? buildingID, Guid? timeSlotID);
        public Task<List<string>> CheckRoom(List<BookingRoom> listRoom);

    }

}
