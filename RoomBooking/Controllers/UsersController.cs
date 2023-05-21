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
        [HttpPost("pagingUser")]
        public async Task<IActionResult> FilterUser([FromBody]PagingParam param)
        {

            var res =await _service.GetPaging((int)param.pageSize, (int)param.pageIndex, param.keyWord,param.roleID);

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
        /// Thực hiện đổi mật khẩu
        /// </summary>
        /// PTTAM 
        [HttpPost("changepass")]
        public async Task<IActionResult> ChangePass(User user)
        {
           
            var res= await _service.ChangePass(user);

            return StatusCode(200, res);
        }


    }
}
