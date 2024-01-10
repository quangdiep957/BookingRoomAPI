using MySqlConnector;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.DAL.Interfaces
{
    public interface IStudentRepository : IBaseRepository<Student>
    {
        /// <summary>
        /// Thực hiện phân trang 
        /// </summary>
        /// <param name="pageSize">Số bản ghi trên 1 trang</param>
        /// <param name="pageIndex">Tổng số trang</param>
        /// <param name="keyWord">Dữ liệu chọn lọc để tìm kiếm</param>
        /// <returns>Object: chứa pageSize, pageIndex, data</returns>
        /// Created by: bqdiep (07/03/2023)
        public Task<object> GetPaging(int pageSize, int pageIndex, string? keyWord, Guid? roleId);


        /// <summary>
        /// Created by: bqdiep (07/03/2023)
        public Task<List<CheckStudentDto>> GetListCheck(CheckStudentParam param);

        /// <summary>
        /// Created by: bqdiep (07/03/2023)
        public Task<bool> CheckAttendanceApp(List<StudentCheckDto> listEntities, MySqlTransaction transaction, MySqlConnection cnn);

        /// <summary>
        /// Thực hiên lấy mã nhân viên mới của người dùng
        /// </summary>
        /// <returns>Mã nhân viên mới</returns>
        /// Created by: bqdiep (07/03/2023)
        public Task<string> GetNewStudentCode();

        /// <summary>
        /// Thay đổi mật khẩu
        /// </summary>
        /// <param name="Student"></param>
        /// <returns></returns>
        public Task<bool> ChangePass(Student Student);

        /// <summary>
        /// kiểm tra email có chưa
        /// </summary>
        /// <param name="Student"></param>
        /// <returns></returns>
        public Task<bool> CheckEmail(string Student);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<List<TimeSlot>> GetTimeSlot();
    }
}
