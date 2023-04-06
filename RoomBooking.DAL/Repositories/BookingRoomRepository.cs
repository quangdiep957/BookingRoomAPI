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
    public class BookingRoomRepository : BaseRepository<BookingRoom>, IBookingRoomRepository
    {
        public BookingRoomRepository(IConfiguration configuration) : base(configuration)
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
           
            var storeName = "Proc_GetSchedulerRoomPaging"; // Tên của thủ thục
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@PageSize", param.pageSize); //input: Số bản ghi/trang
            dynamicParameters.Add("@PageIndex", param.pageIndex);//input: Trang hiện tại
            dynamicParameters.Add("@ListDate", param.week); //input: Mảng ngày
            dynamicParameters.Add("@RoomFilter", param.keyWord); //input: Từ khóa
            dynamicParameters.Add("@RoomID", param.roomID); //input: Khóa chính phòng học
            dynamicParameters.Add("@BuildingID", param.buildingID); //input: Khóa chính tòa
            dynamicParameters.Add("@TimeSlotID", param.timeSlotID); //input: Khóa chính thời gian
            dynamicParameters.Add("@Type", param.type); //input: Khóa chính vai trò
            dynamicParameters.Add("@TotalRecord", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số bản ghi
            dynamicParameters.Add("@TotalPage", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số trang
           
            //2. Lấy dữ liệu
            var employees = await cnn.QueryAsync(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);

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

        /// <summary>
        /// Thực hiện việc thêm nhiều người dùng
        /// </summary>
        /// <param name="listRoom">Danh sách người dùng</param>
        /// <returns></returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public async override Task<bool> InsertMulti(List<BookingRoom> listRoom, MySqlTransaction transaction, MySqlConnection cnn)
        {
            bool isSuccess = true;
            //1. Thực hiện insert Room
            int countRoom = 0; // biến đếm khi thực hiện thêm người dùng
            string sqlQuery = GetAllBindingNames(listRoom[0]); // câu truy vấn lấy ra tên trường của Room
            DynamicParameters dynamicParameters = new DynamicParameters();

            for (int i = 0; i < listRoom.Count; i++)
            {
                sqlQuery += GetAllBindingValues(listRoom[i], i, dynamicParameters); // Thực hiện buid câu truy vấn 
                countRoom++;
            }
            sqlQuery = sqlQuery[..^1]; // bỏ dấu ',' cuối cùng
            var rowEffect = cnn.Execute(sqlQuery, dynamicParameters, transaction: transaction);

            // nếu số bản ghi thay đổi < countRoom
            if (rowEffect < countRoom)
            {

                isSuccess = false; // thêm thất bại
            }

            return isSuccess;



        }


        /// <summary>
        /// Thực hiện lấy danh sách yêu cầu đặt phòng chờ duyệt
        /// </summary>
        /// <param name="param"></param>
        /// PTTAM 04.01.2023
        public async Task<object> GetPagingRequest(PagingParam param, MySqlConnection cnn)
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
