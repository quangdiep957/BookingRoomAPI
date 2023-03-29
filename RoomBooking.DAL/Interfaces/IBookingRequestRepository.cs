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
    public interface IBookingRequestRepository: IBaseRepository<BookingRequest>
    {
        public Task<Object> GetPaging(PagingParam param, MySqlConnection cnn);
    }
    
}
