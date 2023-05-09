using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using RoomBooking.Common.AttributeCustom;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RoomBooking.DAL.Repositories
{
    public class BaseRepository<Entity> : IBaseRepository<Entity>
    {
        protected string _connectionString; // chuỗi kết nối
        protected MySqlConnection _sqlConnection;
        protected string _className; // tên của đối tượng


        /// <summary>
        /// Hàm tạo
        /// </summary> 
        /// CretedBy: PTTAM (07/03/2023)
        public BaseRepository(IConfiguration configuration)
        {

            _connectionString = configuration.GetConnectionString("ROOM_BOOKING");
            _sqlConnection = new MySqlConnection(_connectionString);

            _className = typeof(Entity).Name;
        }

        /// <summary>
        /// Thực hiện lấy toàn bộ danh sách
        /// </summary>
        /// CretedBy: PTTAM (07/03/2023)
        public virtual async Task<IEnumerable<Entity>> GetAll()
        {
           if(_sqlConnection.State!= ConnectionState.Open)
            {
                _sqlConnection.Open();
            }
            DynamicParameters dynamicParameters = new DynamicParameters();
            var storeName = $"Proc_GetAll";
            var fields = GetAllRequestValues<Entity>();
            dynamicParameters.Add("@TableName", _className);
            dynamicParameters.Add("@Properties", fields);
            var entities = await _sqlConnection.QueryAsync<Entity>(storeName, param: dynamicParameters, commandType: System.Data.CommandType.StoredProcedure);
            CloseMyConnection();
            return entities;
        }

        /// <summary>
        /// Thực hiện lấy đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// <returns>Đối tượng cần lấy </returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public virtual async Task<Entity> GetById(Guid entityId)
        {
            if (_sqlConnection.State != ConnectionState.Open)
            {
                _sqlConnection.Open();
            }
            var storeName = "Proc_GetByEntityId";
            var fields = GetAllRequestValues<Entity>();
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@EntityId", entityId);
            dynamicParameters.Add("@TableName", _className);
            dynamicParameters.Add("@Properties", fields);
            var res = await _sqlConnection.QueryFirstOrDefaultAsync<Entity>(storeName, param: dynamicParameters, commandType: System.Data.CommandType.StoredProcedure);
            CloseMyConnection();
            return res;
        }

        /// <summary>
        /// Thực hiện thêm 1 đối tượng
        /// </summary>
        /// <param name="entity">Đối tượng</param>
        /// <returns>Thêm thành công || Thêm thất bại</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public virtual async Task<bool> Insert(Entity entity,MySqlConnection cnn, MySqlTransaction transaction)
        {

            bool isSuccess = true;
            try
            {


                string sqlQuery = GetAllBindingNames(entity);
                DynamicParameters dynamicParameters = new DynamicParameters();
                sqlQuery += GetAllBindingValues(entity, 0, dynamicParameters);
                sqlQuery = sqlQuery[..^1];
                var rowEffect = await cnn.ExecuteAsync(sqlQuery, dynamicParameters, transaction: transaction);
                if (rowEffect < 1)
                {
                    isSuccess = false;
                }


            }
            catch (DbException ex)
            {
                isSuccess = false;

            }
            return isSuccess;

        }

        /// <summary>
        /// Thực hiên cập nhật đối tượng theo khóa chính
        /// </summary>
        /// <param name="entity">Đối tượng cần cập nhật</param>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// <returns>Sửa thành công || Sửa thất bại</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public virtual async Task<bool> Update(Entity entity, Guid entityId, MySqlConnection cnn, MySqlTransaction transaction)
        {
            bool isSuccess = true;


            DynamicParameters dynamicParameters = new DynamicParameters();
            string sqlQuery = GetAllBindingUpdate(entity, entityId, dynamicParameters);

            var rowEffect = await cnn.ExecuteAsync(sqlQuery, dynamicParameters, transaction: transaction);
            if (rowEffect == 0)
            {
                isSuccess = false;
            }
            return isSuccess;
        }

        /// <summary>
        /// Thực hiện xóa đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính đối tượng</param>
        /// <returns>Xóa thành công || Xóa thất bại</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public virtual async Task<bool> Delete(Guid entityId, MySqlConnection cnn, MySqlTransaction transaction)
        {
            bool isSucess = true;
            try
            {
                var storeDelete = "Proc_Delete";
                DynamicParameters paramId = new DynamicParameters();
                paramId.Add("@EntityId", entityId);
                paramId.Add("@TableName", _className);

                var res = await cnn.ExecuteAsync(storeDelete, paramId,transaction, commandType: System.Data.CommandType.StoredProcedure);

            }
            catch (Exception)
            {

                isSucess = false;
            }


            return isSucess;
        }

        /// <summary>
        /// Đóng connection
        /// </summary>
        public async Task CloseConnection()
        {
            _sqlConnection.Close();
            // _sqlConnection.Dispose();

        }

        /// <summary>
        /// Thực hiên lấy tên các trường để truy vấn dữ liệu
        /// </summary>
        ///Created by: PTTAM (30/8/2022)
        protected static string GetAllRequestValues<Entity>()
        {
            // lấy tất cả cá properties


            var properties = typeof(Entity).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ForGetting)));
            // Buid 
            var allNames = "";
            foreach (var property in properties)
            {

                // lấy giá trị 
                var currentName = property.Name;
                if (Attribute.IsDefined(property, typeof(Ambiguous)))
                {
                    allNames += typeof(Entity).Name + "." + currentName + ",";
                }
                else
                {
                    allNames += currentName + ",";

                }

            }
            // bỏ kí tự ',' cuối cùng
            allNames = allNames[..^1];
            return allNames;
        }

        protected string GetAllBindingUpdate(Entity entity, Guid entityId, DynamicParameters parameters)
        {
            // lấy tất cả cá properties ForBinding

            var properties = typeof(Entity).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ForBinding)));
            // Buid câu truy vấn
            var allNames = $"Update {typeof(Entity).Name} SET ";
            foreach (var property in properties)
            {
                if (!Attribute.IsDefined(property, typeof(PrimaryKey)))
                {
                    // lấy tên của prop 
                    allNames += property.Name + " = " + $"@{property.Name}0" + ",";
                    parameters.Add($"@{property.Name}0", property.GetValue(entity));
                }



            }
            allNames = allNames[..^1]; // loại bỏ kí tự ',' cuối cùng
                                       // Lấy ra attribute có tên PrimaryKey
          
            allNames += $" WHERE {typeof(Entity).Name}ID = @{typeof(Entity).Name}ID0;";
            parameters.Add($"@{typeof(Entity).Name}ID" + 0, entityId);
            return allNames;
        }

        /// <summary>
        /// Thực hiện lấy giá trị của đối tượng và add vào parameter
        /// </summary>
        /// <param name="entity">Đối tượng</param>
        /// <param name="index">Vị trí của đối tượng</param>
        /// <param name="parameters">Parameter</param>
        /// <returns>Chuỗi paramerter</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        protected static string GetAllBindingValues(Entity entity, int index, DynamicParameters parameters)
        {
            // lấy tất cả cá properties

            var properties = typeof(Entity).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ForBinding)));
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
        protected string GetAllBindingNames(Entity entity)
        {
            // lấy tất cả cá properties ForBinding

            var properties = typeof(Entity).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ForBinding)));
            // Buid câu truy vấn
            var allNames = $"INSERT INTO {typeof(Entity).Name}(";
            foreach (var property in properties)
            {
                // lấy tên của prop 
                allNames += property.Name + " ,";

            }

            allNames = allNames[..^1]; // loại bỏ kí tự ',' cuối cùng

            allNames += " )Values";
            var primaryKey = typeof(Entity).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PrimaryKey)));
            //Guid key = Guid.NewGuid();
            //foreach (var prop in primaryKey)
            //{
            //    prop.SetValue(entity, key);

            //}
            return allNames;
        }

        /// <summary>
        /// Thực hiện việc thêm nhiều đối tượng
        /// </summary>
        /// <param name="listEntities">Danh sách các đối tượng</param>
        /// <returns>Thêm thành công || Thêm thất bại</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public async virtual Task<bool> InsertMulti(List<Entity> listEntities, MySqlTransaction transaction,MySqlConnection cnn)
        {
            bool isSuccess = true;
            Entity entity = listEntities[0];
            string sqlQuery = GetAllBindingNames(entity);
            DynamicParameters dynamicParameters = new DynamicParameters();
            for (int i = 0; i < listEntities.Count; i++)
            {
                sqlQuery += GetAllBindingValues(listEntities[i], i, dynamicParameters);
            }
            sqlQuery = sqlQuery[..^1];
            var rowEffect = await cnn.ExecuteAsync(sqlQuery, dynamicParameters, transaction: transaction);
            if (rowEffect < listEntities.Count)
            {
                return false;
            }
            return true;
        }
    
        /// <summary>
        /// Thực hiện kiểm tra trường phải là duy nhất
        /// </summary>
        /// <param name="entityName">Tên trường</param>
        /// <param name="entityValue">Giá trị của trường</param>
        /// <param name="entityId">Khóa chính của trường</param>
        /// <returns>True: không trùng, false: trùng</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public async Task<bool> CheckUnique(string entityName, object entityValue, MySqlConnection cnn, MySqlTransaction tran,Guid? entityId = null)
        {
            var storeName = "Proc_CheckUnique";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@TableName", _className);
            parameters.Add("@EntityName", entityName);

            parameters.Add("@EntityId", entityId);

            parameters.Add("@EntityValue", entityValue);

            var res = await cnn.QueryFirstOrDefaultAsync(storeName, parameters,transaction:tran, commandType: System.Data.CommandType.StoredProcedure);

            return res != null;
        }
        public virtual async Task<Object> GetEntityPaging(PagingParam param )
        {
                if(_sqlConnection.State!= ConnectionState.Open)
                {
                    _sqlConnection.Open();
            }
            var storeName = $"Proc_GetEntityPaging";
            DynamicParameters dynamicParameters = new DynamicParameters();
               
            var fields = GetAllRequestValues<Entity>();
            var sql = $"SELECT {fields} from {_className} WHERE {_className}.{_className}Name LIKE @FilterName Order by {_className}.{_className}Name";

            dynamicParameters.Add("@TableName", _className);
                dynamicParameters.Add("@Properties", fields);
                dynamicParameters.Add("@FilterName", !String.IsNullOrEmpty(param.keyWord) ? $"%{param.keyWord}%" : $"%{""}%");
                dynamicParameters.Add("@PageSize", param.pageSize);
                dynamicParameters.Add("@PageIndex", param.pageIndex);

           
            var res = await _sqlConnection.QueryAsync(sql, param: dynamicParameters);
            int totalRecords = 0;
            int totalPages = 0;
            if (res != null)
            {
                totalRecords = res.Count();
                totalPages = (int)Math.Ceiling((decimal)(totalRecords / param.pageSize));
            }
            int offset = (int)(param.pageSize *( param.pageIndex - 1));
            sql += $" LIMIT {offset},{param.pageSize}";
            var data = await _sqlConnection.QueryAsync(sql, param: dynamicParameters);
           
          
            int startRecord = (int)(param.pageSize * (param.pageIndex - 1) + 1); // Bản ghi bắt đầu của trang hiện tại
            int endRecord = (int)(param.pageSize * (param.pageIndex - 1) + param.pageSize); // Bản ghi kết thúc của trang hiện tại

            if (endRecord > totalRecords) // nếu bản ghi kết thúc > tổng số bản ghi
            {
                endRecord = totalRecords; // gán bản ghi kết thúc = tổng số bản ghi
            }

            // nếu bản ghi bắt đầu của trang > bản ghi kết thúc
            if (startRecord > endRecord)
            {
                startRecord = endRecord;// gán bản ghi bắt đầu = bản ghi kết thúc
            }
            CloseMyConnection();
            return new
            {
                TotalPage = totalPages,
                TotalRecord = totalRecords,
                CurrentPage = param.pageIndex,
                StartRecord = startRecord,
                EndRecord = endRecord,
                Data = data
            };
        }
        public MySqlConnection GetOpenConnection()
        {
            _sqlConnection.Open();
            return _sqlConnection;
        }
        public void CloseMyConnection()
        {
            _sqlConnection.Close();
        }
    }
}
