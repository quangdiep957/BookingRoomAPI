using Microsoft.AspNetCore.Mvc.Filters;
using MySqlConnector;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.AttributeCustom;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using RoomBooking.Common.Exception;
using RoomBooking.Common.Resources;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Services
{

    public class BaseService<Entity> : IBaseService<Entity>
    {
        protected IBaseRepository<Entity> _repository; // Interface repository 
        protected List<object> errorList = new List<object>();// Danh sách lỗi
        protected bool isValidCustom = true; // Biến check validate của lớp kế thừa lại BaseService
        bool isValidLength = true; // Biến check độ dài
        protected Dictionary<string, object> errors = new Dictionary<string, object>(); // Dictionary chứa lỗi

        /// <summary>
        /// Hàm tạo
        /// </summary>
        /// <param name="repository"></param>
        public BaseService(IBaseRepository<Entity> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Thực hiện nghiệp vụ khi thêm mới người dùng
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Thêm mới thành công || Thêm mới thất bại</returns>
        /// <exception cref="ValidateException"></exception>
        /// Created by: PTTAM (07/03/2023)
        public async virtual Task<bool> InsertService(Entity entity)
        {
         
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                // Gọi đến hàm validate dữ liệu

                using (MySqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        ValidateError(entity, cnn, tran);

                        // kiểm tra biến isValidCustom và listErrors thỏa mãn điều kiện thì gọi repository để thực hiện việc thêm mới
                        if (isValidCustom == true && errorList.Count <= 0)
                        {
                            var res = await _repository.Insert(entity, cnn, tran);
                            if (res == true)
                            {
                                tran.Commit();

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
        /// Thực hiện nghiệp vụ cập nhật đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính đối tượng</param>
        /// <param name="entity">Đối tượng</param>
        /// <returns>Cập nhật thành công || Cập nhật thất bại</returns>
        /// Created by: PTTAM (07/03/2023)
        public async Task<bool> UpdateService(Guid entityId, Entity entity)
        {
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                try
                {
                    using (MySqlTransaction tran = cnn.BeginTransaction())
                    {
                        try
                        {
                          //  ValidateError(entity, cnn,tran);
                            if (isValidCustom == true && errorList.Count <= 0)
                            {
                                var res = await _repository.Update(entity, entityId, cnn, tran);
                                if (res)
                                {
                                    tran.Commit();
                                    return res;
                                }
                                else
                                {
                                    tran.Rollback();
                                    return res;
                                }
                            }
                            else // Ngược lại throw ra lỗi
                            {
                                throw new ValidateException(errors);
                            }
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

        /// <summary>
        /// Thực hiện nghiệp vụ khi thêm mới nhiều đối tượng
        /// </summary>
        /// <param name="entities">Danh sách các đối tượng</param>
        /// <returns>Thêm mới thành công || Thêm mới thất bại</returns>
        /// Created by: PTTAM (07/03/2023)
        public async Task<bool> InsertMultiService(List<Entity> entities)
        {
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
               
                using (MySqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        for (int i = 0; i < entities.Count; i++)
                        {
                            ValidateError(entities[i], cnn, tran); // gọi hàm validte
                        }
                        // kiểm tra biến isValidCustom và listErrors thỏa mãn điều kiện thì gọi repository để thực hiện việc thêm mới
                        if (isValidCustom == true && errorList.Count <= 0)
                        {
                            var res = await _repository.InsertMulti(entities, tran, cnn);
                            if (res)
                            {
                                tran.Commit();
                            }
                            else { tran.Rollback(); }
                            return res;
                        }
                        else // Ngược lại throw ra lỗi
                        {
                            tran.Rollback();
                            throw new ValidateException(errors);
                        }

                    }
                    catch
                    {
                        tran.Rollback();

                        throw;
                    }
                    finally { _repository.CloseMyConnection(); }
                }


                _repository.CloseMyConnection() ;

            }
            
        }

        /// <summary>
        /// Thực hiện nghiệp vụ khi lấy tất cả dữ liệu
        /// </summary>
        ///  Created by: PTTAM (07/03/2023)
        public async Task<IEnumerable> GetAllService()
        {
            var res = await _repository.GetAll();
            return res;
        }

        /// <summary>
        /// Thực hiện nghiệp vụ khi lấy đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính đối tượng</param>
        ///  Created by: PTTAM (07/03/2023)
        public async Task<Entity> GetByIdService(Guid entityId)
        {
        
            var res = await _repository.GetById(entityId);
            return res;
        }

        /// <summary>
        /// Thực hiện nghiệp vụ khi xóa đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính đối tượng</param>
        ///  Created by: PTTAM (07/03/2023)
        public async Task<bool> DeleteService(Guid entityId)
        {
            bool isSucess = true;
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {

                using (MySqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        var res = await _repository.Delete(entityId, cnn, tran);
                        if (!res)
                        {
                            isSucess = false;
                        }
                    }
                    catch (Exception)
                    {
                        isSucess = false;

                    }
                    finally
                    {
                        cnn.Close();
                    }
                }   
                _repository.CloseMyConnection();
            }
           

              
            return isSucess;
        }
        #region Validate

        /// <summary>
        /// Hàm validate 
        /// </summary>
        /// <param name="entity">Đối tượng cần validare</param>
        /// <param name="id">Khóa chính</param>
        /// Created by: PTTAM (07/03/2023)
        protected void ValidateError(Entity entity,MySqlConnection cnn, MySqlTransaction tran)
        {
            // thực hiện validate Dữ liệu
            //1. Check các trường trống
            CheckEmpty(entity);

            //2. Check độ dài tối đa cho phép của các trường
            CheckLimitLength(entity);

            //3. Check các trường bị trùng
            //if (isValidLength == true)
            //{
            //    CheckUnique(entity,cnn,tran);
            //}
            isValidCustom = ValidateCustom(entity);//Gọi đến hàm validate custom cho từng đối tượng

            // Kiểm tra list validate 
            if (errorList.Count > 0)
            {
                errors.Add("dataError", errorList);
                throw new ValidateException(errors);

            }

        }

        /// <summary>
        /// Kiểm tra các trường bắt buộc nhập không được trống
        /// </summary>
        /// <param name="entity">Đối tượng kiểm tra</param>
        ///  Created by: PTTAM (07/03/2023)
        private void CheckEmpty(Entity entity)
        {
            // lấy tất cả cá properties có attribute là NotEmpty
            var requireProperties = typeof(Entity).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(NotEmpty)));
            foreach (var property in requireProperties)
            {
                var value = property.GetValue(entity)?.ToString(); // lấy ra giá trị

                //  nếu giá trị của property trống
                if (value == null || value == string.Empty)
                {
                    // lấy ra attribute PropertyNameDisplay
                    var getVIName = property.GetCustomAttributes(typeof(PropertyNameDisplay), true).First();
                    // nếu PropertyNameDisplay tồn tại
                    if (getVIName != null)
                    {
                        var getDisplayName = (getVIName as PropertyNameDisplay)?.displayName; // lấy ra tên


                        //listErrors.Add(String.Format(MISAResource.IsErrorEmptyMultiple, getDisplayName));
                        object error = new
                        {
                            errorTitle =Resource.ErrorEmpty,
                            errorName = String.Format(Resource.IsErrorEmptyMultiple, getDisplayName)

                        };
                        errorList.Add(error);

                    }
                }
            }
        }

        /// <summary>
        /// KIểm tra các trường unique có bị trùng hay không
        /// </summary>
        /// <param name="entity">Đối tượng kiểm tra </param>
        ///  Created by: PTTAM (07/03/2023)
        private async void CheckUnique(Entity entity,MySqlConnection cnn, MySqlTransaction tran)
        {
            // lấy attribute Unique để kiểm trường duy nhất
            var propertyUnique = typeof(Entity).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(Unique)));


            var primaryKey = typeof(Entity).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PrimaryKey)));
            Guid userId = new Guid();
            foreach (var prop in primaryKey)
            {
                userId = (Guid)prop.GetValue(entity); // gán lại id của đối tượng

            }
            // lặp qua propertyUnique
            foreach (var property in propertyUnique)
            {
                var value = property.GetValue(entity).ToString(); // lấy ra giá trị
                var name = property.Name; // lấy ra tên 
                //Gọi đến hàm kiểm tra trường phải là duy nhất trong repository
                if (await _repository.CheckUnique(name, value, cnn,tran))
                {
                    // lấy ra attribute PropertyNameDisplay
                    var getVIName = property.GetCustomAttributes(typeof(PropertyNameDisplay), true).First();
                    if (getVIName != null)
                    {
                        var getDisplayName = (getVIName as PropertyNameDisplay)?.displayName; // lấy ra tên 

                        object error = new
                        {
                            UserId = userId,
                            errorTitle = Resource.ErrorDuplicate,
                            errorName = String.Format(Resource.IsErrorDuplicateMutiple, getDisplayName, value)

                        };
                        errorList.Add(error);


                    }
                }

            }
        }

        /// <summary>
        /// Kiểm tra độ dài tối đa cho phép của các trường
        /// </summary>
        /// <param name="entity">Đối tượng kiểm tra</param>
        /// Created by: PTTAM (07/03/2023)
        public void CheckLimitLength(Entity entity)
        {
            //  lấy tất cả cá properties có attribute là DataLength
            var propertiesLength = typeof(Entity).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(DataLength)));
            foreach (var prop in propertiesLength)
            {
                // Nếu value khác null
                if (prop.GetValue(entity) != null)
                {
                    var value = prop.GetValue(entity)?.ToString(); // lấy ra valua
                    var propLength = Attribute.GetCustomAttribute(prop, typeof(DataLength));
                    int length = (propLength as DataLength).Length; // lấy ra value
                    var getVIName = Attribute.GetCustomAttribute(prop, typeof(PropertyNameDisplay));
                    var getDisplayName = (getVIName as PropertyNameDisplay)?.displayName;
                    if (value != null && value.Count() > length)
                    {
                        if (prop.Name == typeof(Entity).Name + "Code")
                            isValidLength = false;

                        object error = new
                        {
                            errorTitle = Resource.ErrorLimitLength,
                            errorName = String.Format(Resource.IsErrorLimitLengthMultiple, getDisplayName, value, length)

                        };
                        errorList.Add(error);

                    }
                }

            }
        }
        /// <summary>
        /// sử dụng phương thức ảo để ghi đè hàm để validate theo từng đối tượng
        /// </summary>
        /// <param name="entity">Đối tượng cần kiểm tra </param>
        /// <returns>true: Không có lỗi, false: Có lỗi</returns> 
        /// Created by: PTTAM (07/03/2023)
        protected virtual bool ValidateCustom(Entity entity)
        {
            return true;
        }

        public async Task<object> GetEntityPaging(PagingParam param)
        {

            var res = await _repository.GetEntityPaging(param);
            return res;
        }

        #endregion

    }
}
