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
    public interface IRoomRepository : IBaseRepository<Room>
    {
        /// <summary>
        /// Thêm nhiều thiết bị
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="tran"></param>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public Task<bool> InsertMultiEntity(List<RoomEquipment> entity, MySqlTransaction tran, MySqlConnection cnn);

        /// <summary>
        /// Thực hiện validate khi xóa nhiều đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// Created by: PTTAM (07/03/2023)
        public Task<bool> DeleteMulti(List<Guid> entityId, MySqlConnection cnn, MySqlTransaction transaction);



    }
}
