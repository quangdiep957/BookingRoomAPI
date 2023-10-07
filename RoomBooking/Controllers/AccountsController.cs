﻿using ExcelDataReader;
using Google.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

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

        /// <summary>
        /// Thực hiện đăng nhập
        /// </summary>
        /// <param name="model">username và password</param>
        /// bqdiep

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthenticateRequest model)
        {
            var user = await _userService.Authenticate(model.Username, model.Password);

            if (user == null)
            {
                return StatusCode(200, false);
            }

            var token =await _tokenService.GenerateToken(user);
            return StatusCode(200, token);
            //return Ok(new { token });
        }
        /// <summary>
        /// login bằng google
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost("loginGoogle")]
        public async Task<IActionResult> LoginGoogle([FromBody] User param)
        {
            var res = await _userService.LoginGoogle(param);
            if(res != null)
            {
                var token = await _tokenService.GenerateToken(res);
                return StatusCode(200, token);
            }
            else
            {
                return BadRequest(new { message = "loi xac thuc" });
            }
           

        }

        /// <summary>
        /// Thực hiện đăng xuất
        /// </summary>
        /// bqdiep 
        //[Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            _tokenService.InvalidateToken(token);

            return Ok(new { message = "Logout successful" });
        }

        /// <summary>
        /// Thực hiện đổi mật khẩu
        /// </summary>
        /// bqdiep 
        [HttpPost("changepass")]
        public IActionResult ChangePass(User user)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token is required" });
            }
            _userService.ChangePass(user);

            return Ok(new { message = "Change PassWord successful" });
        }

        [Authorize]
        [HttpGet("check")]
        public IActionResult Check()
        {
            return Ok(new { message = "Token is valid" });
        }
      

    }
}
