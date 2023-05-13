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
        /// <summary>
        /// Phân trang
        /// </summary>
        /// <param name="param"></param>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public Task<ParamSchedulerBooking> GetPaging(PagingParam param, MySqlConnection cnn);

        /// <summary>
        /// Phân trang các yêu cầu chờ duyệt
        /// </summary>
        /// <param name="param"></param>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public Task<Object> GetPagingRequest(PagingParam param, MySqlConnection cnn);

        /// <summary>
        /// Phân trang lịch sử đặt
        /// </summary>
        /// <param name="param"></param>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public Task<Object> GetPagingHistory(PagingParam param, MySqlConnection cnn);
        /// <summary>
        /// Insert nhiều
        /// </summary>
        /// <param name="listRoom"></param>
        /// <param name="transaction"></param>
        /// <param name="cnn"></param>
        /// <returns></returns>

        public Task<bool> InsertMultiTimeBooking(List<TimeBooking> listRoom, MySqlTransaction transaction, MySqlConnection cnn);

        /// <summary>
        /// Xóa
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="tablename"></param>
        /// <param name="cnn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public Task<bool> DeleteRecord(Guid entityId, string tablename, MySqlConnection cnn, MySqlTransaction transaction);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public Task<ParamReport> GetParamReport(Guid id ,MySqlConnection cnn);
    }

}
