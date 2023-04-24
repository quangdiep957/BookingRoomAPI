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
        public Task<List<SchedulerBooking>> GetPaging(PagingParam param, MySqlConnection cnn);
        public Task<Object> GetPagingRequest(PagingParam param, MySqlConnection cnn);

        public Task<bool> InsertMultiTimeBooking(List<TimeBooking> listRoom, MySqlTransaction transaction, MySqlConnection cnn);

        public Task<bool> DeleteRecord(Guid entityId, string tablename, MySqlConnection cnn, MySqlTransaction transaction);

        public Task<ParamReport> GetParamReport(Guid id ,MySqlConnection cnn);
    }

}
