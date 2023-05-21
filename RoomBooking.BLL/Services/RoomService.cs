using Dapper;
using MySqlConnector;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Exception;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace RoomBooking.BLL.Services
{
    public class RoomService : BaseService<Room>, IRoomService
    {
        IRoomRepository _repository;
        public RoomService(IRoomRepository repository, IRoleRepository roleRepository) : base(repository)
        {
            _repository = repository;
        }
        Dictionary<string, object> errors = new Dictionary<string, object>(); // Dictionary chứa lỗi
        /// <summary>
        /// override lại hàm thêm phòng
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public override async Task<bool> InsertService(Room room)
        {
            // Tạo Guid 
            room.RoomID = Guid.NewGuid();
            // Tách chuỗi id equipment
           
             string[] equipmentIDs = room.ListEquipmentID.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<RoomEquipment> equipmentRoomList = new List<RoomEquipment>();
            // For từng dòng
            foreach (var item in equipmentIDs)
            {
                if (Guid.TryParse(item, out Guid equipmentID))
                {
                    equipmentRoomList.Add(new RoomEquipment
                    {
                        EquipmentID = equipmentID,
                        RoomID = room.RoomID
                    });
                }
            }
     
            
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                // Gọi đến hàm validate dữ liệu

                using (MySqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        ValidateError(room, cnn, tran);

                        // kiểm tra biến isValidCustom và listErrors thỏa mãn điều kiện thì gọi repository để thực hiện việc thêm mới
                        if (isValidCustom == true && errorList.Count <= 0)
                        {
                            var res = await _repository.Insert(room, cnn, tran);
                            if (res)
                            {
                                var resMulti = await _repository.InsertMultiEntity(equipmentRoomList, tran, cnn);
                                if (resMulti)
                                {
                                    tran.Commit();
                                }
                                

                            }
                            else { tran.Rollback(); }
                            return res;
                        }
                        else // Ngược lại throw ra lỗi
                        {

                            throw new ValidateException(errors);
                        }
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }


                }
            }

        }
        /// <summary>
        /// override lại sửa phòng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        public override async Task<bool> UpdateService(Guid id ,Room room)
        {
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                try
                {
                    using (MySqlTransaction tran = cnn.BeginTransaction())
                    {
                        try
                        {
                            // Thực hiện xóa thiết bị phòng
                            var sqlDelEquipment = "DELETE FROM RoomEquipment where RoomID = @RoomID";
                            DynamicParameters param = new DynamicParameters();
                            param.Add("@RoomID", id);
                            var resDel= await cnn.ExecuteAsync(sqlDelEquipment, param, transaction: tran);
                            // Thực hiện thêm lại thiết bị phòng
                            string[] equipmentIDs = room.ListEquipmentID.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            List<RoomEquipment> equipmentRoomList = new List<RoomEquipment>();
                            // For từng dòng
                            foreach (var item in equipmentIDs)
                            {
                                if (Guid.TryParse(item, out Guid equipmentID))
                                {
                                    equipmentRoomList.Add(new RoomEquipment
                                    {
                                        EquipmentID = equipmentID,
                                        RoomID = room.RoomID
                                    });
                                }
                            }
                            var resMulti = await _repository.InsertMultiEntity(equipmentRoomList, tran, cnn);

                            // Thực hiện update lại room
                            var res = await _repository.Update(room, id, cnn, tran);
                            if (res)
                            {
                                tran.Commit();
                            }else { tran.Rollback(); }
                            return res;

                        }
                        catch (Exception)
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }
                finally
                {
                    cnn.Close();
                }
            }
        }

        public override async Task<bool> DeleteService(Guid entityId)
        {
            bool isSuccess = true;
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                try
                {
                    using (MySqlTransaction tran = cnn.BeginTransaction())
                    {
                        try
                        {
                            var roombookings = await cnn.QueryAsync<BookingRoom>("SELECT * From BookingRoom;", transaction: tran);
                            var room = roombookings.FirstOrDefault(x => x.RoomID == entityId);
                            if (room != null)
                            {
                                isSuccess = false;
                            }
                            else
                            {
                                var sql = "Delete From RoomEquipment WHERE RoomID=@RoomID";
                                DynamicParameters param = new DynamicParameters();
                                param.Add("@RoomID", entityId);
                                var res = await cnn.ExecuteAsync(sql, param, transaction: tran);
                                var result = await _repository.Delete(entityId, cnn, tran);
                                if (result)
                                {
                                    tran.Commit();
                                }
                            }
                            
                        }
                        catch (Exception)
                        {
                            isSuccess = false;
                            tran.Rollback();
                            throw;
                        }
                    }
                }
                finally
                {
                    cnn.Close();
                }
            }
            return isSuccess;
        }
    }
}
