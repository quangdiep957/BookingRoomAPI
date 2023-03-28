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
    public class BasesController<Entity> : ControllerBase
    {
        IBaseService<Entity> _service; // interface thực hiện validate dữ liệu


        #region Contructor

        /// <summary>
        /// Hàm tạo thực hiện việc giao tiếp giữa Interface và Controller
        /// </summary>
        /// <param name="service">Interface service</param>
        /// <param name="repository">Interface repository</param>
        /// Created by: PTTAM (08/03/2023)
        public BasesController(IBaseService<Entity> service)
        {
            _service = service;
        }


        #endregion


        #region Resful API

        /// <summary>
        /// Phân trang
        /// </summary>
        /// <returns></returns>
        [HttpPost("paging")]
        public async Task<IActionResult> GetEntityPaging([FromBody] PagingParam param)
        {
            var res = await _service.GetEntityPaging(param);

            return StatusCode(200, res);
        }
        /// <summary>
        /// Thực hiện lấy toàn bộ dữ liệu
        /// </summary>
        /// <returns>
        /// 200 - Danh sách dữ liệu 
        /// 204 - Không có dữ liệu
        /// 400 - Lỗi do đầu vào
        /// 500 - Lỗi do hệ thống
        /// </returns>
        /// Created by: PTTAM (08/03/2023)
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            return StatusCode((int)HTTPStatusCode.SuccessResponse,await _service.GetAllService());
        }

        /// <summary>
        /// Thực hiện lấy đối tượng theo khóa chính
        /// </summary>
        /// <param name="id">Khóa chính của đối tương</param>
        /// <returns>
        /// 200 - Lấy đối tượng thành công
        /// 204 - Đối tượng không tồn tại
        /// 400 - Lỗi do đầu vào
        /// 500 - Lỗi do hệ thống
        /// </returns>
        /// Created by: PTTAM (08/03/2023)
        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return StatusCode((int)HTTPStatusCode.SuccessResponse,await _service.GetByIdService(id));
        }

        /// <summary>
        /// Thực hiện thêm mới đối tượng 
        /// </summary>
        /// <param name="entity">đối tượng thêm mới</param>
        /// <returns>
        /// 200 - Thêm mới thành công
        /// 400 - Lỗi do đầu vào
        /// 500 - Lỗi do hệ thống
        /// </returns>
        /// created by: PTTAM (08/03/2023)
        [HttpPost("insert")]
        public async Task<IActionResult> Insert(Entity entity)
        {
            var res =await _service.InsertService(entity);
            return StatusCode(201, res);

        }

        /// <summary>
        /// Thực hiện sửa 1 đối tượng theo khóa chính
        /// </summary>
        /// <param name="id">khóa chính của  đối tượng</param>
        /// <param name="entity">đôi tượng cần sửa</param>
        /// <returns>
        /// 200 - Sửa thành công
        /// 400 - Lỗi do đầu vào
        /// 500 - Lỗi do hệ thống
        /// </returns>
        /// created by: PTTAM (08/03/2023)
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(Guid id, Entity entity)
        {
            var res = await _service.UpdateService(id, entity);
            return StatusCode(Convert.ToInt32(HTTPStatusCode.SuccessResponse), res);
        }

        /// <summary>
        /// Thực hiến xóa đối tượng theo khóa chính
        /// </summary>
        /// <param name="id">Khóa chính </param>
        /// <returns>
        /// 200 - Danh sách dữ liệu 
        /// 204 - Không có dữ liệu
        /// 400 - Lỗi do đầu vào
        /// 500 - Lỗi do hệ thống
        /// </returns>
        /// created by: PTTAM (08/03/2023)
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return StatusCode(200,await _service.DeleteService(id));
        }

        /// <summary>
        /// Insert multi đối tượng
        /// </summary>
        /// <param name="entities"></param>
        [HttpPost("InsertMultiple")]
        public async Task<IActionResult> InsertMultiple(List<Entity> entities)
        {
            return StatusCode(201, await _service.InsertMultiService(entities));
        }
        #endregion
    }
}
