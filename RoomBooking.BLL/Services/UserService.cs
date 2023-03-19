using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Functions;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Services
{
    public class UserService : BaseService<User>, IUserService
    {
        IUserRepository _repository;

        public UserService(IUserRepository repository) : base(repository)
        {
            _repository = repository;
        }
        private List<User> _users = new List<User>
    {
        new User { UserCode="admin", FullName = "admin", Password = "password", Email = "admin@example.com" }
    };
        /// <summary>
        /// Thực hiện nghiệp vụ khi lấy mã nhân viên mới
        /// </summary>
        /// <returns></returns>
        ///  Created by: PTTAM(10/9/2022)
        public async Task<string> GetNewUserCode()
        {
            var res = await _repository.GetNewUserCode();
            return res;
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
        public async Task<object> GetPaging(int pageSize, int pageIndex, string? keyWord, Guid? roleId)
        {
            var res = await _repository.GetPaging(pageSize, pageIndex, keyWord, roleId);
            return res;
        }

        /// <summary>
        /// Thực hiện nghiệp vụ khi sửa vai trò của người dùng
        /// </summary>
        /// <param name="userId">Khóa chính người dùng</param>
        /// <param name="roleList">Mảng vai trò</param>
        /// <returns>Sửa thành công || Sửa thất bại</returns>
        ///  Created by: PTTAM(10/9/2022)
        public async Task<string> UpdateUserRole(Guid userId, List<UserRole> roleList)
        {
            var res = await _repository.UpdateUserRole(userId, roleList);
            return res;
        }

        /// <summary>
        /// Validate email, ngày sinh, ngày cấp của nhân viên
        /// </summary>
        /// <param name="entity">đối tượng nhân viên</param>
        /// <returns>true: nếu không có lỗi, false: có lỗi</returns>
        /// Created by: PTTAM(10/9/2022)
        protected override bool ValidateCustom(User user)
        {
            
            if (!CommonFunction.IsValidEmail(user.Email))
            {
                isValidCustom = false;


                object error = new
                {
                    errorTitle ="",
                    errorName = ""

                };
                errorList.Add(error);
            }

            return isValidCustom;
        }
        public User Authenticate(string username, string password)
        {
            var user = _users.SingleOrDefault(x => x.UserCode == username && x.Password == password);

            if (user == null)
                return null;

            // remove password before returning
            user.Password = null;

            return user;
        }

      
    }
}
