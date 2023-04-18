using RoomBooking.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Interfaces
{
    public interface IUserService : IBaseService<User>
    {
        /// <summary>
        /// Thực hiện nghiệp vụ khi phân trang 
        /// </summary>
        /// <param name="pageSize">Số bản ghi trên 1 trang</param>
        /// <param name="pageIndex">Tổng số trang</param>
        /// <param name="keyWord">Dữ liệu chọn lọc để tìm kiếm</param>
        /// <returns>Object: chứa pageSize, pageIndex, data</returns>
        /// Created by: PTTAM (07/03/2023)
        public Task<object> GetPaging(int pageSize, int pageIndex, string? keyWord, Guid? roleId);



        /// <summary>
        /// Thực hiên nghiệp vụ khi lấy mã nhân viên mới của người dùng
        /// </summary>
        /// <returns>Mã nhân viên mới</returns>
        /// Created by:  PTTAM (07/03/2023)
        public Task<string> GetNewUserCode();

        
        /// <summary>
        /// Thực hiện nghiệp vụ check đăng nhập
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// Created by: PTTAM (02/04/2023)
        public Task<User> Authenticate(string username, string password);
        public Task<bool> ChangePass(User user);

    }
}
