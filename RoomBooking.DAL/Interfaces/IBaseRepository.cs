using Microsoft.EntityFrameworkCore.ChangeTracking;
using MySqlConnector;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.DAL.Interfaces
{
    public interface IBaseRepository<EntityCustom>
    {
        /// <summary>
        /// Phân trang
        /// </summary>
        /// <returns>Danh sách tất cả các bản ghi</returns>
        /// Created by: PTTAM (19/03/2023)
        public Task<Object> GetEntityPaging(PagingParam param);
        /// <summary>
        /// Lấy tất cả dữ liệu 
        /// </summary>
        /// <returns>Danh sách tất cả các bản ghi</returns>
        /// Created by: PTTAM (06/03/2023)
        public Task<IEnumerable<EntityCustom>> GetAll();

        /// <summary>
        /// Lấy thông tin đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// <returns>Đối tượng lấy được theo khóa chính</returns>
        /// Created by: PTTAM (06/03/2023)
        public Task<EntityCustom> GetById(Guid entityId);

        /// <summary>
        /// Thêm mới một đối tượng
        /// </summary>
        /// <param name="entity">Đối tượng cần thêm mới</param>
        /// <returns>
        ///"Thêm mới thành công" nếu không xảy ra lỗi
        /// "Thêm mới thất bại" (Trường hợp ngược lại)
        /// </returns>
        /// Created by: PTTAM (06/03/2023)
        public Task<bool> Insert(EntityCustom entity,MySqlConnection cnn,MySqlTransaction transaction);

        /// <summary>
        /// Sửa một đối tượng theo khóa chính
        /// </summary>
        /// <param name="entity">Đối tượng cần sửa</param>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        ///  <returns>
        /// "Sửa thành công" nếu không xảy ra lỗi
        /// "Sửa thất bại" (Trường hợp ngược lại)
        ///</returns>
        ///Created by: PTTAM (06/03/2023)
        public Task<bool> Update(EntityCustom entity, Guid entityId,MySqlConnection cnn,MySqlTransaction tran);

        /// <summary>
        /// Xóa đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// <returns>
        /// "Xóa thành công" nếu không xảy ra lỗi
        /// "Xóa thất bại" (Trường hợp ngược lại)
        /// </returns>
        /// Created by: PTTAM (06/03/2023)
        public Task<bool> Delete(Guid entityId, MySqlConnection cnn, MySqlTransaction transaction);

        /// <summary>
        /// Thực hiện thêm nhiều 
        /// </summary>
        /// <param name="listEntities">danh sách đối tượng cần thêm</param>
        /// <returns>Thêm mới thành công || Thêm mới thất bại</returns>
        /// Created by: PTTAM (06/03/2023)
        public Task<bool> InsertMulti(List<EntityCustom> listEntities, MySqlTransaction tran, MySqlConnection cnn);

        /// <summary>
        /// Thực hiện kiểm tra trường dữ liệu phải là duy nhất
        /// </summary>
        /// <param name="entityName">Tên đối tượng</param>
        /// <param name="entityValue">Giá trị đối tượng</param>
        /// <param name="entityId">Khóa chính đối tượng</param>
        /// <returns> true: mã trùng, false: không có mã trùng</returns>
        /// Created by: PTTAM (06/03/2023)
        public Task<bool> CheckUnique(string entityName, object entityValue, MySqlConnection cnn,MySqlTransaction tran,Guid? entityId = null);
        public MySqlConnection GetOpenConnection();
        public void CloseMyConnection();
    }
}
