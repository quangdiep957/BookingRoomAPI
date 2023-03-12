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
    public class SemesterService : BaseService<Semester>, ISemesterService
    {
        public SemesterService(IBaseRepository<Semester> repository) : base(repository)
        {
        }
    }
}
