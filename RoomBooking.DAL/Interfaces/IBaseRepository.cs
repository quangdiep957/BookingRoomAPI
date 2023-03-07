using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.DAL.Interfaces
{
    public interface IBaseRepository<EntityCustom>
    {
        /// <summary>
        /// Lấy tất cả dữ liệu 
        /// </summary>
        /// <returns>Danh sách tất cả các bản ghi</returns>
        /// Created by: PTTAM (06/03/2023)
        public IEnumerable<EntityCustom> GetAll();

        /// <summary>
        /// Lấy thông tin đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// <returns>Đối tượng lấy được theo khóa chính</returns>
        /// Created by: PTTAM (06/03/2023)
        public EntityCustom GetById(Guid entityId);

        /// <summary>
        /// Thêm mới một đối tượng
        /// </summary>
        /// <param name="entity">Đối tượng cần thêm mới</param>
        /// <returns>
        ///"Thêm mới thành công" nếu không xảy ra lỗi
        /// "Thêm mới thất bại" (Trường hợp ngược lại)
        /// </returns>
        /// Created by: PTTAM (06/03/2023)
        public string Insert(EntityCustom entity);

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
        public string Update(EntityCustom entity, Guid entityId);

        /// <summary>
        /// Xóa đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// <returns>
        /// "Xóa thành công" nếu không xảy ra lỗi
        /// "Xóa thất bại" (Trường hợp ngược lại)
        /// </returns>
        /// Created by: PTTAM (06/03/2023)
        public string Delete(Guid entityId);

        /// <summary>
        /// Thực hiện thêm nhiều 
        /// </summary>
        /// <param name="listEntities">danh sách đối tượng cần thêm</param>
        /// <returns>Thêm mới thành công || Thêm mới thất bại</returns>
        /// Created by: PTTAM (06/03/2023)
        public string InsertMulti(List<EntityCustom> listEntities);

        /// <summary>
        /// Thực hiện kiểm tra trường dữ liệu phải là duy nhất
        /// </summary>
        /// <param name="entityName">Tên đối tượng</param>
        /// <param name="entityValue">Giá trị đối tượng</param>
        /// <param name="entityId">Khóa chính đối tượng</param>
        /// <returns> true: mã trùng, false: không có mã trùng</returns>
        /// Created by: PTTAM (06/03/2023)
        public bool CheckUnique(string entityName, object entityValue, Guid? entityId = null);
    }
}
