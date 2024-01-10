using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Interfaces
{
    public interface IStudentService : IBaseService<Student>
    {
        /// <summary>
        /// Thực hiện nghiệp vụ khi phân trang 
        /// </summary>
        /// <param name="pageSize">Số bản ghi trên 1 trang</param>
        /// <param name="pageIndex">Tổng số trang</param>
        /// <param name="keyWord">Dữ liệu chọn lọc để tìm kiếm</param>
        /// <returns>Object: chứa pageSize, pageIndex, data</returns>
        /// Created by: bqdiep (07/03/2023)
        public Task<object> GetPaging(int pageSize, int pageIndex, string? keyWord, Guid? roleId);

        /// <summary>
        /// Thực hiện Checkface
        /// Created by: bqdiep (14/12/2023)
        public Task<Student> CheckFaceID(string StudentCode);

        /// <summary>
        /// Thực hiện Checkface
        /// Created by: bqdiep (14/12/2023)
        public Task<object> GetListCheck(CheckStudentParam param);

        /// <summary>
        /// Thực hiện điểm danh bằng app
        /// Created by: bqdiep (14/12/2023)
        public Task<object> CheckAttendanceApp(ParamStudent param);

    }
}
