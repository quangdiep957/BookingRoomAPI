using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using RoomBooking.Common.Entities;
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
        public async Task<Object> GetPaging(int pageSize, int pageIndex, string? keyWord, Guid? roleId)
        {

            var storeName = "Proc_GetRoomPaging"; // Tên của thủ thục
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@PageSize", pageSize); //input: Số bản ghi/trang
            dynamicParameters.Add("@PageIndex", pageIndex);//input: Trang hiện tại
            dynamicParameters.Add("@RoomFilter", keyWord); //input: Từ khóa
            dynamicParameters.Add("@RoleId", roleId); //input: Khóa chính vai trò
            dynamicParameters.Add("@TotalRecord", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số bản ghi
            dynamicParameters.Add("@TotalPage", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số trang
            //2. Lấy dữ liệu
            var employees = await _sqlConnection.QueryAsync<Room>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);

            int totalRecord = dynamicParameters.Get<int>("@TotalRecord"); // Lấy ra tổng số bản ghi
            int totalPage = dynamicParameters.Get<int>("@TotalPage"); // Lấy ra tổng số trang
            int startRecord = pageSize * (pageIndex - 1) + 1; // Bản ghi bắt đầu của trang hiện tại
            int endRecord = pageSize * (pageIndex - 1) + pageSize; // Bản ghi kết thúc của trang hiện tại

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
                CurrentPage = pageIndex,
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
        public override async Task<bool> InsertMulti(List<BookingRoom> listRoom)
        {

            base._sqlConnection.Open();
            //1. Thực hiện insert Room
            int countRoom = 0; // biến đếm khi thực hiện thêm người dùng
            string sqlQuery = GetAllBindingNames(listRoom[0]); // câu truy vấn lấy ra tên trường của Room
            DynamicParameters dynamicParameters = new DynamicParameters();
            MySqlTransaction transaction = _sqlConnection.BeginTransaction();


            List<Room> dataRoom = (List<Room>)await _sqlConnection.QueryAsync<Room>("SELECT * FROM Room;", transaction: transaction);

            List<TimeSlot> slotTime = (List<TimeSlot>)await _sqlConnection.QueryAsync<TimeSlot>("SELECT * FROM TimeSlot;", transaction: transaction);
            foreach (BookingRoom room in listRoom)
            {
                var itemRoom = dataRoom.Where(x => x.RoomCode == room.Room).FirstOrDefault();

                var itemTimeSlot = slotTime.Where(x => x.TimeSlotName == room.Times).FirstOrDefault();
                room.BookingRoomID = Guid.NewGuid();
                room.RoomID = itemRoom.RoomID;
                room.TimeSlotID = itemTimeSlot.TimeSlotID;
                room.Subject = "Lịch học tuần " + room.Week;
                room.UserID = new Guid("d06fc80f-c4d4-11ed-b43e-f8b46ac25bb6");
                room.YearPlan = room.DateBooking.Year;
                room.DayOfWeek = room.DayOfWeek == "1" ? "CN" : room.DayOfWeek;


            }

            for (int i = 0; i < listRoom.Count; i++)
            {
                sqlQuery += GetAllBindingValues(listRoom[i], i, dynamicParameters); // Thực hiện buid câu truy vấn 
                countRoom++;
            }
            sqlQuery = sqlQuery[..^1]; // bỏ dấu ',' cuối cùng
            var rowEffect = _sqlConnection.Execute(sqlQuery, dynamicParameters, transaction: transaction);

            // nếu số bản ghi thay đổi < countRoom
            if (rowEffect < countRoom)
            {
                transaction.Rollback(); // rollback 
                CloseConnection();

                return false; // thêm thất bại
            }

            transaction.Commit();
            CloseConnection();
            return true;



        }
        public async Task<object> CheckRoom(List<BookingRoom> listRoom)
        {

            base._sqlConnection.Open();
            string sqlQuery = GetAllBindingNames(listRoom[0]); // câu truy vấn lấy ra tên trường của Room
            DynamicParameters dynamicParameters = new DynamicParameters();
            MySqlTransaction transaction = _sqlConnection.BeginTransaction();


            List<Room> dataRoom = (List<Room>)await _sqlConnection.QueryAsync<Room>("SELECT * FROM Room;", transaction: transaction);

            List<BookingRoom> listRoomNotIn = new();
            foreach (BookingRoom room in listRoom)
            {
                 
                var itemRoom = dataRoom.Where(x => x.RoomCode == room.Room).FirstOrDefault();
               
                if (itemRoom == null)
                {
                    listRoomNotIn.Add(room);
                }

            }
           var res= listRoomNotIn.Select(x=>x.Room).Distinct().ToList();
            CloseConnection();
            return res;
        }
    }
}
