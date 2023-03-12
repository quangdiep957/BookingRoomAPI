using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;

namespace RoomBooking.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AccountsController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login( AuthenticateRequest model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);

            //if (user == null)
            //{
            //    return BadRequest(new { message = "Username or password is incorrect" });
            //}

            var token = _tokenService.GenerateJwtToken(user);
           return  StatusCode(200, token);
            //return Ok(new { token });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            _tokenService.RevokeToken(token);

            return Ok(new { message = "Logout successful" });
        }

        [Authorize]
        [HttpGet("check")]
        public IActionResult Check()
        {
            return Ok(new { message = "Token is valid" });
        }


    }
}
