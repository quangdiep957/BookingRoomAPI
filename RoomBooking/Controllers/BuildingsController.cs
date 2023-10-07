﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.API.Controllers;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;

namespace BuildingBooking.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BuildingsController : BasesController<Building>
    {
        IBuildingService _service;
        // /// <summary>
        // /// Hàm tạo thực hiện việc giao tiếp giữa Interface và Controller
        // /// </summary>
        // /// <param name="service">Thực hiện validate dữ liệu</param>
        // /// <param name="repository">Thực hiện các chức năng</param>
        // /// Created by: bqdiep (30/8/2022)
        public BuildingsController(IBuildingService service) : base(service)
        {
            _service = service;
        }
    }
}
