using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using RoomBooking.Common.AttributeCustom;
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
        public async Task<ParamSchedulerBooking> GetPaging(PagingParam param, MySqlConnection cnn)
        {
           
            var storeName = "Proc_GetSchedulerBooking"; // Tên của thủ thục
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@CapacityMin", param.capacityMin); //input: Số bản ghi/trang
            dynamicParameters.Add("@CapacityMax", param.capacityMax);//input: Trang hiện tại
            dynamicParameters.Add("@ListEquipment", param.equipmentIDs); //input: Từ khóa
            dynamicParameters.Add("@RoomID", param.roomID); //input: Khóa chính phòng học
            dynamicParameters.Add("@BuildingID", param.buildingID); //input: Khóa chính tòa
            dynamicParameters.Add("@UserID", param.userID); //input: Khóa chính thời gian
           
            //2. Lấy dữ liệu
            var data = await cnn.QueryMultipleAsync(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);

            ParamSchedulerBooking result = new();

            result.bookings= (List<SchedulerBooking>)await data.ReadAsync<SchedulerBooking>(); 
            result.rooms= (List<Room>)await data.ReadAsync<Room>();
            return result;

        }
        /// <summary>
        /// Thực hiện lấy giá trị của đối tượng và add vào parameter
        /// </summary>
        /// <param name="entity">Đối tượng</param>
        /// <param name="index">Vị trí của đối tượng</param>
        /// <param name="parameters">Parameter</param>
        /// <returns>Chuỗi paramerter</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public string GetAllBindingValue(TimeBooking entity, int index, DynamicParameters parameters)
        {
            // lấy tất cả cá properties

            var properties = typeof(TimeBooking).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ForBinding)));
            // Buid 
            var allValuesParam = "( ";
            foreach (var property in properties)
            {
                // lấy giá trị
                allValuesParam += "@" + property.Name + index + " ,";


            }
            allValuesParam = allValuesParam[..^1];
            allValuesParam += " ),";
            foreach (var property in properties)
            {

                // lấy giá trị
                var currentValue = property.GetValue(entity);
                parameters.Add($"@{property.Name}" + index, currentValue);


            }

            // bỏ kí tự ',' cuối cùng
            return allValuesParam;

        }

        /// <summary>
        /// Thực hiện lấy tên trường của đối tượng khi thêm mới
        /// </summary>
        /// <param name="entity">Đối tượng</param>
        /// <returns>Chuỗi chứa câu lệnh insert chứa tên các trường</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        protected string GetAllBindingName(TimeBooking entity)
        {
            // lấy tất cả cá properties ForBinding

            var properties = typeof(TimeBooking).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ForBinding)));
            // Buid câu truy vấn
            var allNames = $"INSERT INTO {typeof(TimeBooking).Name}(";
            foreach (var property in properties)
            {
                // lấy tên của prop 
                allNames += property.Name + " ,";

            }

            allNames = allNames[..^1]; // loại bỏ kí tự ',' cuối cùng

            allNames += " )Values";
            return allNames;
        }
        /// <summary>
        /// Thực hiện việc thêm nhiều lịch đặt phog
        /// </summary>
        /// <param name="listRoom">Danh sách người dùng</param>
        /// <returns></returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public async Task<bool> InsertMultiTimeBooking(List<TimeBooking> listRoom, MySqlTransaction transaction, MySqlConnection cnn)
        {
            bool isSuccess = true;
            //1. Thực hiện insert Room
            int countRoom = 0; // biến đếm khi thực hiện thêm người dùng
            string sqlQuery = GetAllBindingName(listRoom[0]); // câu truy vấn lấy ra tên trường của Room
            DynamicParameters dynamicParameters = new DynamicParameters();

            for (int i = 0; i < listRoom.Count; i++)
            {
                sqlQuery += GetAllBindingValue(listRoom[i], i, dynamicParameters); // Thực hiện buid câu truy vấn 
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
        /// Thực hiện việc thêm nhiều lịch đặt phog
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
        /// Thực hiện xóa đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính đối tượng</param>
        /// <returns>Xóa thành công || Xóa thất bại</returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public async Task<bool> DeleteRecord(Guid entityId, string tablename ,MySqlConnection cnn, MySqlTransaction transaction)
        {
            bool isSucess = true;
            try
            {
                var storeDelete = "Proc_Delete_Record";
                var properties = typeof(TimeBooking).GetProperties().FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(KeyDelete)));
                DynamicParameters paramId = new DynamicParameters();
                paramId.Add("@EntityId", entityId);
                paramId.Add("@TableName", tablename);
                paramId.Add("@Property", properties.Name);

                var res = await cnn.ExecuteAsync(storeDelete, paramId, transaction, commandType: System.Data.CommandType.StoredProcedure);

            }
            catch (Exception)
            {

                isSucess = false;
            }


            return isSucess;
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
            dynamicParameters.Add("@UserID", param.userID); //input: Khóa chính phòng học
            dynamicParameters.Add("@KeyWord", param.keyWord); //input: Khóa chính phòng học
            dynamicParameters.Add("@RoleOption", param.roleOption);
            dynamicParameters.Add("@TotalRecord", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số bản ghi
            dynamicParameters.Add("@TotalPage", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số trang

            //2. Lấy dữ liệu
            List<BookingRoom> data = (List<BookingRoom>)await cnn.QueryAsync<BookingRoom>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);

            int totalRecord = dynamicParameters.Get<int>("@TotalRecord"); // Lấy ra tổng số bản ghi
            int totalPage = dynamicParameters.Get<int>("@TotalPage"); // Lấy ra tổng số trang
            int startRecord = (int)(param.pageSize * (param.pageIndex - 1) + 1); // Bản ghi bắt đầu của trang hiện tại
            int endRecord = (int)(param.pageSize * (param.pageIndex - 1) + param.pageSize); // Bản ghi kết thúc của trang hiện tại

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
                Data = data
            };
        }

        /// <summary>
        /// hàm getpaging lịch sử đặt phòng
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override async Task<Object> GetEntityPaging(PagingParam param)
        {
            if (_sqlConnection.State != ConnectionState.Open)
            {
                _sqlConnection.Open();
            }
            var orConditions = new List<string>();
            string whereClause = $"u.UserID = '{param.userID}'";
            // Kiểm tra xem có lấy theo chi tiết theo BookingRoomID không
            if(param != null && param.bookingRoomID != Guid.Empty)
            {
                whereClause = whereClause + $" AND b.BookingRoomID = '{param.bookingRoomID}'";
            }    

            if (param.keyWord != null)
            {
                orConditions.Add($"u.FullName LIKE '%{param.keyWord}%'");
                orConditions.Add($"r.RoomName LIKE '%{param.keyWord}%'");
                orConditions.Add($"b.Description LIKE '%{param.keyWord}%'");
            }

            if (orConditions.Count > 0)
            {
                whereClause = $"({string.Join(" OR ", orConditions)})";
            }
            var parameters = new DynamicParameters();
            parameters.Add("@v_Offset", (param.pageIndex - 1) * param.pageSize);
            parameters.Add("@v_Limit", param.pageSize);
            parameters.Add("@v_Sort", "r.ModifiedDate DESC");
            parameters.Add("@v_Where", whereClause);
            var sqlCommand = "Proc_History_Booking_GetPaging";
            var data = await _sqlConnection.QueryAsync(sqlCommand, param: parameters, commandType: System.Data.CommandType.StoredProcedure);
            int totalRecords = 0;
            int totalPages = 0;
            int startRecord = 0;
            int endRecord = 0;
            if (data != null && data.Count() > 0)
            {
                totalRecords = data.Count();
                totalPages = (int)Math.Ceiling((decimal)(totalRecords / param.pageSize));
                startRecord = (int)(param.pageSize * (param.pageIndex - 1) + 1); // Bản ghi bắt đầu của trang hiện tại
                endRecord = (int)(param.pageSize * (param.pageIndex - 1) + param.pageSize); // Bản ghi kết thúc của trang hiện tại

                if (endRecord > totalRecords) // nếu bản ghi kết thúc > tổng số bản ghi
                {
                    endRecord = totalRecords; // gán bản ghi kết thúc = tổng số bản ghi
                }

                // nếu bản ghi bắt đầu của trang > bản ghi kết thúc
                if (startRecord > endRecord)
                {
                    startRecord = endRecord;// gán bản ghi bắt đầu = bản ghi kết thúc
                }
            }
            CloseMyConnection();
            return new
            {
                TotalPage = totalPages,
                TotalRecord = totalRecords,
                CurrentPage = param.pageIndex,
                StartRecord = startRecord,
                EndRecord = endRecord,
                Data = data
            };
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
        public async Task<ParamReport> GetParamReport(Guid id, MySqlConnection cnn)
        {

            var storeName = "Proc_GetReportBookingRoom"; // Tên của thủ thục
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@BookingRoomID", id);

            //2. Lấy dữ liệu
            var bookingRoom = await cnn.QueryFirstOrDefaultAsync<ParamReport>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);
            return bookingRoom;

        }
        /// <summary>
        /// Thực hiện lấy đối tượng theo khóa chính
        /// </summary>
        /// <param name="entityId">Khóa chính của đối tượng</param>
        /// <returns>Đối tượng cần lấy </returns>
        ///  CretedBy: PTTAM (07/03/2023)
        public override async Task<BookingRoom> GetById(Guid entityId)
        {
            BookingRoom booking = new();
            if (_sqlConnection.State != ConnectionState.Open)
            {
                _sqlConnection.Open();
            }
            var res = await _sqlConnection.QueryAsync<BookingRoom>("SELECT * FROM BookingRoom");
            booking = res.FirstOrDefault(x => x.BookingRoomID == entityId);

            

            return booking;
        }

        public async Task<object> GetPagingHistory(PagingParam param, MySqlConnection cnn)
        {
            var storeName = "Proc_GetPagingHistory"; // Tên của thủ thục
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@PageSize", param.pageSize); //input: Số bản ghi/trang
            dynamicParameters.Add("@PageIndex", param.pageIndex);//input: Trang hiện tại
            dynamicParameters.Add("@UserID", param.userID); //input: Khóa chính phòng học
            dynamicParameters.Add("@KeyWord", param.keyWord); //input: Khóa chính phòng học
            dynamicParameters.Add("@TotalRecord", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số bản ghi
            dynamicParameters.Add("@TotalPage", DbType.Int32, direction: ParameterDirection.Output); // output: tổng số trang

            //2. Lấy dữ liệu
            List<BookingRoom> data = (List<BookingRoom>)await cnn.QueryAsync<BookingRoom>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure);

            int totalRecord = dynamicParameters.Get<int>("@TotalRecord"); // Lấy ra tổng số bản ghi
            int totalPage = dynamicParameters.Get<int>("@TotalPage"); // Lấy ra tổng số trang
            int startRecord = (int)(param.pageSize * (param.pageIndex - 1) + 1); // Bản ghi bắt đầu của trang hiện tại
            int endRecord = (int)(param.pageSize * (param.pageIndex - 1) + param.pageSize); // Bản ghi kết thúc của trang hiện tại

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
                Data = data
            };
        }
    }
}
