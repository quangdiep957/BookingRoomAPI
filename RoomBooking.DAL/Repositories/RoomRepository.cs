using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using RoomBooking.Common.AttributeCustom;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace RoomBooking.DAL.Repositories
{
    public class RoomRepository : BaseRepository<Room>, IRoomRepository
    {
        public RoomRepository(IConfiguration configuration) : base(configuration)
        {
        }
        /// <summary>
        /// Thực hiện lấy giá trị của đối tượng và add vào parameter
        /// </summary>
        /// <param name="entity">Đối tượng</param>
        /// <param name="index">Vị trí của đối tượng</param>
        /// <param name="parameters">Parameter</param>
        /// <returns>Chuỗi paramerter</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public string GetAllBindingValue(RoomEquipment entity, int index, DynamicParameters parameters)
        {
            // lấy tất cả cá properties

            var properties = typeof(RoomEquipment).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ForBinding)));
            // Buid 
            var allValuesParam = "( ";
            foreach (var property in properties)
            {
                // lấy giá trị
                allValuesParam += "@" + property.Name + index + " ,";


            }
            allValuesParam = allValuesParam[..^1];
            allValuesParam += " ),";
            foreach (var property in properties)
            {

                // lấy giá trị
                var currentValue = property.GetValue(entity);
                parameters.Add($"@{property.Name}" + index, currentValue);


            }

            // bỏ kí tự ',' cuối cùng
            return allValuesParam;

        }

        /// <summary>
        /// Thực hiện lấy tên trường của đối tượng khi thêm mới
        /// </summary>
        /// <param name="entity">Đối tượng</param>
        /// <returns>Chuỗi chứa câu lệnh insert chứa tên các trường</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        protected string GetAllBindingName(RoomEquipment entity)
        {
            // lấy tất cả cá properties ForBinding

            var properties = typeof(RoomEquipment).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ForBinding)));
            // Buid câu truy vấn
            var allNames = $"INSERT INTO {typeof(RoomEquipment).Name}(";
            foreach (var property in properties)
            {
                // lấy tên của prop 
                allNames += property.Name + " ,";

            }

            allNames = allNames[..^1]; // loại bỏ kí tự ',' cuối cùng

            allNames += " )Values";
            return allNames;
        }

        /// <summary>
        /// Thêm nhiều thiết bị trong phòng học
        /// </summary>
        /// <param name="listEntities"></param>
        /// <param name="tran"></param>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public async Task<bool> InsertMultiEntity(List<RoomEquipment> listEquipments, MySqlTransaction tran, MySqlConnection cnn)
        {
            bool isSuccess = true;
            int countEquipment = 0; // biến đếm khi thực hiện thêm thiết bị
            string sqlQuery = GetAllBindingName(listEquipments[0]); // câu truy vấn lấy ra tên trường của user
            DynamicParameters dynamicParameters = new DynamicParameters();
            for (int i = 0; i < listEquipments.Count; i++)
            {
                sqlQuery += GetAllBindingValue(listEquipments[i], i, dynamicParameters); // Thực hiện buid câu truy vấn 
                countEquipment++;
            }
            sqlQuery = sqlQuery[..^1]; // bỏ dấu ',' cuối cùng
            var rowEffect = await cnn.ExecuteAsync(sqlQuery, dynamicParameters, transaction: tran);

            // nếu số bản ghi thay đổi < countUser
            if (rowEffect < countEquipment)
            {
                isSuccess = false;
            }
            return isSuccess;
        }

        ///// <summary>
        ///// Thực hiện xóa đối tượng theo khóa chính
        ///// </summary>
        ///// <param name="entityId">Khóa chính đối tượng</param>
        ///// <returns>Xóa thành công || Xóa thất bại</returns>
        /////  CretedBy: PTTAM (07/03/2023)
        //public override async Task<bool> Delete(Guid entityId, MySqlConnection cnn, MySqlTransaction transaction)
        //{
        //    bool isSucess = true;
        //    try
        //    {
        //        var storeDelete = "Proc_Delete_Record";

        //        var properties = typeof(RoomEquipment).GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(KeyDelete)));
        //        DynamicParameters paramId = new DynamicParameters();
        //        paramId.Add("@EntityId", entityId);
        //        paramId.Add("@TableName", "roomequipment");
        //        paramId.Add("@Property", properties.Name);

        //        var res = await cnn.ExecuteAsync(storeDelete, paramId, transaction, commandType: System.Data.CommandType.StoredProcedure);

        //    }
        //    catch (Exception)
        //    {

        //        isSucess = false;
        //    }


        //    return isSucess;
        //}

        /// <summary>
        /// hàm xóa nhiều
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<bool> DeleteMulti(List<Guid> ids, MySqlConnection cnn, MySqlTransaction transaction)
        {
            bool isSucess = true;
            try
            {
                var conditionWhere = " IN(";
                for (int i = 0; i < ids.Count; i++)
                {
                    conditionWhere = conditionWhere + $"'{ids[i]}',";
                }
                conditionWhere = conditionWhere.Remove(conditionWhere.Length - 1) + ")";
                var storeDelete = "Proc_Delete_Multi";
                // lấy tên key của bảng
                // truy vấn dữ liệu
                var properties = typeof(RoomEquipment).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(KeyDelete)));
                DynamicParameters paramId = new DynamicParameters();
                paramId.Add("@EntityId", properties);
                paramId.Add("@TableName", "roomequipment");
                paramId.Add("@conditionWhere", conditionWhere);

                var res = await cnn.ExecuteAsync(storeDelete, paramId, transaction, commandType: System.Data.CommandType.StoredProcedure);

            }
            catch (Exception)
            {

                isSucess = false;
            }


            return isSucess;
        }
        /// <summary>
        /// hàm getpaging phòng học
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        //public override async Task<Object> GetEntityPaging(PagingParam param)
        //{
        //    if (_sqlConnection.State != ConnectionState.Open)
        //    {
        //        _sqlConnection.Open();
        //    }
        //    var orConditions = new List<string>();
        //    string whereClause = "";

        //    if (param.keyWord != null)
        //    {
        //        orConditions.Add($"r.RoomName LIKE '%{param.keyWord}%'");
        //        orConditions.Add($"r.Capacity LIKE '%{param.keyWord}%'");
        //        orConditions.Add($"e.EquipmentName LIKE '%{param.keyWord}%'");
        //    }

        //    if (orConditions.Count > 0)
        //    {
        //        whereClause = $"({string.Join(" OR ", orConditions)})";
        //    }
        //    var parameters = new DynamicParameters();
        //    parameters.Add("@v_Offset", (param.pageIndex - 1) * param.pageSize);
        //    parameters.Add("@v_Limit", param.pageSize);
        //    parameters.Add("@v_Sort", "r.ModifiedDate DESC");
        //    parameters.Add("@v_Where", whereClause);
        //    var sqlCommand = "Proc_RoomEquipment_GetPaging";
        //    var data = await _sqlConnection.QueryAsync(sqlCommand, param: parameters, commandType: System.Data.CommandType.StoredProcedure);
        //    int totalRecords = 0;
        //    int totalPages = 0;
        //    if (data != null)
        //    {
        //        totalRecords = data.Count();
        //        totalPages = (int)Math.Ceiling((decimal)(totalRecords / param.pageSize));
        //    }
        //    int startRecord = (int)(param.pageSize * (param.pageIndex - 1) + 1); // Bản ghi bắt đầu của trang hiện tại
        //    int endRecord = (int)(param.pageSize * (param.pageIndex - 1) + param.pageSize); // Bản ghi kết thúc của trang hiện tại

        //    if (endRecord > totalRecords) // nếu bản ghi kết thúc > tổng số bản ghi
        //    {
        //        endRecord = totalRecords; // gán bản ghi kết thúc = tổng số bản ghi
        //    }

        //    // nếu bản ghi bắt đầu của trang > bản ghi kết thúc
        //    if (startRecord > endRecord)
        //    {
        //        startRecord = endRecord;// gán bản ghi bắt đầu = bản ghi kết thúc
        //    }
        //    CloseMyConnection();
        //    return new
        //    {
        //        TotalPage = totalPages,
        //        TotalRecord = totalRecords,
        //        CurrentPage = param.pageIndex,
        //        StartRecord = startRecord,
        //        EndRecord = endRecord,
        //        Data = data
        //    };
        //}

    }
}
