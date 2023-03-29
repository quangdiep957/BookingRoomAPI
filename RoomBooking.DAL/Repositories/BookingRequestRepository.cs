using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.DAL.Repositories
{
    public class BookingRequestRepository : BaseRepository<BookingRequest>, IBookingRequestRepository
    {
        public BookingRequestRepository(IConfiguration configuration) : base(configuration)
        {
        }
        /// <summary>
        /// Thực hiện việc phân trang
        /// </summary>
        /// <param name="pageSize">Số bản ghi/ 1 trang</param>
        /// <param name="pageIndex">Trang số bao nhiêu</param>
        /// <param name="keyWord">Điều kiện lọc dữ liệu/param>
        /// <param name="roleId">Khóa chính của vai trò /param>
        /// <returns>Object chứa những thông tin cần thiết</returns>
        /// Created by: PTTAM (07/03/2023)
        public async Task<Object> GetPaging(PagingParam param, MySqlConnection cnn)
        {

            var storeName = "Proc_GetPagingRequestBooking"; // Tên của thủ thục
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@PageSize", param.pageSize); //input: Số bản ghi/trang
            dynamicParameters.Add("@PageIndex", param.pageIndex);//input: Trang hiện tại
            dynamicParameters.Add("@WeekID", param.weekID); //input: Khóa chính phòng học
            dynamicParameters.Add("@KeyWord", param.keyWord); //input: Khóa chính phòng học
            dynamicParameters.Add("@TotalRecord", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số bản ghi
            dynamicParameters.Add("@TotalPage", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số trang

            //2. Lấy dữ liệu
            var employees = await cnn.QueryAsync<Object>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);

            int totalRecord = dynamicParameters.Get<int>("@TotalRecord"); // Lấy ra tổng số bản ghi
            int totalPage = dynamicParameters.Get<int>("@TotalPage"); // Lấy ra tổng số trang
            int startRecord = param.pageSize * (param.pageIndex - 1) + 1; // Bản ghi bắt đầu của trang hiện tại
            int endRecord = param.pageSize * (param.pageIndex - 1) + param.pageSize; // Bản ghi kết thúc của trang hiện tại

            if (endRecord > totalRecord) // nếu bản ghi kết thúc > tổng số bản ghi
            {
                endRecord = totalRecord; // gán bản ghi kết thúc = tổng số bản ghi
            }

            // nếu bản ghi bắt đầu của trang > bản ghi kết thúc
            if (startRecord > endRecord)
            {
                startRecord = endRecord;// gán bản ghi bắt đầu = bản ghi kết thúc
            }
            return new
            {
                TotalPage = totalPage,
                TotalRecord = totalRecord,
                CurrentPage = param.pageIndex,
                StartRecord = startRecord,
                EndRecord = endRecord,
                Data = employees
            };

        }
    }
}
