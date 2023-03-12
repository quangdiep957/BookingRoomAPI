using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Services
{
    public class EquipmentService : BaseService<Equipment>, IEquipmentService
    {
        public EquipmentService(IBaseRepository<Equipment> repository) : base(repository)
        {
        }
    }
}
