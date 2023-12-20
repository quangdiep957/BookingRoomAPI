using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Interfaces
{
    public interface ISubjectService : IBaseService<Subject>
    {
        /// <summary

        /// <param name="TimeSlotID"></param>
        /// bqdiep 25/03/2023
        public Task<List<ClassSubject>> GetClassActive(ClassParam param);
    }
}
