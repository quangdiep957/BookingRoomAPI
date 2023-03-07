using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Exception
{
    public class HttpResponseException
    {
        public HttpResponseException(int statusCode, object? value = null) =>
       (StatusCode, Value) = (statusCode, value);

        /// <summary>
        /// 200 - Thực hiện thành công
        /// 204 - Không có dữ liệu
        /// 400 - Lỗi do đầu vào
        /// 500 - Lỗi do hệ thống
        /// </summary>
        /// created by: PTTAM (06/03/2023)
        public int StatusCode { get; }


        public object? Value { get; }
    }
}
