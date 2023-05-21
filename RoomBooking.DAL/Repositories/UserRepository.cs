using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Enum;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace RoomBooking.DAL.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(IConfiguration configuration) : base(configuration)
        {


        }
        /// <summary>
        /// Thực hiện việc phân trang
        /// </summary>
        /// <param name="pageSize">Số bản ghi/ 1 trang</param>
        /// <param name="pageIndex">Trang số bao nhiêu</param>
        /// <param name="keyWord">Điều kiện lọc dữ liệu/param>
        /// <param name="roleId">Khóa chính của vai trò /param>
        /// <returns>Object chứa những thông tin cần thiết</returns>
        /// Created by: PTTAM (07/03/2023)
        public async Task<Object> GetPaging(int pageSize, int pageIndex, string? keyWord, Guid? roleId)
        {

            var storeName = "Proc_GetUserPaging"; // Tên của thủ thục
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@PageSize", pageSize); //input: Số bản ghi/trang
            dynamicParameters.Add("@PageIndex", pageIndex);//input: Trang hiện tại
            dynamicParameters.Add("@UserFilter", keyWord); //input: Từ khóa
            dynamicParameters.Add("@RoleId", roleId); //input: Khóa chính vai trò
            dynamicParameters.Add("@TotalRecord", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số bản ghi
            dynamicParameters.Add("@TotalPage", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số trang
            //2. Lấy dữ liệu
            var employees = await _sqlConnection.QueryAsync<User>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);

            int totalRecord = dynamicParameters.Get<int>("@TotalRecord"); // Lấy ra tổng số bản ghi
            int totalPage = dynamicParameters.Get<int>("@TotalPage"); // Lấy ra tổng số trang
            int startRecord = pageSize * (pageIndex - 1) + 1; // Bản ghi bắt đầu của trang hiện tại
            int endRecord = pageSize * (pageIndex - 1) + pageSize; // Bản ghi kết thúc của trang hiện tại

            if (endRecord > totalRecord) // nếu bản ghi kết thúc > tổng số bản ghi
            {
                endRecord = totalRecord; // gán bản ghi kết thúc = tổng số bản ghi
            }

            // nếu bản ghi bắt đầu của trang > bản ghi kết thúc
            if (startRecord > endRecord)
            {
                startRecord = endRecord;// gán bản ghi bắt đầu = bản ghi kết thúc
            }
            return new
            {
                TotalPage = totalPage,
                TotalRecord = totalRecord,
                CurrentPage = pageIndex,
                StartRecord = startRecord,
                EndRecord = endRecord,
                Data = employees
            };
        }



        /// <summary>
        /// Thực hiện lấy mã người dùng mới
        /// </summary>
        /// <returns>Mã người dùng mới</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public async Task<string> GetNewUserCode()
        {
            var storeName = "Proc_GetNewCode";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@UserNewCode", DbType.String, direction: ParameterDirection.Output);
            await _sqlConnection.ExecuteAsync(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);
            string employeeNewCode = dynamicParameters.Get<String>("@UserNewCode");
            return employeeNewCode;
        }

        public async Task<bool> ChangePass(User user)
        {
            var query = "UPDATE user u SET u.Password = @PassWordNew WHERE u.UserID = @UserID";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@UserID", user.UserID);
            dynamicParameters.Add("@PasswordNew", user.PasswordNew);
            var a = await _sqlConnection.ExecuteAsync(query, dynamicParameters);
            if (a > 0) { return true; }
            else
            {
                return false;
            }

        }
        /// <summary>
        /// Overide hàm cập nhật người dùng
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityId"></param>
        /// <param name="cnn"></param>
        /// <param name="transaction"></param>
        /// PTTAM 04.05.2023
        public override async Task<bool> Update(User entity, Guid entityId, MySqlConnection cnn, MySqlTransaction transaction)
        {
            var isSuccess = true;
            var sql = "Update User SET UserCode = @UserCode," +
                "FullName = @FullName," +
                "Address = @Address," +
                "Email = @Email,DepartmentID = @DepartmentID,RoleID = @RoleID,PhoneNumber = @PhoneNumber WHERE UserID = @UserID;";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add($"@UserCode", entity.UserCode);
            dynamicParameters.Add($"@FullName", entity.FullName);
            dynamicParameters.Add($"@Address", entity.Address);
            dynamicParameters.Add($"@Email", entity.Email);
            dynamicParameters.Add($"@DepartmentID", entity.DepartmentID);
            dynamicParameters.Add($"@PhoneNumber", entity.PhoneNumber);
            dynamicParameters.Add($"@RoleID", entity.RoleID);
            dynamicParameters.Add($"@UserID", entityId);
            var  res= await cnn.ExecuteAsync(sql, dynamicParameters,transaction, commandType: System.Data.CommandType.Text);
            if (res==0)
            {
                isSuccess = false;
            }
            return isSuccess;
        }

        public override async Task<IEnumerable<User>> GetAll()
        {
            if (_sqlConnection.State != ConnectionState.Open)
            {
                _sqlConnection.Open();
            }
            var sql = "SELECT * FROM User";
            var res = await _sqlConnection.QueryAsync<User>(sql);
            return res;

        }


    }
}
