using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RoomBooking.BLL.Interfaces;
using RoomBooking.BLL.Services;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using RoomBooking.Common.Enum;
using RoomBooking.Common.Resources;
using System.Text;
using System.Text.Json;
using static Dapper.SqlMapper;

namespace RoomBooking.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BookingRoomsController : BasesController<BookingRoom>
    {
        private readonly IBookingRoomService _scheduleService;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public BookingRoomsController(IBookingRoomService scheduleService,IEmailService emailService, IMemoryCache cache) : base(scheduleService)
        {
            _scheduleService = scheduleService;
            _emailService = emailService;
            _cache = cache;


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
            if (res != null)
            {
                // Nếu lưu thành công thì sẽ lấy userID đang đăng nhập 
                //var user = _cache.Get<User>("userCache").UserID.ToString();
                if (param.userID != null)
                    await _scheduleService.SendNotify(param.userID.ToString(), Resource.SendPending, DateTime.Now, false);
                    // gửi cho người mở cửa
                    await _scheduleService.SendNotify(res.SupporterID.ToString(), Resource.SendPending, DateTime.Now, false);
            }
            return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);
        }
        /// <summary>
        /// Thực hiện gửi email phê duyệt/ từ chối
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost("sendingEmailAproveOrReject")]
        public async Task<IActionResult> SendingEmailAproveOrReject(BookingRoomParam param)
        {
            var res = await _scheduleService.SendingEmailAproveOrReject(param);
           
            return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);
        }
        /// <summary>
        /// Thực hiện phân trang chờ duyệt
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
        /// Thực hiện phân trang lịch sử đặt phòng
        /// </summary>
        /// <param name="pageSize">Số bản ghi/ 1 trang</param>
        /// <param name="pageIndex">Trang số bao nhiêu</param>
        /// <param name="keyWord">Điều kiện lọc dữ liệu</param>
        /// Created by: PTTAM (08/03/2023)
        [HttpPost("pagingHistoryBooking")]
        public async Task<IActionResult> HistoryBooking([FromBody] PagingParam param)
        {

            var res = await _scheduleService.GetPagingHistory(param);

            return StatusCode(200, res);

        }
        /// <summary>
        /// Thực hiện gửi yêu cầu đặt phòng
        /// </summary>
        /// <param name="bookingRoom"></param>
        /// <returns></returns>
        [HttpPost("insertBookingRequest")]
        public async Task<IActionResult> InsertBookingRequest(BookingRoom param)
        {
            var res = await _scheduleService.InsertBookingRequest(param, param.UserID);
            if(res != null)
            {
                // Nếu lưu thành công thì sẽ lấy userID đang đăng nhập 
               var user = _cache.Get<User>("userCache").UserID.ToString();
                if(user != null)
                 // gửi cho chính nó
                 await _scheduleService.SendNotify(user, Resource.Pending, DateTime.Now,false);
                 await _scheduleService.SendNotify(param.AdminID.ToString(), Resource.Pending, DateTime.Now, true);
            }    
            return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);
        }
        /// <summary>
        /// Gửi email chờ duyệt
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost("sendEmailPending")]
        public async Task<IActionResult> SendEmailPending(BookingRoom param)
        {
            var res = await _scheduleService.SendingEmailPending(param, param.UserID);
            return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);

        }
        /// <summary>
        /// Thực hiện sửa yêu cầu đặt phòng
        /// </summary>
        /// <param name="bookingRoom"></param>
        /// <returns></returns>
        [HttpPut("updateBookingRequest/{BookingID}")]
        public async Task<IActionResult> UpdateBookingRequest(Guid BookingID,BookingRoom bookingRoom)
        {
            try
            {
                var res = await _scheduleService.UpdateBookingRequest(BookingID, bookingRoom);
                if (res != null)
                {
                    // Nếu lưu thành công thì sẽ lấy userID đang đăng nhập 
                    var cache = _cache.Get<User>("userCache");
                    if(cache!= null)
                    {
                        var user = _cache.Get<User>("userCache").UserID.ToString();
                        if (user != null)
                            await _scheduleService.SendNotify(user, Resource.Pending, DateTime.Now, false);
                            await _scheduleService.SendNotify(bookingRoom.AdminID.ToString(), Resource.Pending, DateTime.Now, true);
                    }    
                }
                return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Thực hiện sửa yêu cầu đặt phòng
        /// </summary>
        /// <param name="bookingRoom"></param>
        /// <returns></returns>
        [HttpPut("sendingEmailUpdate/{BookingID}")]
        public async Task<IActionResult> SendingEmailUpdate(Guid BookingID, BookingRoom bookingRoom)
        {
            try
            {
                var res = await _scheduleService.SendingEmailUpdate(BookingID, bookingRoom);
                return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Thực hiện hủy yêu cầu đặt phòng
        /// </summary>
        /// <param name="bookingRoom"></param>
        /// <returns></returns>
        [HttpGet("cancelBookingRequest")]
        public async Task<IActionResult> CancelBookingRequest(Guid BookingID)
        {
            var res = await _scheduleService.CancelBookingRoom(BookingID);
            return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);
        }
        /// <summary>
        /// Thực hiện hủy yêu cầu đặt phòng ko tính phút
        /// </summary>
        /// <param name="bookingRoom"></param>
        /// <returns></returns>
        [HttpPut("cancelBookingRequestNomal/{id}")]
        public async Task<IActionResult> cancelBookingRequestNomal(Guid id)
        {
            var res = await _scheduleService.CancelBookingRoomNomal(id);
            return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);
        }
        /// <summary>
        /// Thực hiện gửi email hủy đặt phòng
        /// </summary>
        /// <param name="bookingRoom"></param>
        /// <returns></returns>
        [HttpPut("sendingEmailCancel/{id}")]
        public async Task<IActionResult> SendingEmailCancel(Guid id)
        {
            var res = await _scheduleService.SendingEmailCancel(id);
            return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);
        }


        /// <summary>
        /// Thực hiện lấy danh sach lịch sử đặt phòng
        /// </summary>
        /// <param name="BookingID"></param>
        /// <returns></returns>
        [HttpPost("historyBookingRoom")]
        public async Task<IActionResult> HistoryBookingRoom(PagingParam param)
        {
            var res = await _scheduleService.GetEntityPaging(param);
            return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);
        }
        /// <summary>
        /// Xem báo cáo theo mã BookingID
        /// </summary>
        /// <param name="entities"></param>
        [HttpGet("PrintReport")]
        public async Task<IActionResult> PrintReport(Guid id)
        {
            var res = await _scheduleService.PrintReport(id);
            // kiểm tra nếu đã có dữ liệu thì gọi API in báo cáo
            if(res != null)
            {
                // Khởi tạo một đối tượng HttpClient
                HttpClient client = new HttpClient();
                var json = JsonSerializer.Serialize(res);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gửi yêu cầu Post đến API
               HttpResponseMessage response = await client.PostAsync("https://localhost:44338/Home/ShowReport", content);

                    if (response.IsSuccessStatusCode)
                    {
                    Stream stream = await response.Content.ReadAsStreamAsync();
                    return new FileStreamResult(stream, "application/pdf");
                }
                    else
                    {
                        return BadRequest();
                    }
            }
            else
            {
                return BadRequest();
            }
            
        }


    }

}
