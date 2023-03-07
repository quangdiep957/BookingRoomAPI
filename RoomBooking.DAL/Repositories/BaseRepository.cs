using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using RoomBooking.Common.AttributeCustom;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
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

            _connectionString = configuration.GetConnectionString("MISA_PROCESS");
            _sqlConnection = new MySqlConnection(_connectionString);

            _className = typeof(Entity).Name;
        }

        /// <summary>
        /// Thực hiện lấy toàn bộ danh sách
        /// </summary>
        /// CretedBy: PTTAM (07/03/2023)
        public IEnumerable<Entity> GetAll()
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            var storeName = $"Proc_GetAll";
            var fields = GetAllRequestValues<Entity>();
            dynamicParameters.Add("@TableName", _className);
            dynamicParameters.Add("@Properties", fields);
            var entities = _sqlConnection.Query<Entity>(storeName, param: dynamicParameters, commandType: System.Data.CommandType.StoredProcedure);

            CloseConnection();
            return entities;
        }

        /// <summary>
        /// Thực hiện lấy đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// <returns>Đối tượng cần lấy </returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public virtual Entity GetById(Guid entityId)
        {
            var storeName = "Proc_GetByEntityId";
            var fields = GetAllRequestValues<Entity>();
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@EntityId", entityId);
            dynamicParameters.Add("@TableName", _className);
            dynamicParameters.Add("@Properties", fields);
            var res = _sqlConnection.QueryFirstOrDefault<Entity>(storeName, param: dynamicParameters, commandType: System.Data.CommandType.StoredProcedure);
            CloseConnection();
            return res;
        }

        /// <summary>
        /// Thực hiện thêm 1 đối tượng
        /// </summary>
        /// <param name="entity">Đối tượng</param>
        /// <returns>Thêm thành công || Thêm thất bại</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public string Insert(Entity entity)
        {
            var storeName = $"Proc_Insert{_className}";

            var res = _sqlConnection.Execute(storeName, param: entity, commandType: System.Data.CommandType.StoredProcedure);
            CloseConnection();
            if (res == 1)
            {
                return "Thành công";
               // return MISAResource.InsertSuccess;

            }
            else
            {
                return "Thất bại";
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
        public string Update(Entity entity, Guid entityId)
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
                var rowEffect = _sqlConnection.Execute(storeUpdate, param: entity, transaction: transaction, commandType: System.Data.CommandType.StoredProcedure);
                transaction.Commit();

                return "Thành công";
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
        public string Delete(Guid entityId)
        {
            var storeDelete = "Proc_Delete";
            DynamicParameters paramId = new DynamicParameters();
            paramId.Add("@EntityId", entityId);
            paramId.Add("@TableName", _className);
            var res = _sqlConnection.Execute(storeDelete, paramId, commandType: System.Data.CommandType.StoredProcedure);
            CloseConnection();
            if (res == 1)
            {
                return "Thành công";

            }
            else return "Thất bại";

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
        public virtual string InsertMulti(List<Entity> listEntities)
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
            var rowEffect = _sqlConnection.Execute(sqlQuery, dynamicParameters, transaction: transaction);
            if (rowEffect < listEntities.Count)
            {
                transaction.Rollback();
                return "Thất bại";
            }
            transaction.Commit();
            CloseConnection();
            return "Thành công";
        }

        /// <summary>
        /// Thực hiện kiểm tra trường phải là duy nhất
        /// </summary>
        /// <param name="entityName">Tên trường</param>
        /// <param name="entityValue">Giá trị của trường</param>
        /// <param name="entityId">Khóa chính của trường</param>
        /// <returns>True: không trùng, false: trùng</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public bool CheckUnique(string entityName, object entityValue, Guid? entityId = null)
        {
            var storeName = "Proc_CheckUnique";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@TableName", _className);
            parameters.Add("@EntityName", entityName);

            parameters.Add("@EntityId", entityId);

            parameters.Add("@EntityValue", entityValue);

            var res = _sqlConnection.QueryFirstOrDefault(storeName, parameters, commandType: System.Data.CommandType.StoredProcedure);
            CloseConnection();
            return res != null;
        }
    }
}
