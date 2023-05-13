using RoomBooking.Common.Entities.Params;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Interfaces
{
    public interface IBaseService<Entity>
    {

        /// <summary>
        /// Thực hiện nghiệp vụ khi phân trang 
        /// </summary>
        /// <param name="pageSize">Số bản ghi trên 1 trang</param>
        /// <param name="pageIndex">Tổng số trang</param>
        /// <param name="keyWord">Dữ liệu chọn lọc để tìm kiếm</param>
        /// <returns>Object: chứa pageSize, pageIndex, data</returns>
        /// Created by: PTTAM (07/03/2023)
        public  Task<object> GetEntityPaging(PagingParam param);
        /// <summary>
        /// Thực hiện validate lấy tất cả dữ liệu
        /// </summary>
        ///Created by: PTTAM (07/03/2023)
        public  Task<IEnumerable>  GetAllService();

        /// <summary>
        /// Thực hiện validate lấy thông tin đối tượng theo id
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// Created by: PTTAM (07/03/2023)
        public  Task<Entity> GetByIdService(Guid entityId);
        /// <summary>
        /// Thực hiện validate khi thêm mới đối tượng
        /// </summary>
        /// <param name="entity">đối tượng validate</param>
        /// Created by: PTTAM (10/09/2022)
        public  Task<bool> InsertService(Entity entity);

        /// <summary>
        /// Thực hiện validate khi thêm nhiều đối tượng
        /// </summary>
        /// <param name="entities">Danh sách đối tượng</param>
        /// Created by: PTTAM (07/03/2023)
        public  Task<bool> InsertMultiService(List<Entity> entities);

        /// <summary>
        /// Thực hiện validate khi sửa đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// <param name="entity">Đối tượng validate khi sửa</param>
        /// Thực hiện validate khi thêm mới đối tượng
        /// Created by: PTTAM (07/03/2023)
        public  Task<bool> UpdateService(Guid entityId, Entity entity);

        /// <summary>
        /// Thực hiện validate khi xóa đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// Created by: PTTAM (07/03/2023)
        public  Task<bool> DeleteService(Guid entityId);

        public Task SendNotify(string ID, string notify, DateTime time, bool? sendAdmin);
    }
}
