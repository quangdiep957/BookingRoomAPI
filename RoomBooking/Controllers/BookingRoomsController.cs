using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.BLL.Interfaces;
using RoomBooking.BLL.Services;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using RoomBooking.Common.Enum;
using static Dapper.SqlMapper;

namespace RoomBooking.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BookingRoomsController : ControllerBase
    {
        private readonly IBookingRoomService _scheduleService;
        public BookingRoomsController(IBookingRoomService scheduleService)
        {
            _scheduleService = scheduleService;
            
        }
        [HttpPost("excel")]
        public async Task<IActionResult> ReadScheduleFile(IFormFile file)
        {
            try
            {
                var filePath = Path.GetTempFileName();

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var scheduleItems = await _scheduleService.ReadExcelFile(filePath);

                return Ok(scheduleItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Thực hiện phân trang 
        /// </summary>
        /// <param name="pageSize">Số bản ghi/ 1 trang</param>
        /// <param name="pageIndex">Trang số bao nhiêu</param>
        /// <param name="keyWord">Điều kiện lọc dữ liệu</param>
        /// Created by: PTTAM (08/03/2023)
        [HttpPost("pagingScheduler")]
        public async Task<IActionResult> FilterRoom([FromBody] PagingParam param)
        {

            var res = await _scheduleService.GetPaging(param);

            return StatusCode(200, res);

        }

        /// <summary>
        /// Thực hiện duyệt phòng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost("requestBookingRoom")]
        public async Task<IActionResult> RequestBookingRoom(BookingRoomParam param)
        {
            var res = await _scheduleService.RequestBookingRoom(param);
            return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);
        }

        /// <summary>
        /// Thực hiện phân trang 
        /// </summary>
        /// <param name="pageSize">Số bản ghi/ 1 trang</param>
        /// <param name="pageIndex">Trang số bao nhiêu</param>
        /// <param name="keyWord">Điều kiện lọc dữ liệu</param>
        /// Created by: PTTAM (08/03/2023)
        [HttpPost("pagingRequestBooking")]
        public async Task<IActionResult> RequestBooking([FromBody] PagingParam param)
        {

            var res = await _scheduleService.GetPagingRequest(param);

            return StatusCode(200, res);

        }
        /// <summary>
        /// Thực hiện gửi yêu cầu đặt phòng
        /// </summary>
        /// <param name="bookingRoom"></param>
        /// <returns></returns>
        [HttpPost("insertBookingRequest")]
        public async Task<IActionResult> InsertBookingRequest( BookingRoomParam param )
        {
            var res = await _scheduleService.InsertBookingRequest(param.booking,param.userID);
            return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);
        }
    }
}
