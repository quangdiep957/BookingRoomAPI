using RoomBooking.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.DAL.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        /// <summary>
        /// Thực hiện phân trang 
        /// </summary>
        /// <param name="pageSize">Số bản ghi trên 1 trang</param>
        /// <param name="pageIndex">Tổng số trang</param>
        /// <param name="keyWord">Dữ liệu chọn lọc để tìm kiếm</param>
        /// <returns>Object: chứa pageSize, pageIndex, data</returns>
        /// Created by: PTTAM (07/03/2023)
        public Task<object> GetPaging(int pageSize, int pageIndex, string? keyWord, Guid? roleId);



        /// <summary>
        /// Thực hiên lấy mã nhân viên mới của người dùng
        /// </summary>
        /// <returns>Mã nhân viên mới</returns>
        /// Created by: PTTAM (07/03/2023)
        public Task<string> GetNewUserCode();

        /// <summary>
        /// Thay đổi mật khẩu
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> ChangePass(User user);
    }
}
