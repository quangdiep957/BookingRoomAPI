using Microsoft.Extensions.Caching.Memory;
using MySqlConnector;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Services
{
    public class ClassService : BaseService<Class>, IClassService
    {
        IClassRepository _repository;
        public ClassService(IClassRepository repository) : base(repository)
        {
            _repository = repository;
        }

        public async Task<List<Class>> GetClassActive(ClassParam param)
        {
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                var res = await _repository.GetClassActive(param, cnn);
                return res;
            } 
        }
    }
}
