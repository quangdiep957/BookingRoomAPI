using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RoomBooking.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsersController : BasesController<User>
    {
        IUserService _service;

        // /// <summary>
        // /// Hàm tạo thực hiện việc giao tiếp giữa Interface và Controller
        // /// </summary>
        // /// <param name="service">Thực hiện validate dữ liệu</param>
        // /// <param name="repository">Thực hiện các chức năng</param>
        // /// Created by: PTTAM (08/03/2023)
        public UsersController(IUserService service) : base(service)
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
        [HttpGet("filter")]
        public async Task<IActionResult> FilterUser(int pageSize, int pageIndex, string? keyWord, Guid? roleId)
        {

            var res =await _service.GetPaging(pageSize, pageIndex, keyWord, roleId);

            return StatusCode(200, res);

        }

        /// <summary>
        /// Thực hiện lấy mã người dùng mới
        /// </summary>
        /// <returns>
        /// 200 - Danh sách dữ liệu 
        /// 204 - Không có dữ liệu
        /// 400 - Lỗi do đầu vào
        /// 500 - Lỗi do hệ thống
        /// </returns>
        [HttpGet("NewUserCode")]
        public async Task<IActionResult> GetNewUserCode()
        {
            var res = await _service.GetNewUserCode();
            return StatusCode(200, res);
        }

        /// <summary>
        /// Thực hiện sửa vai trò của người dùng theo khóa chính
        /// </summary>
        /// <param name="userId">Khóa chính người dùng</param>
        /// <param name="roles">Mảng vai trò</param>
        /// <returns> /// 200 - Danh sách dữ liệu 
        /// 204 - Không có dữ liệu
        /// 400 - Lỗi do đầu vào
        /// 500 - Lỗi do hệ thống</returns>
        [HttpPost("UserRole")]
        public IActionResult UpdateUserRole(Guid userId, List<UserRole> roles)
        {
            var res = _service.UpdateUserRole(userId, roles);
            return StatusCode(200, res);
        }

    }
}
