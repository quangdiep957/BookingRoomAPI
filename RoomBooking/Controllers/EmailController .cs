using ExcelDataReader;
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
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;

        public EmailController(IEmailService emailService, ITokenService tokenService)
        {
            _emailService = emailService;
            _tokenService = tokenService;
        }

        [HttpPost]
        public bool SendEmail(EmailData emailData)
        {
            var emailSettings = new EmailSettings();
            return _emailService.SendEmail(emailData);
        }


    }
}
