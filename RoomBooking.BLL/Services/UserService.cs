using MySqlConnector;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Enum;
using RoomBooking.Common.Exception;
using RoomBooking.Common.Functions;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSystem.Security.Cryptography;
using static Dapper.SqlMapper;

namespace RoomBooking.BLL.Services
{
    public class UserService : BaseService<User>, IUserService
    {
        IUserRepository _repository;
        IRoleRepository _roleRepo;
        public UserService(IUserRepository repository,IRoleRepository roleRepository) : base(repository)
        {
            _repository = repository;
            _roleRepo = roleRepository;
        }
      
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
        /// Thực hiện nghiệp vụ khi thêm mới người dùng
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Thêm mới thành công || Thêm mới thất bại</returns>
        /// <exception cref="ValidateException"></exception>
        /// Created by: PTTAM (07/03/2023)
        public async override Task<bool> InsertService(User entity)
        {

            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                // Gọi đến hàm validate dữ liệu

                using (MySqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        ValidateError(entity, cnn, tran);
                        
                        // kiểm tra biến isValidCustom và listErrors thỏa mãn điều kiện thì gọi repository để thực hiện việc thêm mới
                        if (isValidCustom == true && errorList.Count <= 0)
                        {
                            entity.Password=HashPassword(entity.Password);
                            var res = await _repository.Insert(entity, cnn, tran);
                            if (res == true)
                            {
                                tran.Commit();

                            }
                            else { tran.Rollback(); }
                            return res;
                        }
                        else // Ngược lại throw ra lỗi
                        {

                            throw new ValidateException(errors);
                        }
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }


                }
            }

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

        /// <summary>
        /// Kiểm tra đăng nhập
        /// </summary>
        /// <param name="username">tên đăng nhập</param>
        /// <param name="password">mật khẩu</param>
        /// Created by: PTTAM
        public async Task<User> Authenticate(string username, string password)
        {
            var users = await _repository.GetAll();
            var user = users.FirstOrDefault(x=>x.Email==username);
           
            if (user == null) { return null; }
            bool checkPassword=VerifyPassword(password,user.Password);
            if (checkPassword)
            {
                var roles = await _roleRepo.GetAll();
                var role = roles.FirstOrDefault(x => x.RoleID == user.RoleID);

                user.RoleOption = role.RoleValue;
                // remove password before returning
                user.Password = null;
            }
            else
            {
                return null;
            }
         

            return user;
        }
       /// <summary>
       /// Mã hóa mật khẩu
       /// </summary>
       /// <param name="password"></param>
       /// <returns></returns>
        private string HashPassword(string password)
        {
            // Chuyển đổi mật khẩu sang chuỗi byte
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Tạo đối tượng SHA256Managed để tạo mã băm
            SHA256Managed sha256 = new SHA256Managed();

            // Tạo mã băm của mật khẩu
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);

            // Chuyển đổi mã băm thành chuỗi hex
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Giải mã
        /// </summary>
        /// <param name="password"></param>
        /// <param name="hashedPassword"></param>
        /// <returns></returns>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            // Chuyển đổi mật khẩu sang chuỗi byte
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Tạo đối tượng SHA256Managed để tạo mã băm
            SHA256Managed sha256 = new SHA256Managed();

            // Tạo mã băm của mật khẩu
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);

            // Chuyển đổi mã băm thành chuỗi hex
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }
            string hashedPasswordToCheck = builder.ToString();

            // So sánh hai mã băm
            return hashedPasswordToCheck == hashedPassword;
        }

    }
}
