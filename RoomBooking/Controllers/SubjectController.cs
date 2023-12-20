using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using RoomBooking.Common.Enum;

namespace RoomBooking.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SubjectController : BasesController<Subject>
    {
        ISubjectService _service;
        // /// <summary>
        // /// Hàm tạo thực hiện việc giao tiếp giữa Interface và Controller
        // /// </summary>
        // /// <param name="service">Thực hiện validate dữ liệu</param>
        // /// <param name="repository">Thực hiện các chức năng</param>
        // /// Created by: bqdiep (08/03/2023)
        public SubjectController(ISubjectService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// Thực hiện lấy danh sach lớp đã đăng ký
        /// </summary>
        /// <param name="BookingID"></param>
        /// <returns></returns>
        [HttpPost("GetClassActive")]
        public async Task<IActionResult> GetClassActive(ClassParam param)
        {
            var res = await _service.GetClassActive(param);

            return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);

        }
    }
}
