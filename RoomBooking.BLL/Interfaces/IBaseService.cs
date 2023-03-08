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
        /// Thực hiện validate lấy tất cả dữ liệu
        /// </summary>
        ///Created by: PTTAM (07/03/2023)
        public IEnumerable GetAllService();

        /// <summary>
        /// Thực hiện validate lấy thông tin đối tượng theo id
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// Created by: PTTAM (07/03/2023)
        public Entity GetByIdService(Guid entityId);
        /// <summary>
        /// Thực hiện validate khi thêm mới đối tượng
        /// </summary>
        /// <param name="entity">đối tượng validate</param>
        /// Created by: PTTAM (10/09/2022)
        public string InsertService(Entity entity);

        /// <summary>
        /// Thực hiện validate khi thêm nhiều đối tượng
        /// </summary>
        /// <param name="entities">Danh sách đối tượng</param>
        /// Created by: PTTAM (07/03/2023)
        public string InsertMultiService(List<Entity> entities);

        /// <summary>
        /// Thực hiện validate khi sửa đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// <param name="entity">Đối tượng validate khi sửa</param>
        /// Thực hiện validate khi thêm mới đối tượng
        /// Created by: PTTAM (07/03/2023)
        public string UpdateService(Guid entityId, Entity entity);

        /// <summary>
        /// Thực hiện validate khi xóa đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// Created by: PTTAM (07/03/2023)
        public string DeleteService(Guid entityId);
    }
}
