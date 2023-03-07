using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Enum;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Created by: PTTAM (09/09/2022)
        public Object GetPaging(int pageSize, int pageIndex, string? keyWord, Guid? roleId)
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
            var employees = _sqlConnection.Query<User>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);

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
        /// Thực hiện việc thêm nhiều người dùng
        /// </summary>
        /// <param name="listUsers">Danh sách người dùng</param>
        /// <returns></returns>
        ///  CretedBy: PTTAM (15/09/2022)
        public override string InsertMulti(List<User> listUsers)
        {
            // Mở connection
            //if (_sqlConnection.State == System.Data.ConnectionState.Closed)
            //{
            //    _sqlConnection.Open();
            //}
            base._sqlConnection.Open();
            //1. Thực hiện insert user
            List<User_Role> listUserRole = new List<User_Role>(); // Danh sách vai trò của người dùng
            int countUser = 0; // biến đếm khi thực hiện thêm người dùng
            int countUserRole = 0; // biến đếm khi thực hiện thêm vai trò của người dùng
            string sqlQuery = GetAllBindingNames(listUsers[0]); // câu truy vấn lấy ra tên trường của user
            DynamicParameters dynamicParameters = new DynamicParameters();
            MySqlTransaction transaction = _sqlConnection.BeginTransaction();
            for (int i = 0; i < listUsers.Count; i++)
            {
                listUserRole.AddRange(listUsers[i].UserRoles); // thêm vai trò của người dùng vào danh sách vai trò 
                sqlQuery += GetAllBindingValues(listUsers[i], i, dynamicParameters); // Thực hiện buid câu truy vấn 
                countUser++;
            }
            sqlQuery = sqlQuery[..^1]; // bỏ dấu ',' cuối cùng
            var rowEffect = _sqlConnection.Execute(sqlQuery, dynamicParameters, transaction: transaction);

            // nếu số bản ghi thay đổi < countUser
            if (rowEffect < countUser)
            {
                transaction.Rollback(); // rollback 
                return ""; // thêm thất bại
            }
            else // ngược lại
            {   // thực hiện thêm mới vai trò
                int res = InsertUserRole(listUserRole, ref countUserRole, transaction);
                if (res < countUserRole)
                {
                    transaction.Rollback();
                    return "";
                }
            }
            transaction.Commit();
            CloseConnection();
            return "";
        }

        /// <summary>
        /// Thực hiện thêm mới vai trò của người dùng
        /// </summary>
        /// <param name="listUserRole">Mảng danh sách vai trò của người dùng</param>
        /// <param name="countUserRole"> biến đếm khi thực hiện thêm vai trò của người dùng</param>
        /// <param name="transaction">Transaction</param>
        ///  CretedBy: PTTAM (15/09/2022)
        private int InsertUserRole(List<User_Role> listUserRole, ref int countUserRole, MySqlTransaction transaction)
        {
            DynamicParameters parametersInsertUserRole = new DynamicParameters();
            var sqlInsertUserRole = "INSERT INTO UserRole(UserId,RoleId) Values";
            for (int i = 0; i < listUserRole.Count; i++)
            {
                sqlInsertUserRole += "(@UserId" + i + ", @RoleId" + i + " ),";
                parametersInsertUserRole.Add("@UserId" + i, listUserRole[i].UserID);
                parametersInsertUserRole.Add("@RoleId" + i, listUserRole[i].RoleID);
                countUserRole++;
            }

            sqlInsertUserRole = sqlInsertUserRole[..^1];
            var res = _sqlConnection.Execute(sqlInsertUserRole, parametersInsertUserRole, transaction: transaction);
            return res;
        }

        /// <summary>
        /// Thực hiện cập nhật vai trò của người dùng
        /// </summary>
        /// <param name="userId">Khóa chính người dùng</param>
        /// <param name="roleList">Danh sách vai trò của người dùng</param>
        /// CretedBy: PTTAM (15/09/2022)
        public string UpdateUserRole(Guid userId, List<User_Role> roleList)
        {
            //Mở connection
            if (_sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                _sqlConnection.Open();
            }

            MySqlTransaction transaction = _sqlConnection.BeginTransaction();
            var sqlUpdateUserRole = "UPDATE User set RoleNames = @RoleNames WHERE UserId = @UserId";
            DynamicParameters paramUpdate = new DynamicParameters();
            string roleName = "";
            var listInsert = roleList.FindAll(o => o.State == UpdateMode.InsertMode); // Danh sách vai trò có state = thêm mới
            var listDelete = roleList.FindAll(o => o.State == UpdateMode.DeleteMode); // Danh sách vai trò có state = delete
            var listUnChange = roleList.FindAll(o => o.State == UpdateMode.Unchanged); // Danh sách vai trò có state = unchange
            int countInsert = 0;
            int countUnchange = 0;
            int countDelete = 0;
            // Nếu danh sách vai trò có state = insert mà không trống
            if (listInsert.Count > 0)
            {
                // Thực hiện thêm vai trò
                int rowEffect = InsertRole(userId, transaction, ref roleName, listInsert, ref countInsert);
                if (rowEffect < countInsert)
                {
                    transaction.Rollback();
                    return "";
                }
            }
            // Nếu danh sách vai trò có state = delete mà không trống
            if (listDelete.Count > 0)
            {
                // Thực hiên xóa vai trò
                int rowEffect = DeleteRole(userId, transaction, listDelete, ref countDelete);
                if (rowEffect < countDelete)
                {
                    transaction.Rollback();
                    return "";

                }
            }
            // thực hiện cập nhật lại trường dư thừa RoleNames
            int res = UpdateRoleName(userId, transaction, sqlUpdateUserRole, paramUpdate, ref roleName, listUnChange, ref countUnchange);
            if (res != 1)
            {
                transaction.Rollback();
                return "";

            }
            transaction.Commit();
            return "";

        }

        /// <summary>
        /// Cập nhật lại trường dư thừa RoleNames
        /// </summary>
        /// <param name="userId">Khóa chính người dùng</param>
        /// <param name="transaction">Transaction</param>
        /// <param name="sqlUpdateUserRole">Câu truy vấn thực hiện update</param>
        /// <param name="paramUpdate">parameter update</param>
        /// <param name="roleName">Tên vai trò</param>
        /// <param name="listUnChange">Danh sách vai trò không thay đổi</param>
        /// <param name="countUnchange">Biến đếm khi cập nhật</param>
        ///  CretedBy: PTTAM (15/09/2022)
        private int UpdateRoleName(Guid userId, MySqlTransaction transaction, string sqlUpdateUserRole, DynamicParameters paramUpdate, ref string roleName, List<User_Role> listUnChange, ref int countUnchange)
        {
            // Nếu danh sách vai trò có state = unchange mà không trống
            if (listUnChange.Count > 0)
            {
                foreach (var item in listUnChange)
                {
                    roleName += item.RoleName.Trim() + "; ";
                    countUnchange++;
                }
            }

            if (roleName != "")
            {
                roleName = roleName[..^2];

            }
            paramUpdate.Add("@UserId", userId);
            paramUpdate.Add("@RoleNames", roleName);
            var res = _sqlConnection.Execute(sqlUpdateUserRole, paramUpdate, transaction: transaction);
            return res;
        }
        /// <summary>
        /// Thực hiên xóa vai trò
        /// </summary>
        /// <param name="userId">Khóa chính người dùng</param>
        /// <param name="transaction">transaction</param>
        /// <param name="listDelete">Danh sách cần xóa</param>
        /// <param name="countDelete">Biến đếm khi xóa</param>
        ///  CretedBy: PTTAM (15/09/2022)
        private int DeleteRole(Guid userId, MySqlTransaction transaction, List<User_Role> listDelete, ref int countDelete)
        {
            var sqlDeleteUserRole = "DELETE FROM UserRole WHERE RoleId IN (";
            DynamicParameters paramsDelete = new DynamicParameters();

            for (int i = 0; i < listDelete.Count; i++)
            {
                sqlDeleteUserRole += "@RoleId" + i + ",";
                paramsDelete.Add("@RoleId" + i, listDelete[i].RoleID);
                countDelete++;
            }
            sqlDeleteUserRole = sqlDeleteUserRole[..^1];
            paramsDelete.Add("@UserId", userId);
            sqlDeleteUserRole += " )" + " AND UserId = @UserId";
            var rowEffect = _sqlConnection.Execute(sqlDeleteUserRole, paramsDelete, transaction: transaction);
            return rowEffect;
        }

        /// <summary>
        /// Thêm vai trò của người dùng
        /// </summary>
        /// <param name="userId">Khóa chính người dùng</param>
        /// <param name="transaction">Transaction</param>
        /// <param name="roleName">Tên vai trò</param>
        /// <param name="listInsert">Danh sách cần thêm</param>
        /// <param name="countInsert">Biến đếm khi thêm</param>
        /// CretedBy: PTTAM (15/09/2022)
        private int InsertRole(Guid userId, MySqlTransaction transaction, ref string roleName, List<User_Role> listInsert, ref int countInsert)
        {
            var sqlInsertUserRole = "INSERT INTO UserRole(UserId,RoleId) Values";
            DynamicParameters paramInsert = new DynamicParameters();
            paramInsert.Add("@UserId", userId);
            for (int i = 0; i < listInsert.Count; i++)
            {
                sqlInsertUserRole += "( @UserId" + " , @RoleId" + i + " ),";
                paramInsert.Add("@RoleId" + i, listInsert[i].RoleID);
                roleName += listInsert[i].RoleName.Trim() + "; ";
                countInsert++;
            }
            sqlInsertUserRole = sqlInsertUserRole[..^1];
            var rowEffect = _sqlConnection.Execute(sqlInsertUserRole, paramInsert, transaction: transaction);
            return rowEffect;
        }

        /// <summary>
        /// Thực hiện lấy mã người dùng mới
        /// </summary>
        /// <returns>Mã người dùng mới</returns>
        ///  CretedBy: PTTAM (15/09/2022)
        public string GetNewUserCode()
        {
            var storeName = "Proc_GetNewCode";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@UserNewCode", DbType.String, direction: ParameterDirection.Output);
            _sqlConnection.Execute(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);
            string employeeNewCode = dynamicParameters.Get<String>("@UserNewCode");
            return employeeNewCode;
        }


        /// <summary>
        /// Thực hiện lấy thông tin của người dùng theo khóa chính
        /// </summary>
        /// <param name="userId">Khóa chính người dùng</param>
        /// <returns>THông tin người dùng</returns>
        ///  CretedBy: PTTAM (15/09/2022)
        public override User GetById(Guid userId)
        {

            var storeName = "Proc_GetUserById";
            DynamicParameters dynamic = new DynamicParameters();
            dynamic.Add("@UserId", userId);
            var users = _sqlConnection.QueryMultiple(storeName, param: dynamic, commandType: System.Data.CommandType.StoredProcedure);

            User user = users.Read<User>().First();
            user.UserRoles = users.Read<User_Role>().ToList();

            return user;
        }

       
    }
}
