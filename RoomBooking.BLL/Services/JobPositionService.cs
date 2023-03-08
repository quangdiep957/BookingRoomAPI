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
    public class JobPositionService : BaseService<Job_Position>, IJobPositionService
    {
        public JobPositionService(IBaseRepository<Job_Position> repository) : base(repository)
        {
        }
    }
}
