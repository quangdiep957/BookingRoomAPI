using RoomBooking.Common.Entities;
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
        public Task<object> GetPaging(int pageSize, int pageIndex, int type, string week, string? keyWord, Guid? roomID, Guid? buildingID, Guid? timeSlotID);
    }
}
