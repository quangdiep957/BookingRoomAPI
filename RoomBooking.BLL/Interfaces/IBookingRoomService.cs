using MySqlConnector;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Interfaces
{
    public interface IBookingRoomService:IBaseService<BookingRoom>
    {
        /// <summary>
        /// Import file excel phòng học 
        /// </summary>
        /// <param name="filePath"></param>
        /// PTTAM 25.03.20223
        public Task<Object> ReadExcelFile(string filePath);

        /// <summary>
        /// Thực hiện phân trang danh sách phòng chưa sử dụng | đang sử dụng
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="Type"></param>
        /// <param name="Week"></param>
        /// <param name="keyWord"></param>
        /// <param name="RoomID"></param>
        /// <param name="BuildingID"></param>
        /// <param name="TimeSlotID"></param>
        /// PTTAM 25/03/2023
        public Task<object> GetPaging(PagingParam param);
        /// <summary>
        /// Xử lý yêu cầu duyệt phòng
        /// </summary>
        /// <param name="requestID">Lịch đặt phòng</param>
        /// <param name="option">
        /// 1. Đồng ý
        /// 2.Từ chối
        /// </param>
        /// <returns></returns>
        public Task<object> RequestBookingRoom(Guid requestID, int option);

        /// <summary>
        /// Gọi phân trang cho các yêu cầu chờ duyệt
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Task<object> GetPagingRequest(PagingParam param);

        /// <summary>
        /// Thực hiện gửi yêu cầu đặt phòng
        /// </summary>
        /// <param name="bookings"></param>
        /// <returns></returns>
        public Task<object> InsertBookingRequest(BookingRoom bookings);

    }
}
