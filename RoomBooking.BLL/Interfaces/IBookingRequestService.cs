using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Interfaces
{
    public interface IBookingRequestService : IBaseService<BookingRequest>
    {

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
    }
}
