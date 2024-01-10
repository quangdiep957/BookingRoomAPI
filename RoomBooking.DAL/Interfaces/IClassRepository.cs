using MySqlConnector;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.DAL.Interfaces
{
    public interface IClassRepository : IBaseRepository<Class>
    {

        /// <summary>
        /// lấy danh sách lớp
        /// </summary>
        /// <param name="param"></param>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public Task<List<Class>> GetClassActive(ClassParam param, MySqlConnection cnn);
    }

}
