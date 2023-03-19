using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using RoomBooking.Common.AttributeCustom;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<IEnumerable<Entity>> GetAll()
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            var storeName = $"Proc_GetAll";
            var fields = GetAllRequestValues<Entity>();
            dynamicParameters.Add("@TableName", _className);
            dynamicParameters.Add("@Properties", fields);
            var entities = await _sqlConnection.QueryAsync<Entity>(storeName, param: dynamicParameters, commandType: System.Data.CommandType.StoredProcedure);

            CloseConnection();
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
            var storeName = "Proc_GetByEntityId";
            var fields = GetAllRequestValues<Entity>();
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@EntityId", entityId);
            dynamicParameters.Add("@TableName", _className);
            dynamicParameters.Add("@Properties", fields);
            var res = await _sqlConnection.QueryFirstOrDefaultAsync<Entity>(storeName, param: dynamicParameters, commandType: System.Data.CommandType.StoredProcedure);
            CloseConnection();
            return res;
        }

        /// <summary>
        /// Thực hiện thêm 1 đối tượng
        /// </summary>
        /// <param name="entity">Đối tượng</param>
        /// <returns>Thêm thành công || Thêm thất bại</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public async Task<bool> Insert(Entity entity)
        {
            var storeName = $"Proc_Insert{_className}";

            var res = await _sqlConnection.ExecuteAsync(storeName, param: entity, commandType: System.Data.CommandType.StoredProcedure);
            CloseConnection();
            if (res == 1)
            {
                return true;

            }
            else
            {
                return false;
                //return MISAResource.InsertFail;
            }
        }

        /// <summary>
        /// Thực hiên cập nhật đối tượng theo khóa chính
        /// </summary>
        /// <param name="entity">Đối tượng cần cập nhật</param>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// <returns>Sửa thành công || Sửa thất bại</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public async Task<bool> Update(Entity entity, Guid entityId)
        {
            if (_sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                _sqlConnection.Open();
            }
            MySqlTransaction transaction = _sqlConnection.BeginTransaction();

            try
            {

                var storeUpdate = $"Proc_Update{_className}"; // tên thủ tục cập nhật đối tượng

                // Lấy ra attribute có tên PrimaryKey
                var primaryKey = typeof(Entity).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PrimaryKey)));
                foreach (var prop in primaryKey)
                {
                    prop.SetValue(entity, entityId); // gán lại id của đối tượng

                }
                var rowEffect = await _sqlConnection.ExecuteAsync(storeUpdate, param: entity, transaction: transaction, commandType: System.Data.CommandType.StoredProcedure);
                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                CloseConnection();

            }
        }

        /// <summary>
        /// Thực hiện xóa đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính đối tượng</param>
        /// <returns>Xóa thành công || Xóa thất bại</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public async Task<bool> Delete(Guid entityId)
        {
            var storeDelete = "Proc_Delete";
            DynamicParameters paramId = new DynamicParameters();
            paramId.Add("@EntityId", entityId);
            paramId.Add("@TableName", _className);
            var res = await _sqlConnection.ExecuteAsync(storeDelete, paramId, commandType: System.Data.CommandType.StoredProcedure);
            CloseConnection();
            if (res == 1)
            {
                return true;

            }
            else return false;

        }

        /// <summary>
        /// Đóng connection
        /// </summary>
        public void CloseConnection()
        {
            _sqlConnection.Close();

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
            return allNames;
        }

        /// <summary>
        /// Thực hiện việc thêm nhiều đối tượng
        /// </summary>
        /// <param name="listEntities">Danh sách các đối tượng</param>
        /// <returns>Thêm thành công || Thêm thất bại</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public async virtual Task<bool> InsertMulti(List<Entity> listEntities)
        {
            if (_sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                _sqlConnection.Open();
            }
            Entity entity = listEntities[0];
            string sqlQuery = GetAllBindingNames(entity);
            DynamicParameters dynamicParameters = new DynamicParameters();
            MySqlTransaction transaction = _sqlConnection.BeginTransaction(); 
            for (int i = 0; i < listEntities.Count; i++)
            {
                sqlQuery += GetAllBindingValues(listEntities[i], i, dynamicParameters);
            }
            sqlQuery = sqlQuery[..^1];
            var rowEffect = await _sqlConnection.ExecuteAsync(sqlQuery, dynamicParameters, transaction: transaction);
            if (rowEffect < listEntities.Count)
            {
                transaction.Rollback();
                return false;
            }
            transaction.Commit();
            CloseConnection();
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
        public async Task<bool> CheckUnique(string entityName, object entityValue, Guid? entityId = null)
        {
            var storeName = "Proc_CheckUnique";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@TableName", _className);
            parameters.Add("@EntityName", entityName);

            parameters.Add("@EntityId", entityId);

            parameters.Add("@EntityValue", entityValue);

            var res = await _sqlConnection.QueryFirstOrDefaultAsync(storeName, parameters, commandType: System.Data.CommandType.StoredProcedure);
            CloseConnection();
            return res != null;
        }

        public async Task<Object> GetEntityPaging(string filterName, int pageSize, int pageIndex)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            var storeName = $"Proc_GetEntityPaging";
            var fields = GetAllRequestValues<Entity>();
            dynamicParameters.Add("@TableName", _className);
            dynamicParameters.Add("@Properties", fields);
            dynamicParameters.Add("@FilterName", !String.IsNullOrEmpty(filterName)?filterName:"");
            dynamicParameters.Add("@PageSize", pageSize);
            dynamicParameters.Add("@PageIndex", pageIndex);
            var data = await _sqlConnection.QueryAsync(storeName, param: dynamicParameters, commandType: System.Data.CommandType.StoredProcedure);
            int totalRecords = 0;
            int totalPages = 0;
            if (data!= null ) {
                totalRecords = data.Count();
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            }
            int startRecord = pageSize * (pageIndex - 1) + 1; // Bản ghi bắt đầu của trang hiện tại
            int endRecord = pageSize * (pageIndex - 1) + pageSize; // Bản ghi kết thúc của trang hiện tại

            if (endRecord > totalRecords) // nếu bản ghi kết thúc > tổng số bản ghi
            {
                endRecord = totalRecords; // gán bản ghi kết thúc = tổng số bản ghi
            }

            // nếu bản ghi bắt đầu của trang > bản ghi kết thúc
            if (startRecord > endRecord)
            {
                startRecord = endRecord;// gán bản ghi bắt đầu = bản ghi kết thúc
            }
         
            CloseConnection();
            return new
            {
                TotalPage = totalPages,
                TotalRecord = totalRecords,
                CurrentPage = pageIndex,
                StartRecord = startRecord,
                EndRecord = endRecord,
                Data = data
            };
        }
    }
}
