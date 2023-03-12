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
        public object GetPaging(int pageSize, int pageIndex, string? keyWord, Guid? roleId);



        /// <summary>
        /// Thực hiên nghiệp vụ khi lấy mã nhân viên mới của người dùng
        /// </summary>
        /// <returns>Mã nhân viên mới</returns>
        /// Created by:  PTTAM (07/03/2023)
        public string GetNewUserCode();

        /// <summary>
        /// Thực hiên nghiệp vụ khi sửa vai trò của nhân viên
        /// </summary>
        /// <param name="roleList">Mảng vai trò của nhân viên</param>
        /// <returns>Sửa thành công || Sửa thất bại</returns>
        /// Created by: PTTAM (07/03/2023)
        public string UpdateUserRole(Guid userId, List<UserRole> roleList);

        public User Authenticate(string username, string password);
    }
}
