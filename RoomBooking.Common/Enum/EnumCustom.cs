using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Enum
{
    /// <summary>
    /// HTTP Status code
    /// </summary>
    public enum HTTPStatusCode
    {
        /// <summary>
        /// Phản hồi thành công
        /// </summary>
        SuccessResponse = 200,

        /// <summary>
        /// Lỗi phía Client
        /// </summary>
        ClientError = 400,

        /// <summary>
        /// Lỗi phía Server
        /// </summary>
        ServeError = 500

    }

    /// <summary>
    /// Enum check thêm, xóa, không thay đổi
    /// </summary>
    public enum UpdateMode
    {
        /// <summary>
        /// Trạng thái thêm
        /// </summary>
        InsertMode = 1,

        /// <summary>
        /// Trạng thái Delete
        /// </summary>
        DeleteMode = 2,

        /// <summary>
        /// Không thay đổi
        /// </summary>
        Unchanged = 0
    }

    public enum OptionRequest
    {
        // Chờ duyệt
        Await=1,
        // Đồng ý
        Approve=2,
        // Từ chối
        Reject=3
    }

    public enum StatusRoom
    {
        // Phòng trống
        Empty=1,
        // Phòng đang được sử dụng
        Active=2,
        // Bảo trì
        Maintenance=3
    }
}
