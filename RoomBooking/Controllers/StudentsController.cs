using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RoomBooking.BLL.Interfaces;
using RoomBooking.BLL.Services;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RoomBooking.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class StudentsController : BasesController<Student>
    {
        IStudentService _service;

        // /// <summary>
        // /// Hàm tạo thực hiện việc giao tiếp giữa Interface và Controller
        // /// </summary>
        // /// <param name="service">Thực hiện validate dữ liệu</param>
        // /// <param name="repository">Thực hiện các chức năng</param>
        // /// Created by: bqdiep (08/03/2023)
        public StudentsController(IStudentService service) : base(service)
        {
            _service = service;
        }
        /// <summary>
        /// Thực hiện phân trang 
        /// </summary>
        /// <param name="pageSize">Số bản ghi/ 1 trang</param>
        /// <param name="pageIndex">Trang số bao nhiêu</param>
        /// <param name="keyWord">Điều kiện lọc dữ liệu</param>
        /// Created by: bqdiep (08/03/2023)
        [HttpPost("pagingStudent")]
        public async Task<IActionResult> FilterStudent([FromBody]PagingParam param)
        {

            var res =await _service.GetPaging((int)param.pageSize, (int)param.pageIndex, param.keyWord,param.roleID);

            return StatusCode(200, res);

        }
        /// <summary>
        /// Thực hiện xác thực và cập nhật điểm danh
        /// </summary>
        /// Created by: bqdiep (14/12/2023)
        [HttpPost("CheckFaceID")]
        public async Task<IActionResult> CheckFaceID( string studentCode)
        {

            var res = await _service.CheckFaceID(studentCode);

            return StatusCode(200, res);

        }

        /// <summary>
        /// Lấy danh sách cần điểm danh
        /// </summary>
        /// Created by: bqdiep (14/12/2023)
        [HttpPost("GetListCheck")]
        public async Task<IActionResult> GetListCheck(CheckStudentParam param)
        {

            var res = await _service.GetListCheck(param);

            return StatusCode(200, res);

        }

        /// <summary>
        /// Điểm danh bằng phần mềm
        /// </summary>
        /// Created by: bqdiep (14/12/2023)
        [HttpPost("CheckAttendanceApp")]
        public async Task<IActionResult> CheckAttendanceApp([FromBody] ParamStudent param)
        {

            var res = await _service.CheckAttendanceApp(param);

            return StatusCode(200, res);

        }


    }
}
