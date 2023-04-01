using MySqlConnector;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.DAL.Interfaces
{
    public interface IBookingRoomRepository : IBaseRepository<BookingRoom>
    {
        public Task<object> GetPaging(PagingParam param, MySqlConnection cnn);
        public Task<List<string>> CheckRoom(List<BookingRoom> listRoom);
        public Task<Object> GetPagingRequest(PagingParam param, MySqlConnection cnn);

    }

}
