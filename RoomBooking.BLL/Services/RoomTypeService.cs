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
    public class RoomTypeService : BaseService<RoomType>, IRoomTypeService
    {
        public RoomTypeService(IBaseRepository<RoomType> repository) : base(repository)
        {
        }
    }
}
