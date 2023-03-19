using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.BLL.Interfaces;
using RoomBooking.BLL.Services;

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
    }
}
