using Dapper;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MySqlConnector;
using Newtonsoft.Json;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using RoomBooking.Common.Enum;
using RoomBooking.Common.Resources;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using XAct;
using XAct.Domain.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RoomBooking.BLL.Services
{
    public class BookingRoomService : BaseService<BookingRoom>, IBookingRoomService
    {
        IBookingRoomRepository _repository;
        const int cancel = 4;
        ITimeBookingRepository _repoTimeBooking;
        private readonly IMemoryCache _cache;
        public BookingRoomService(IBookingRoomRepository repository, ITimeBookingRepository repoTimeBooking, IMemoryCache cache) : base(repository)
        {
            _repository = repository;
            _repoTimeBooking = repoTimeBooking;
            _cache = cache;
        }


        /// <summary>
        /// Đọc file excell
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>Object</returns>
        public async Task<Object> ReadExcelFile(string filePath)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            List<BookingRoom> scheduleItems = new List<BookingRoom>();

            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        UseColumnDataType = true,
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true,
                        }
                    });

                    var dataTable = dataSet.Tables[0];

                    foreach (DataRow row in dataTable.Rows)
                    {
                        scheduleItems.Add(new BookingRoom
                        {
                            Building = row["Tòa nhà"].ToString(),
                            Room = row["Phòng học"].ToString(),
                            DayOfWeek = row["Thứ"].ToString(),
                            Time = row["Thời gian"].ToString(),
                            MorningFreePeriod = row["Tiết trống sáng"].ToString(),
                            AfternoonFreePeriod = row["Tiết trống chiều"].ToString(),
                            EveningFreePeriod = row["Tiết trống tối"].ToString(),
                            Week = row["Tuần"].ToString(),
                        });
                    }
                }
            }
            // Thực hiện convert lại dữ liệu
            List<BookingRoom> lst = ConvertScheduleList(scheduleItems);


            Object result = new();
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {

                using (MySqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        List<BookingError> errors = new List<BookingError>();
                        List<TimeBooking> lstTimeBooking = new();
                        int count = lst.Count;
                        bool checkRoom = await CheckRoom(lst, cnn, tran, errors, lstTimeBooking);
                        if (checkRoom && !errors.Any())
                        {
                            var resBookingRoom = await _repository.InsertMulti(lst, tran, cnn);
                            var resTimeBooking = await _repoTimeBooking.InsertMulti(lstTimeBooking, tran, cnn);
                            tran.Commit();
                            var res = (resBookingRoom == true && resTimeBooking == true) ? true : false;
                            result = new
                            {

                                IsSuccess = res,
                                Data = errors,
                                Count = count

                            };
                        }
                        else
                        {
                            tran.Rollback();
                            result = new
                            {

                                IsSuccess = false,
                                Data = errors,
                                Count = count


                            };
                        }


                    }
                    catch { tran.Rollback(); }
                    finally { _repository.CloseMyConnection(); }
                }

                _repository.CloseMyConnection();

            }

            return result;


        }

        /// <summary>
        /// Thực hiện validate dữ liệu phòng trước khi import
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="cnn"></param>
        /// <param name="tran"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        /// PTTAM 04/06/2023
        private async Task<bool> CheckRoom(List<BookingRoom> lst, MySqlConnection cnn, MySqlTransaction tran, List<BookingError> errors, List<TimeBooking> lstTimeBooking)
        {
            // Thực hiện kiểm tra phòng đã được đặt chưa
            bool checkRoom = true;
            List<Room> dataRoom = (List<Room>)await cnn.QueryAsync<Room>("SELECT * FROM Room;", transaction: tran);
            List<TimeSlot> slotTime = (List<TimeSlot>)await cnn.QueryAsync<TimeSlot>("SELECT * FROM TimeSlot;", transaction: tran);
            List<BookingRoom> listRoomNotIn = new();

            foreach (BookingRoom room in lst)
            {
                var itemRoom = dataRoom.Where(x => x.RoomCode == room.Room).FirstOrDefault();
                if (itemRoom == null)
                {

                    listRoomNotIn.Add(room);
                }
                else
                {
                    var itemTimeSlot = slotTime.Where(x => x.TimeSlotName == room.Times).FirstOrDefault();
                    room.BookingRoomID = Guid.NewGuid();
                    room.RoomID = itemRoom.RoomID;
                    room.Subject = "Lịch học tuần " + room.Week;
                    room.UserID = Guid.Empty;
                    room.YearPlan = room.StartDate.Year;
                    room.DayOfWeek = room.DayOfWeek == "1" ? "CN" : room.DayOfWeek;
                    room.TimeSlots = itemTimeSlot.TimeSlotID.ToString();
                    room.StatusBooking = (int?)StatusBookingRoom.Browse;
                    lstTimeBooking.Add(new TimeBooking
                    {
                        BookingRoomID = room.BookingRoomID,
                        TimeSlotID = itemTimeSlot.TimeSlotID
                    });
                }

            }

            if (listRoomNotIn != null && listRoomNotIn.Any())
            {
                foreach (var room in listRoomNotIn.Select(x => x.Room).Distinct().ToList())
                {
                    errors.Add(new BookingError
                    {
                        Error = "Không có dữ liệu",
                        DescriptionError = $"Không có phòng {room}."
                    });
                }
                checkRoom = false;
            }

            // Thực hiện kiểm tra phòng đã được đặt chưa
            checkRoom = await CheckRoomIsUsed(lst, cnn, tran, errors);
            return checkRoom;
        }

        /// <summary>
        ///  Thực hiện kiểm tra phòng đã được đặt chưa
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="cnn"></param>
        /// <param name="tran"></param>
        /// <param name="errors"></param>
        /// <param name="checkRoom"></param>
        /// <returns></returns>
        private static async Task<bool> CheckRoomIsUsed(List<BookingRoom> lst, MySqlConnection cnn, MySqlTransaction tran, List<BookingError> errors)
        {
            bool checkRoom = true;
            List<BookingRoom> lstBookingRoom = (List<BookingRoom>)await cnn.QueryAsync<BookingRoom>("SELECT * FROM BookingRoom;", transaction: tran);
            lstBookingRoom = lstBookingRoom.Where(x => x.StatusBooking != (int)StatusBookingRoom.Cancel).ToList();
            List<Room> listRoom = (List<Room>)await cnn.QueryAsync<Room>("SELECT * FROM Room;", transaction: tran);
            List<TimeSlot> lstTimeSlot = (List<TimeSlot>)await cnn.QueryAsync<TimeSlot>("SELECT * FROM TimeSlot;", transaction: tran);
            var data = lst.Where(x => x.BookingRoomID != Guid.Empty).ToList();
            string roomName = "";
            int timeName = 0;
            foreach (BookingRoom room in data)
            {
                // Tách chuỗi TimeSlotID
                string[] timeIDs = room.TimeSlots.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                // For từng dòng
                foreach (var item in timeIDs)
                {
                    var itemRoomStartDate = lstBookingRoom.FirstOrDefault(x => x.RoomID == room.RoomID
                    && x.BookingRoomID != room.BookingRoomID &&
                    x.TimeSlots.Contains(item) && x.StartDate.ToString("yyyy/MM/dd") == room.StartDate.ToString("yyyy/MM/dd"));
                    var itemRoomEndDate = lstBookingRoom.FirstOrDefault(x => x.RoomID == room.RoomID
                         && x.BookingRoomID != room.BookingRoomID &&
                    x.TimeSlots.Contains(item) && x.StartDate.ToString("yyyy/MM/dd") == room.StartDate.ToString("yyyy/MM/dd"));

                    if (room.StartDate == room.EndDate)
                    {
                        if (itemRoomStartDate != null)
                        {
                            roomName = listRoom.FirstOrDefault(x => x.RoomID == itemRoomStartDate.RoomID).RoomName;
                            timeName = lstTimeSlot.FirstOrDefault(x => item == x.TimeSlotID.ToString()).TimeSlotName;
                            errors.Add(new BookingError
                            {
                                Error = "Đã có dữ liệu",
                                DescriptionError = $"{roomName} ca {timeName} ngày {room.StartDate.ToString("dd/MM/yyyy")} đã được đặt."
                            });
                            checkRoom = false;
                        }
                    }
                    else
                    {
                        if (itemRoomStartDate != null)
                        {
                            roomName = listRoom.FirstOrDefault(x => x.RoomID == itemRoomStartDate.RoomID).RoomName;
                            timeName = lstTimeSlot.FirstOrDefault(x => item == x.TimeSlotID.ToString()).TimeSlotName;
                            errors.Add(new BookingError
                            {
                                Error = "Đã có dữ liệu",
                                DescriptionError = $"{roomName} ca {timeName} ngày {room.StartDate.ToString("dd/MM/yyyy")} đã được đặt."
                            });
                            checkRoom = false;
                        }
                        else if (itemRoomEndDate != null)
                        {
                            roomName = listRoom.FirstOrDefault(x => x.RoomID == itemRoomEndDate.RoomID).RoomName;
                            timeName = lstTimeSlot.FirstOrDefault(x => item == x.TimeSlotID.ToString()).TimeSlotName;
                            errors.Add(new BookingError
                            {
                                Error = "Đã có dữ liệu",
                                DescriptionError = $"{roomName} ca {timeName} ngày {room.StartDate.ToString("dd/MM/yyyy")} đã được đặt."
                            });
                            checkRoom = false;
                        }
                    }


                }

            }

            return checkRoom;
        }

        /// <summary>
        /// thực hiện kiểm tra phòng trước 30 phút
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="cnn"></param>
        /// <param name="tran"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static async Task<BookingRoom> BookingRoomTime(Guid BookingRoomID, MySqlConnection cnn, MySqlTransaction tran, List<BookingError> errors)
        {
            bool checkTime = false;
            var parameters = new DynamicParameters();
            parameters.Add("@Id", BookingRoomID);
            BookingRoom bookingRoom = (BookingRoom)await cnn.QueryFirstOrDefaultAsync<BookingRoom>("SELECT b.* ,t1.StartTime FROM bookingroom b INNER JOIN timebooking t ON b.BookingRoomID = t.BookingRoomID INNER JOIN timeslot t1 ON t.TimeSlotID = t1.TimeSlotID WHERE b.BookingRoomID = @Id ORDER BY t1.TimeSlotName ASC limit 1;", parameters, transaction: tran);


            return bookingRoom;
        }

        /// <summary>
        /// Kiểm tra xem phòng đã được duyệt chưa
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="cnn"></param>
        /// <param name="tran"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static async Task<int> CheckRoomIsStatus(Guid BookingRoomID, MySqlConnection cnn, MySqlTransaction tran, List<BookingError> errors)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", BookingRoomID);
            int lstBookingRoom = (await cnn.QueryFirstOrDefaultAsync<int>("SELECT StatusBooking FROM BookingRoom where BookingRoomID = @Id ;", parameters, transaction: tran));

            return lstBookingRoom;
        }
        /// <summary>
        /// Thực hiện convert lại dữ liệu
        /// </summary>
        /// <param name="scheduleList"></param>
        /// <returns></returns>
        private List<BookingRoom> ConvertScheduleList(List<BookingRoom> scheduleList)
        {
            var newScheduleList = new List<BookingRoom>();

            for (int i = 0; i < scheduleList.Count; i++)
            {
                int j = i - 1;
                // Kiểm tra nếu tên tòa nhà chưa được xác định
                if (string.IsNullOrEmpty(scheduleList[i].Room))
                {
                    scheduleList[i].Room = scheduleList[j].Room;
                    scheduleList[i].Time = scheduleList[j].Time;
                    scheduleList[i].Week = scheduleList[j].Week;

                }
                scheduleList[i].Room = scheduleList[i].Room.Replace(" ", "");
                // Tách thông tin về toà nhà và phòng học
                var buildingInfo = scheduleList[i].Room.Split('-');
                var buildingCode = buildingInfo.Length > 1 ? buildingInfo[1] : scheduleList[i].Room;
                var buildingName = scheduleList[i].Room;
                // Tạo danh sách các tiết trống sáng, chiều và tối
                var morningFreePeriod = scheduleList[i].MorningFreePeriod;
                var afternoonFreePeriod = scheduleList[i].AfternoonFreePeriod;
                var eveningFreePeriod = scheduleList[i].EveningFreePeriod;
                // Tạo danh sách các ngày trong tuần
                if (scheduleList[i].DayOfWeek == "CN")
                {
                    scheduleList[i].DayOfWeek = "1";
                }
                var weekDays = scheduleList[i].DayOfWeek.Split(',').Select(x => int.Parse(x));
                var dates = new List<DateTime>();
                foreach (var weekday in weekDays)
                {
                    // Tính ngày bắt đầu và kết thúc của tuần
                    var weekStart = DateTime.ParseExact(scheduleList[i].Time.Split('-')[0], "dd/MM", null);
                    var weekEnd = DateTime.ParseExact(scheduleList[i].Time.Split('-')[1], "dd/MM/yyyy", null);

                    // Tìm ngày trong tuần tương ứng với ngày thứ weekday
                    var diff = weekday - 1 - (int)weekStart.DayOfWeek;
                    var date = weekStart.AddDays(diff);
                    dates.Add(date);
                }

                // Thêm mục mới vào danh sách
                foreach (var date in dates)
                {
                    int day = int.Parse(date.DayOfWeek.ToString("d")) + 1;
                    string dayOfWeek = day.ToString();

                    var newSchedule = new BookingRoom
                    {
                        Building = buildingCode,
                        Room = buildingName,
                        DayOfWeek = dayOfWeek,
                        Time = date.ToString(),
                        MorningFreePeriod = morningFreePeriod.ToString(),
                        AfternoonFreePeriod = afternoonFreePeriod.ToString(),
                        EveningFreePeriod = eveningFreePeriod.ToString(),
                        Week = scheduleList[i].Week,
                        StartDate = date,
                        EndDate = date
                    };
                    newScheduleList.Add(newSchedule);
                }
            }

            // Tạo một danh sách rỗng để chứa các mục được chuyển đổi
            List<BookingRoom> convertedList = new List<BookingRoom>();
            // Vòng lặp qua từng mục trong danh sách ban đầu
            foreach (BookingRoom item in newScheduleList)
            {
                convertedList.Add(new BookingRoom
                {
                    Building = item.Building,
                    Room = item.Room,
                    DayOfWeek = item.DayOfWeek,
                    SlotTime = item.MorningFreePeriod,
                    Week = item.Week,
                    Times = 1,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                });
                convertedList.Add(new BookingRoom
                {
                    Building = item.Building,
                    Room = item.Room,
                    DayOfWeek = item.DayOfWeek,
                    SlotTime = item.AfternoonFreePeriod,
                    Week = item.Week,
                    Times = 3,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,


                });
                convertedList.Add(new BookingRoom
                {
                    Building = item.Building,
                    Room = item.Room,
                    DayOfWeek = item.DayOfWeek,
                    SlotTime = item.EveningFreePeriod,
                    Week = item.Week,
                    Times = 5,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,

                });


            }
            List<BookingRoom> list = new();
            foreach (var item in convertedList)
            {
                string[] periods = item.SlotTime.Split(',');
                if (periods.Length < 4)
                {
                    list.AddRange(ConverDataToTimeSlot(item, periods, (int)item.Times));
                }

            }


            return list;


        }

        /// <summary>
        /// Thực hiện convert dữ liệu theo ca học
        /// </summary>
        /// <param name="item"></param>
        /// <param name="lstPeriods"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        private List<BookingRoom> ConverDataToTimeSlot(BookingRoom item, string[] lstPeriods, int slot)
        {
            List<BookingRoom> convertedList = new List<BookingRoom>();
            if (lstPeriods.Length > 1)
            {


                int ca = 0;
                if (item.Times == 5)
                {
                    ca = (int)item.Times;

                }
                else
                {
                    if (item.SlotTime.StartsWith("1,"))
                    {
                        ca = 2;
                    }
                    else if (item.SlotTime.StartsWith("4"))
                    {
                        ca = 1;
                    }
                    else if (item.SlotTime.StartsWith("7"))
                    {
                        ca = 4;
                    }
                    else if (item.SlotTime.StartsWith("10"))
                    {
                        ca = 3;
                    }

                }

                convertedList.Add(new BookingRoom
                {
                    Building = item.Building,
                    Room = item.Room,
                    DayOfWeek = item.DayOfWeek,
                    Times = int.Parse(ca.ToString()),
                    TimeSlotName = $"Ca {ca}",
                    Week = item.Week,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate
                });

            }
            else
            {
                int ca = (int)item.Times;
                int j = ca;
                for (int i = ca - 1; i <= item.Times; i++)
                {
                    if (i == 5) { break; }
                    convertedList.Add(new BookingRoom
                    {
                        Building = item.Building,
                        Room = item.Room,
                        DayOfWeek = item.DayOfWeek,
                        Times = j,
                        TimeSlotName = $"Ca {j}",
                        Week = item.Week,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                    });
                    j++;
                }

            }
            return convertedList;
        }

        /// <summary>
        /// Phân trang 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="keyWord"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<object> GetPaging(PagingParam param)
        {
            object result = null;
            int option = 0;
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {

                try
                {

                    ParamSchedulerBooking scheduler = await _repository.GetPaging(param, cnn);
                    List<SchedulerBooking> bookingRooms = new();
                    if (scheduler.rooms != null && scheduler.rooms.Any())
                    {

                        foreach (var booking in scheduler.bookings)
                        {
                            string[] timeIDs = booking.TimeSlots.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            // For từng dòng 
                            foreach (var item in timeIDs)
                            {
                                var time = await cnn.QueryAsync<TimeSlot>("SELECT * FROM TimeSlot");
                                var itemTime = time.FirstOrDefault(x => x.TimeSlotID.ToString() == item);
                                var startDate = DateTime.Parse(booking.StartDate.ToString("yyyy-MM-dd") + " " + itemTime.StartTime);
                                var endDate = DateTime.Parse(booking.EndDate.ToString("yyyy-MM-dd") + " " + itemTime.EndTime);


                                bookingRooms.Add(new SchedulerBooking
                                {
                                    BookingRoomID = booking.BookingRoomID,
                                    AvartarColor = booking.AvartarColor,
                                    RoomID = booking.RoomID,
                                    RoomName = booking.RoomName,
                                    FullName = booking.FullName,
                                    Quantity = booking.Quantity,
                                    RoomStatus = booking.RoomStatus,
                                    EndDate = endDate,
                                    StartDate = startDate,
                                    Subject = booking.Subject,
                                    Description = booking.Description,
                                    TimeSlotName = "Ca " + itemTime.TimeSlotName,
                                    StatusBooking = booking.StatusBooking,
                                });

                            }
                        }
                        int count = scheduler.rooms.Count();
                        if (count == 1)
                        {

                            result = new
                            {
                                option = (int)OptionPagingScheduler.OneRoom,
                                dataBooking = bookingRooms,
                                dataRoom = scheduler.rooms
                            };
                        }
                        else
                        {

                            result = new
                            {
                                option = (int)OptionPagingScheduler.AnyRoom,
                                dataBooking = bookingRooms,
                                dataRoom = scheduler.rooms
                            };
                        }

                    }
                }
                catch (Exception ex)
                {

                }


            }
            return result;
        }

        /// <summary>
        /// Xử lý duyệt phòng
        /// </summary>
        /// <param name="booking"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<BookingRoom> RequestBookingRoom(BookingRoomParam param)
        {
            var bookingRoom = new BookingRoom();
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                using (MySqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        var query = "SELECT b.*, r.SupporterID,r.SupporterEmail,r.SupporterName FROM BookingRoom b INNER JOIN room r ON b.RoomID = r.RoomID where b.BookingRoomID = @ID";
                        var parammeter = new DynamicParameters();
                        parammeter.Add("@ID", param.bookingRoomID);
                        var booking = await cnn.QueryFirstOrDefaultAsync<BookingRoom>(query, parammeter, transaction: tran);

                        //1.1. Gán lại trạng thái phòng theo yêu cầu gửi lên
                        booking.StatusBooking = param.option;
                        booking.RefusalReason = param.refusalReason;
                        //1.2. Update lại trạng thái đặt phòng trong bảng BookingRoom
                        var isUpdateBookingRequest = await _repository.Update(booking, booking.BookingRoomID, cnn, tran);

                        if (!isUpdateBookingRequest)
                        {

                            bookingRoom = null;

                        }
                        else
                        {
                            bookingRoom = booking;
                            tran.Commit();

                        }



                    }
                    catch (Exception ex)
                    {

                        bookingRoom = null;
                        tran.Rollback();
                    }


                }
            }
            return bookingRoom;
        }

        /// <summary>
        /// Thực hiện lấy danh sách yêu cầu đặt phòng chờ duyệt
        /// </summary>
        /// <param name="param"></param>
        /// PTTAM 04.01.2023
        public async Task<object> GetPagingRequest(PagingParam param)
        {
            object res = null;
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                res = await _repository.GetPagingRequest(param, cnn);

            }
            return res;
        }

        /// <summary>
        /// Thực hiện thêm mới yêu cầu đặt phòng
        /// </summary>
        /// <param name="bookings"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<object> InsertBookingRequest(BookingRoom booking, Guid userID)
        {
            object result = null;
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {

                using (MySqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        List<User> listUser = (List<User>)await cnn.QueryAsync<User>("SELECT * FROM User;", transaction: tran);
                        List<Role> listRole = (List<Role>)await cnn.QueryAsync<Role>("SELECT * FROM Role;", transaction: tran);

                        var user = listUser.FirstOrDefault(x => x.UserID == userID);
                        var role = listRole.FirstOrDefault(x => x.RoleID == user.RoleID);
                        if (role.RoleValue == (int)RoleOption.Admin)
                        {
                            booking.StatusBooking = (int?)StatusBookingRoom.Browse;
                        }
                        else
                        {
                            booking.StatusBooking = (int?)StatusBookingRoom.Pending;
                        }
                        List<BookingRoom> bookings = new List<BookingRoom>();

                        bookings.Add(booking);
                        List<BookingError> errors = new List<BookingError>();
                        //2. Check phòng đã được sử dụng hay chưa
                        bool checkRoom = await CheckRoomIsUsed(bookings, cnn, tran, errors);


                        //2.1. Nếu phòng chưa được sử dụng
                        if (checkRoom)
                        {
                            booking.BookingRoomID = Guid.NewGuid();
                            // Thực hiện insert
                            var resBooking = await _repository.Insert(booking, cnn, tran);
                            // thực hiện insert ca học
                            List<TimeBooking> lstTimeBooking = new();
                            // Tách chuỗi TimeSlotID
                            string[] timeIDs = booking.TimeSlots.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            // For từng dòng 
                            foreach (var item in timeIDs)
                            {
                                lstTimeBooking.Add(new TimeBooking
                                {
                                    BookingRoomID = booking.BookingRoomID,
                                    TimeSlotID = new Guid(item)
                                });

                            }
                            var resTimeBooking = await _repoTimeBooking.InsertMulti(lstTimeBooking, tran, cnn);
                            var res = (resBooking == true && resTimeBooking == true) ? true : false;

                            if (res)
                            {
                                result = new
                                {
                                    IsSucess = res,
                                    Data = errors,
                                    BookingData = booking,
                                };
                                tran.Commit();

                            }
                        }
                        //2.2. Nếu phòng đã được sử dùng
                        else
                        {
                            result = new
                            {
                                IsSucess = false,
                                Data = errors
                            };

                        }

                    }
                    catch { tran.Rollback(); }
                }


            }
            return result;
        }

        public async Task<object> UpdateBookingRequest(Guid BookingRoomID, BookingRoom booking)
        {
            object result = null;
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {

                using (MySqlTransaction tran = cnn.BeginTransaction())

                {
                    try
                    {
                        // Tách chuỗi TimeSlotID
                        string[] timeIDs = booking.TimeSlots.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        List<BookingRoom> bookings = new List<BookingRoom>();
                        List<TimeSlot> listTime = new();
                        // 1. Thực hiện tách booking theo các ca khác nhau nếu người dùng thêm nhiều ca
                        foreach (var item in timeIDs)
                        {
                            //booking.TimeSlotID = new Guid(item);
                            bookings.Add(booking);
                        }
                        List<BookingError> errors = new List<BookingError>();
                        //2. Check phòng đã được sử dụng hay chưa
                        bool checkRoom = await CheckRoomIsUsed(bookings, cnn, tran, errors);
                        //2.1. Nếu phòng chưa được sử dụng
                        if (checkRoom)
                        {
                            bool resUpdate = false;
                            // Kiểm tra xem phòng đã được duyệt chưa
                            int status = await CheckRoomIsStatus(BookingRoomID, cnn, tran, errors);
                            // nếu đang chờ duyệt thì update luôn
                            if (status == (int)StatusBookingRoom.Pending)
                            {
                                resUpdate = await _repository.Update(booking, BookingRoomID, cnn, tran);
                            }
                            else
                            {
                                // cập nhập lại status 
                                booking.StatusBooking = (int?)StatusBookingRoom.Pending;
                                // Thông báo email cho khách hàng
                                resUpdate = await _repository.Update(booking, BookingRoomID, cnn, tran);
                            }
                            if (resUpdate)
                            {
                                // thực hiện xóa hết các ca đi
                                var del = await _repository.DeleteRecord(BookingRoomID, "timebooking", cnn, tran);
                                // Thực hiện insert lại các ca
                                // Tạo dữ liệu insert 
                                var timeBookings = new List<TimeBooking>();
                                foreach (var item in timeIDs)
                                {
                                    var timeBooking = new TimeBooking();
                                    timeBooking.BookingRoomID = BookingRoomID;
                                    timeBooking.TimeSlotID = new Guid(item);
                                    timeBookings.Add(timeBooking);
                                }
                                var res = await _repository.InsertMultiTimeBooking(timeBookings, tran, cnn);
                                if (res)
                                {
                                    result = new
                                    {
                                        IsSusses = res,
                                        Data = errors
                                    };
                                    tran.Commit();

                                }
                            }
                            else
                            {
                                result = new
                                {
                                    IsSusses = false,
                                    Data = errors
                                };
                            }
                        }
                        //2.2. Nếu phòng đã được sử dùng
                        else
                        {
                            result = new
                            {
                                IsSusses = false,
                                Data = errors
                            };

                        }

                    }
                    catch { tran.Rollback(); }
                }


            }
            return result;
        }
        /// <summary>ToString("dd/MM/YYYY"))
        /// sinh ra đoạn mã để trả về email
        /// </summary>
        /// <returns></returns>
        public string CreateFormHTML(ParamReport param)
        {
            string html = "";
            string status = "";
            switch (param.StatusBooking)
            {
                case (int)StatusBookingRoom.Pending:
                    status = Resource.Pending; break;
                case (int)StatusBookingRoom.Browse:
                    status = Resource.Browse; break;
                case (int)StatusBookingRoom.Miss:
                    status = Resource.Miss; break;
                case (int)StatusBookingRoom.Cancel:
                    status = Resource.Cancel; break;
                default:
                    break;
            }
            // đọc file template
            string relativePath = @"HTML\templateHTML.txt";
            string absolutePath = Path.Combine(@"", relativePath);
            html = File.ReadAllText(absolutePath);
            html = html.Replace("{{param.Header}}", param.Header);
            html = html.Replace("{{param.FullName}}", param.FullName);
            html = html.Replace("{{param.RoomName}}", param.RoomName);
            html = html.Replace("{{param.BuildingName}}", param.BuildingName);
            html = html.Replace("{{param.DateBooking}}", param.StartDate.ToString("dd/MM/yyyy"));
            html = html.Replace("{{param.DateMiss}}", param.DateMiss);
            html = html.Replace("{{param.TimeSlotName}}", param.TimeSlotName);
            html = html.Replace("{{param.Capacity}}", param.Capacity.ToString() + " người");
            html = html.Replace("{{param.StatusBooking}}", status);
            html = html.Replace("{{param.Footer}}", param.Footer);
            return html;
        }

        /// <summary>
        /// Hủy yêu cầu đặt phòng
        /// </summary>
        /// <param name="BookingRoomID"></param>
        /// <returns></returns>
        public async Task<object> CancelBookingRoom(Guid BookingRoomID)
        {
            object result = null;
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {

                using (MySqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        List<BookingError> errors = new List<BookingError>();
                        //1. Check thời gian đặt phòng trước 30p hay không
                        BookingRoom bookingTime = await BookingRoomTime(BookingRoomID, cnn, tran, errors);
                        bool checkTimeBooking = false;
                        BookingRoom a = new BookingRoom();
                        if (bookingTime != null)
                        {
                            TimeSpan currentTime = DateTime.Now.TimeOfDay.Subtract(TimeSpan.FromMinutes(30));
                            //checkTimeBooking  =  (bookingTime.DateBooking < DateTime.Now && bookingTime.StartTime < currentTime) ? false : true;     
                        }
                        //1.1. Nếu thời gian bắt đầu vào phòng sớm hơn 30p thì cho phép hủy
                        if (checkTimeBooking)
                        {
                            // cập nhật trạng thái về hủy (4)

                            bookingTime.StatusBooking = (int?)StatusBookingRoom.Cancel;
                            bool resUpdate = await _repository.Update(bookingTime, BookingRoomID, cnn, tran);
                            // Cập nhật vào bảng history
                            // 
                            if (resUpdate)
                            {
                                result = new
                                {
                                    IsSusses = resUpdate,
                                    Data = errors
                                };
                                tran.Commit();
                                // Gửi email thông báo thành công
                                var emailFrom = new EmailData();
                                var emailParam = await _repository.GetParamReport(BookingRoomID, cnn); ;
                                // lấy dữ liệu đổ vào email
                                // lấy email theo user dữ liệu đặt phòng
                                emailParam.Header = Resource.EmailHeaderCancel;
                                emailParam.Footer = Resource.EmailFooterCancel;
                                emailFrom.EmailToId = emailParam.Email;
                                emailFrom.EmailBody = $"{CreateFormHTML(emailParam)}";
                                emailFrom.EmailSubject = "Thông báo hủy phòng";
                                emailFrom.EmailToName = emailParam.FullName;
                                SendEmail(emailFrom);

                            }
                            else
                            {
                                result = new
                                {
                                    IsSusses = false,
                                    Data = errors
                                };
                            }
                        }
                        //1.2. Nếu phòng đã gần đến thời gian 
                        else
                        {
                            result = new
                            {
                                IsSusses = false,
                                Data = errors
                            };

                        }

                    }
                    catch { tran.Rollback(); }
                }


            }
            return result;
        }

        /// <summary>
        /// Xem báo cáo theo booking ID
        /// </summary>
        /// <param name="entityId">Khóa chính đối tượng</param>
        ///  Created by: PTTAM (07/03/2023)
        public async Task<ParamReport> PrintReport(Guid id)
        {
            var res = new ParamReport();
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                res = await _repository.GetParamReport(id, cnn);
                _repository.CloseMyConnection();
            }
            return res;
        }

        /// <summary>
        /// Phân trang lịch sử đặt phòng
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<object> GetPagingHistory(PagingParam param)
        {
            object res = null;
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                res = await _repository.GetPagingHistory(param, cnn);

            }
            return res;
        }
        /// <summary>
        /// Hủy yêu cầu đặt phòng ko tính phút
        /// </summary>
        /// <param name="BookingRoomID"></param>
        /// <returns></returns>
        /// PTTAM
        public async Task<object> CancelBookingRoomNomal(Guid BookingRoomID)
        {
            object result = null;
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {

                using (MySqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        List<BookingRoom> dataBookingRoom = (List<BookingRoom>)await cnn.QueryAsync<BookingRoom>("SELECT * FROM BookingRoom;", transaction: tran);
                        var booking = dataBookingRoom.FirstOrDefault(x => x.BookingRoomID == BookingRoomID);
                        if (booking != null)
                        {
                            booking.StatusBooking = (int)StatusBookingRoom.Cancel;
                        }
                        bool resUpdate = await _repository.Update(booking, BookingRoomID, cnn, tran);
                        tran.Commit();
                        result = new
                        {
                            IsSusses = resUpdate,
                        };

                    }
                    catch { tran.Rollback(); }
                }


            }
            return result;
        }
        /// <summary>
        /// Gửi email chờ duyệt
        /// </summary>
        /// <param name="bookings"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> SendingEmailPending(BookingRoom booking, Guid userID)
        {
            bool result = true;
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {


                try
                {


                    // Gửi email thông báo thành công
                    var emailFrom = new EmailData();
                    var emailParam = await _repository.GetParamReport(booking.BookingRoomID, cnn); ;
                    // lấy dữ liệu đổ vào email
                    // lấy email theo user dữ liệu đặt phòng
                    emailParam.Header = Resource.EmailHeaderInsert;
                    emailParam.Footer = Resource.EmailFooter;
                    emailFrom.EmailToId = emailParam.Email;
                    emailFrom.EmailBody = $"{CreateFormHTML(emailParam)}";
                    emailFrom.EmailSubject = "Thông báo đặt phòng";
                    emailFrom.EmailToName = emailParam.FullName;
                    SendEmail(emailFrom);
                    // gửi email cho admin
                    var emailToAdmin = new EmailData();
                    emailToAdmin.EmailToId = booking.AdminEmail;
                    emailParam.Header = $"Giảng viên {emailParam.FullName} đã gửi yêu cầu đặt phòng đến bạn. Dưới đây là thông tin chi tiết về yêu cầu đặt phòng của giảng viên {emailParam.FullName}:";
                    emailParam.Footer = Resource.EmailFooter;

                    emailToAdmin.EmailSubject = "Thông báo đặt phòng";
                    emailToAdmin.EmailToName = emailParam.AdminName;
                    emailParam.FullName = emailParam.AdminName;
                    emailToAdmin.EmailBody = $"{CreateFormHTML(emailParam)}";
                    SendEmail(emailToAdmin);


                }
                catch { result = false; }



            }
            return result;
        }

        /// <summary>
        /// Gửi email cập nhật phòng
        /// </summary>
        /// <param name="BookingRoomID"></param>
        /// <param name="bookings"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> SendingEmailUpdate(Guid BookingRoomID, BookingRoom booking)
        {
            bool result = true;
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {


                try
                {

                    // Gửi email thông báo thành công
                    var emailFrom = new EmailData();
                    var emailParam = await _repository.GetParamReport(BookingRoomID, cnn); ;
                    // lấy dữ liệu đổ vào email
                    // lấy email theo user dữ liệu đặt phòng
                    emailParam.Header = Resource.EmailHeaderUpdate;
                    emailParam.Footer = Resource.EmailFooter;
                    emailFrom.EmailToId = emailParam.Email;
                    emailFrom.EmailBody = $"{CreateFormHTML(emailParam)}";
                    emailFrom.EmailSubject = "Thông báo đặt phòng";
                    SendEmail(emailFrom);
                    // gửi email cho admin
                    var emailToAdmin = new EmailData();
                    emailToAdmin.EmailToId = booking.AdminEmail;
                    emailParam.Header = $"Giảng viên {emailParam.FullName} đã cập nhật lại yêu cầu đặt phòng . Dưới đây là thông tin chi tiết về yêu cầu đặt phòng của giảng viên {emailParam.FullName}:";
                    emailParam.Footer = Resource.EmailFooter;
                    emailToAdmin.EmailSubject = "Thông báo đặt phòng";
                    emailParam.FullName = emailParam.AdminName;
                    emailToAdmin.EmailBody = $"{CreateFormHTML(emailParam)}";
                    SendEmail(emailToAdmin);


                }
                catch { result = false; }



            }
            return result;
        }

        /// <summary>
        /// Gửi email thông báo phê duyệt hoặc từ chối
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> SendingEmailAproveOrReject(BookingRoomParam param)
        {
            bool result = true;

            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                  try
                    {
                        var query = "SELECT b.*, r.SupporterID,r.SupporterEmail,r.SupporterName FROM BookingRoom b INNER JOIN room r ON b.RoomID = r.RoomID where b.BookingRoomID = @ID";
                        var parammeter = new DynamicParameters();
                        parammeter.Add("@ID", param.bookingRoomID);
                        var booking = await cnn.QueryFirstOrDefaultAsync<BookingRoom>(query, parammeter);

                        //1.1. Gán lại trạng thái phòng theo yêu cầu gửi lên
                        booking.StatusBooking = param.option;
                        booking.RefusalReason = param.refusalReason;



                        // Gửi email thông báo thành công
                        var emailFrom = new EmailData();
                        var emailParam = await _repository.GetParamReport(booking.BookingRoomID, cnn);
                        // lấy dữ liệu đổ vào email
                        // lấy email theo user dữ liệu đặt phòng
                        // Nếu lưu thành công thì sẽ lấy userID đang đăng nhập 
                        var status = "";
                        if (param.option == (int)(StatusBookingRoom.Browse))
                        {
                            emailParam.Header = Resource.AdminBrowser;

                        }
                        else if (param.option == (int)(StatusBookingRoom.Miss))
                        {
                            emailParam.Header = $"Quản trị viên đã từ chối yêu cầu đặt phòng của bạn vì lý do '{emailParam.RefusalReason.ToLower()}'. Dưới đây là thông tin chi tiết về yêu cầu đặt phòng của bạn:";
                        }
                        var userAdmin = _cache.Get<User>("userCache").FullName;
                        emailParam.Footer = Resource.EmailFooter;
                        emailFrom.EmailToId = emailParam.Email;
                        emailFrom.EmailBody = $"{CreateFormHTML(emailParam)}";
                        emailFrom.EmailSubject = "Thông báo đặt phòng";
                        emailFrom.EmailToName = emailParam.FullName;
                        SendEmail(emailFrom);
                        // gửi email cho người mở cửa
                        if (param.option == (int)(StatusBookingRoom.Browse))
                        {
                            emailParam.Header = $"Quản trị viên đã phê duyệt yêu cầu đặt phòng của {booking.FullName}, vui lòng mở cửa đúng giờ. Dưới đây là thông tin chi tiết về yêu cầu đặt phòng của {booking.FullName}:";
                            // lấy dữ liệu đổ vào email
                            // lấy email theo user dữ liệu đặt phòng

                            emailParam.Footer = Resource.EmailFooter;
                            emailFrom.EmailToId = booking.SupporterEmail;
                            emailFrom.EmailBody = $"{CreateFormHTML(emailParam)}";
                            emailFrom.EmailSubject = "Thông báo mở cửa phòng";
                            emailFrom.EmailToName = booking.SupporterName;
                            SendEmail(emailFrom);
                        }




                    }
                    catch (Exception ex)
                    {

                        result = false;
                    }


                
            }
            return result;
        }
        public async Task<bool> SendingEmailCancel(Guid BookingRoomID)
        {
            bool result = true;
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {


                try
                {

                    // Gửi email thông báo thành công
                    var emailFrom = new EmailData();
                    var emailParam = await _repository.GetParamReport(BookingRoomID, cnn); ;
                    // lấy dữ liệu đổ vào email
                    // lấy email theo user dữ liệu đặt phòng
                    emailParam.Header = Resource.EmailHeaderCancel;
                    emailParam.Footer = Resource.EmailFooterCancel;
                    emailFrom.EmailToId = emailParam.Email;
                    emailFrom.EmailBody = $"{CreateFormHTML(emailParam)}";
                    emailFrom.EmailSubject = "Thông báo hủy phòng";
                    emailFrom.EmailToName = emailParam.FullName;
                    SendEmail(emailFrom);


                }
                catch { result = false; }



            }
            return result;
        }
    }
}
