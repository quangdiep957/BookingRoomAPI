using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.API.Controllers;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;

namespace BuildingBooking.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EquipmentTypesController : BasesController<EquipmentType>
    {
        IEquipmentTypeService _service;
        // /// <summary>
        // /// Hàm tạo thực hiện việc giao tiếp giữa Interface và Controller
        // /// </summary>
        // /// <param name="service">Thực hiện validate dữ liệu</param>
        // /// <param name="repository">Thực hiện các chức năng</param>
        // /// Created by: PTTAM (30/8/2022)
        public EquipmentTypesController(IEquipmentTypeService service) : base(service)
        {
            _service = service;
        }
    }
}
