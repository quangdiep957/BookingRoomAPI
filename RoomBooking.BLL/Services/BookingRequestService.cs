using MySqlConnector;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Services
{
    public class BookingRequestService : BaseService<BookingRequest>, IBookingRequestService
    {
        IBookingRequestRepository _repository;
        public BookingRequestService(IBookingRequestRepository repository) : base(repository)
        {
            _repository = repository;
        }
        /// <summary>
        /// Thực hiện nghiệp vụ khi phân trang
        /// </summary>
        /// <param name="pageSize">Số bản ghi / trang</param>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="keyWord">Từ khóa</param>
        /// <param name="roleId">Khóa chính vai trò</param>
        /// <returns>Object chứa danh sách người dùng lọc được theo yêu cầu</returns>
        ///  Created by: PTTAM(10/9/2022)
        public async Task<object> GetPaging(PagingParam param)
        {
            object res = null;
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                res = await _repository.GetPaging(param,cnn);

            }
            return res;
        }
    }
}
