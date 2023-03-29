using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;

namespace RoomBooking.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BookingRequestsController : BasesController<BookingRequest>
    {
        IBookingRequestService _service;
        // /// <summary>
        // /// Hàm tạo thực hiện việc giao tiếp giữa Interface và Controller
        // /// </summary>
        // /// <param name="service">Thực hiện validate dữ liệu</param>
        // /// <param name="repository">Thực hiện các chức năng</param>
        // /// Created by: PTTAM (08/03/2023)
        public BookingRequestsController(IBookingRequestService service) : base(service)
        {
            _service = service;
        }
        /// <summary>
        /// Thực hiện phân trang 
        /// </summary>
        /// <param name="pageSize">Số bản ghi/ 1 trang</param>
        /// <param name="pageIndex">Trang số bao nhiêu</param>
        /// <param name="keyWord">Điều kiện lọc dữ liệu</param>
        /// Created by: PTTAM (08/03/2023)
        [HttpPost("pagingRequestBooking")]
        public async Task<IActionResult> FilterRoom([FromBody] PagingParam param)
        {

            var res = await _service.GetPaging(param);

            return StatusCode(200, res);

        }
    }
}
