using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using RoomBooking.Common.AttributeCustom;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RoomBooking.DAL.Repositories
{
    public class StudentRepository : BaseRepository<Student>, IStudentRepository
    {
        
        public StudentRepository(IConfiguration configuration) : base(configuration)
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
        /// Created by: bqdiep (07/03/2023)
        public async Task<Object> GetPaging(int pageSize, int pageIndex, string? keyWord, Guid? roleId)
        {

            var storeName = "Proc_GetStudentPaging"; // Tên của thủ thục
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@PageSize", pageSize); //input: Số bản ghi/trang
            dynamicParameters.Add("@PageIndex", pageIndex);//input: Trang hiện tại
            dynamicParameters.Add("@StudentFilter", keyWord); //input: Từ khóa
            dynamicParameters.Add("@RoleId", roleId); //input: Khóa chính vai trò
            dynamicParameters.Add("@TotalRecord", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số bản ghi
            dynamicParameters.Add("@TotalPage", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số trang
            //2. Lấy dữ liệu
            var employees = await _sqlConnection.QueryAsync<Student>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);

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
        ///  CretedBy: bqdiep (07/03/2023)
        public async Task<string> GetNewStudentCode()
        {
            var storeName = "Proc_GetNewCode";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@StudentNewCode", DbType.String, direction: ParameterDirection.Output);
            await _sqlConnection.ExecuteAsync(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);
            string employeeNewCode = dynamicParameters.Get<String>("@StudentNewCode");
            return employeeNewCode;
        }
        /// <summary>
        /// Kiểm tra email google có trong db chưa
        /// </summary>
        /// <returns>Mã người dùng mới</returns>
        ///  CretedBy: bqdiep (07/03/2023)
        public async Task<bool> CheckEmail(string email)
        {
            var query = "select count(*) from Student where email = @Email";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@Email", email);
           int count = await _sqlConnection.ExecuteScalarAsync<int>(query, param: dynamicParameters);

            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> ChangePass(Student Student)
        {
           
            var query = "UPDATE Student u SET u.Password = @PassWordNew WHERE u.StudentID = @StudentID";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@StudentID", Student.StudentID);
            dynamicParameters.Add("@PasswordNew", Student.PasswordNew);
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
        /// bqdiep 04.05.2023
        public override async Task<bool> Update(Student entity, Guid entityId, MySqlConnection cnn, MySqlTransaction transaction)
        {
            var isSuccess = true;
            var sql = "Update Student SET StudentCode = @StudentCode," +
                "FullName = @FullName," +
                "Address = @Address," +
                "Email = @Email,DepartmentID = @DepartmentID,ClassID = @ClassID,PhoneNumber = @PhoneNumber WHERE StudentID = @StudentID;";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add($"@StudentCode", entity.StudentCode);
            dynamicParameters.Add($"@FullName", entity.FullName);
            dynamicParameters.Add($"@Address", entity.Address);
            dynamicParameters.Add($"@Email", entity.Email);
            dynamicParameters.Add($"@DepartmentID", entity.DepartmentID);
            dynamicParameters.Add($"@PhoneNumber", entity.PhoneNumber);
            dynamicParameters.Add($"@ClassID", entity.ClassID);
            dynamicParameters.Add($"@StudentID", entityId);
            var  res= await cnn.ExecuteAsync(sql, dynamicParameters,transaction, commandType: System.Data.CommandType.Text);
            if (res==0)
            {
                isSuccess = false;
            }
            return isSuccess;
        }

        public override async Task<IEnumerable<Student>> GetAll()
        {
            if (_sqlConnection.State != ConnectionState.Open)
            {
                _sqlConnection.Open();
            }
            var sql = "SELECT * FROM Student";
            var res = await _sqlConnection.QueryAsync<Student>(sql);
            return res;

        }

        public async Task<List<CheckStudentDto>> GetListCheck(CheckStudentParam param)
        {
            var storeName = "Proc_Attendance";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@ClassID", param.ClassID);
            dynamicParameters.Add("@BookingRoomID", param.BookingRoomID);
            dynamicParameters.Add("@DateTime", param.DateRequest);
            dynamicParameters.Add("@SubjectID", param.SubjectID);
            dynamicParameters.Add("@UserID", param.UserID);
            List<CheckStudentDto> res = (await _sqlConnection.QueryAsync<CheckStudentDto>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure)).ToList();
            return res;
        }

        public async Task<bool> CheckAttendanceApp(List<StudentCheckDto> listEntities, MySqlTransaction transaction, MySqlConnection cnn)
        {
            // build script cập nhật
            var sql = "INSERT INTO checkstudent ( DateRequest,SubjectID, ClassID,BookingRoomID,StudentCode, Status, StudentID) VALUES";

            DynamicParameters dynamicParameters = new DynamicParameters();
            for (int i = 0; i < listEntities.Count; i++)
            {
                sql += GetAllBindingValuesCustom(listEntities[i], i, dynamicParameters);
            }
            sql = sql[..^1];
            var rowEffect = await cnn.ExecuteAsync(sql, dynamicParameters, transaction: transaction);
            if (rowEffect < listEntities.Count)
            {
                return false;
            }
            return true;
        }

        public async Task<List<TimeSlot>> GetTimeSlot()
        {
            var res = new List<TimeSlot>();
            res  = (List<TimeSlot>)await _sqlConnection.QueryAsync<TimeSlot>("Select * FROM TimeSlot");
            return res;
        }
       
        /// <summary>
        /// Thực hiện lấy giá trị của đối tượng và add vào parameter
        /// </summary>
        /// <param name="entity">Đối tượng</param>
        /// <param name="index">Vị trí của đối tượng</param>
        /// <param name="parameters">Parameter</param>
        /// <returns>Chuỗi paramerter</returns>
        ///  CretedBy: bqdiep (07/03/2023)
        private static string GetAllBindingValuesCustom(StudentCheckDto entity, int index, DynamicParameters parameters)
        {
            // lấy tất cả cá properties

            var properties = typeof(StudentCheckDto).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ForBinding)));
            // Buid 
            var allValuesParam = "( ";
            foreach (var property in properties)
            {
                // lấy giá trị
                allValuesParam += "@" + property.Name + index + " ,";


            }
            allValuesParam = allValuesParam[..^1];
            allValuesParam += " ),";
            foreach (var property in properties)
            {

                // lấy giá trị
                var currentValue = property.GetValue(entity);
                parameters.Add($"@{property.Name}" + index, currentValue);


            }

            // bỏ kí tự ',' cuối cùng
            return allValuesParam;

        }

    }
}
