using Dapper;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
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
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RoomBooking.BLL.Services
{
    public class BookingRoomService : BaseService<BookingRoom>, IBookingRoomService
    {
        IBookingRoomRepository _repository;
        IBookingHistoryRepository _historyRepository;
        ITimeBookingRepository _repoTimeBooking;
        public BookingRoomService(IBookingRoomRepository repository, ITimeBookingRepository repoTimeBooking, IBookingHistoryRepository historyRepository) : base(repository)
        {
            _repository = repository;
            _repoTimeBooking = repoTimeBooking;
            _historyRepository = historyRepository;
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
                        if (checkRoom) {
                            var resBookingRoom = await _repository.InsertMulti(lst, tran, cnn);
                            var resTimeBooking= await _repoTimeBooking.InsertMulti(lstTimeBooking, tran, cnn);
                            tran.Commit();
                            var res= (resBookingRoom==true&&resTimeBooking==true)?true:false;
                            result = new
                            {

                                IsSuccess = res,
                                Data= errors,
                                Count= count

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
        private async Task<bool> CheckRoom(List<BookingRoom> lst,MySqlConnection cnn, MySqlTransaction tran, List<BookingError> errors,List<TimeBooking> lstTimeBooking)
        {
            // Thực hiện kiểm tra phòng đã được đặt chưa
            bool checkRoom = true;
            List<Room> dataRoom = (List<Room>)await cnn.QueryAsync<Room>("SELECT * FROM Room;", transaction: tran);
            List<TimeSlot> slotTime = (List<TimeSlot>)await cnn.QueryAsync<TimeSlot>("SELECT * FROM TimeSlot;", transaction: tran);
            List<TimeBooking> timeBookings= (List<TimeBooking>)await cnn.QueryAsync<TimeBooking>("SELECT * FROM TimeBooking;", transaction: tran);
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
                    room.YearPlan = room.DateBooking.Year;
                    room.DayOfWeek = room.DayOfWeek == "1" ? "CN" : room.DayOfWeek;
                    room.TimeSlots = itemTimeSlot.TimeSlotID.ToString();
                    lstTimeBooking.Add(new TimeBooking
                    {
                        BookingRoomID = room.BookingRoomID,
                        TimeSlotID = itemTimeSlot.TimeSlotID
                    }) ;
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
            List<Room> listRoom = (List<Room>)await cnn.QueryAsync<Room>("SELECT * FROM Room;", transaction: tran);
            List<TimeSlot> lstTimeSlot = (List<TimeSlot>)await cnn.QueryAsync<TimeSlot>("SELECT * FROM TimeSlot;", transaction: tran);
            var data = lst.Where(x => x.BookingRoomID != Guid.Empty).ToList();
            string roomName = "";
            int timeName =0;
            foreach (BookingRoom room in data)
            {
                // Tách chuỗi TimeSlotID
                string[] timeIDs = room.TimeSlots.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                // For từng dòng
                foreach (var item in timeIDs)
                {
                    var itemRoom = lstBookingRoom.FirstOrDefault(x => x.RoomID == room.RoomID && timeIDs.Contains(item) && x.DateBooking.ToString("yyyy/MM/dd") == room.DateBooking.ToString("yyyy/MM/dd"));
                    

                    if (itemRoom != null)
                    {
                        roomName = listRoom.FirstOrDefault(x => x.RoomID == itemRoom.RoomID).RoomName;
                        timeName = lstTimeSlot.FirstOrDefault(x => item==x.TimeSlotID.ToString()).TimeSlotName;
                        errors.Add(new BookingError
                        {
                            Error = "Đã có dữ liệu",
                            DescriptionError = $"{roomName} ca {timeName} ngày {room.DateBooking.ToString("dd/MM/yyyy")} đã được đặt."
                        });
                        checkRoom = false;
                    }

                }

            }

            return checkRoom;
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
                    if (day == 8)
                    {
                        dayOfWeek = "CN";
                    }

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
                        DateBooking = date,
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
                    DateBooking = item.DateBooking
                });
                convertedList.Add(new BookingRoom
                {
                    Building = item.Building,
                    Room = item.Room,
                    DayOfWeek = item.DayOfWeek,
                    SlotTime = item.AfternoonFreePeriod,
                    Week = item.Week,
                    Times = 3,
                    DateBooking = item.DateBooking

                });
                convertedList.Add(new BookingRoom
                {
                    Building = item.Building,
                    Room = item.Room,
                    DayOfWeek = item.DayOfWeek,
                    SlotTime = item.EveningFreePeriod,
                    Week = item.Week,
                    Times = 5,
                    DateBooking = item.DateBooking

                });


            }
            List<BookingRoom> list = new();
            foreach (var item in convertedList)
            {
                string[] periods = item.SlotTime.Split(',');
                if (periods.Length < 4)
                {
                    list.AddRange(ConverDataToTimeSlot(item, periods, item.Times));
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
                    ca = item.Times;

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
                    Week = item.Week,
                    DateBooking = item.DateBooking
                });

            }
            else
            {
                int ca = item.Times;
                int j = ca - 1;
                for (int i = ca - 1; i <= item.Times; i++)
                {
                    if (i == 5) { break; }
                    convertedList.Add(new BookingRoom
                    {
                        Building = item.Building,
                        Room = item.Room,
                        DayOfWeek = item.DayOfWeek,
                        Times = ca++,
                        Week = item.Week,
                        DateBooking = item.DateBooking
                    });
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
            int[] weekDays = { 2, 3, 4, 5, 6, 7, 8 };
            var datetimes = new List<DateTime>();
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
              //  var lstWeek = await cnn.QueryAsync<Week>("SELECT * FROM Week;");
             //   var weekCurent = lstWeek.FirstOrDefault(x => x.WeekCode == param.week);
                foreach (var weekday in weekDays)
                {
                    // Tính ngày bắt đầu và kết thúc của tuần
                    //var weekStart = DateTime.ParseExact(scheduleItems[0].Time.Split('-')[0], "dd/MM/yyyy", null);
                //    var weekStart = DateTime.ParseExact(weekCurent.StartDate.ToString(), "M/d/yyyy h:mm:ss tt", null);
               //     var weekEnd = DateTime.ParseExact(weekCurent.EndDate.ToString(), "M/d/yyyy h:mm:ss tt", null);

                    // Tìm ngày trong tuần tương ứng với ngày thứ weekday
                //    var diff = weekday - 1 - (int)weekStart.DayOfWeek;
                //    var date = weekStart.AddDays(diff);
                //    datetimes.Add(date);
                }
                var dateConvert = datetimes.Select(d => d.ToString("yyyy/dd/MM")).ToList();
                string jsonDate = JsonConvert.SerializeObject(dateConvert);
                param.week = jsonDate;
                var res = await _repository.GetPaging(param, cnn);
                result = res;
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
        public async Task<object> RequestBookingRoom(BookingRoomParam param)
        {
            object result = null;

            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                using (MySqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        var requests = await cnn.QueryAsync<BookingRoom>("SELECT * FROM BookingRoom",transaction:tran);
                        //1. Lấy ra yêu cầu của người dùng gửi lên
                        var booking = requests.FirstOrDefault(x => x.BookingRoomID == param.bookingRoomID);
                        //1.1. Gán lại trạng thái phòng theo yêu cầu gửi lên
                        booking.StatusBooking = param.option;
                        booking.RefusalReason=param.refusalReason;
                        //1.2. Update lại trạng thái đặt phòng trong bảng BookingRoom
                        var isUpdateBookingRequest = await _repository.Update(booking, booking.BookingRoomID, cnn, tran);

                        if (!isUpdateBookingRequest)
                        {
                            result = new
                            {
                                IsSucces = false,
                            };
                        }
                        else
                        {
                            result = new
                            {
                                IsSucces = true,
                            };
                            tran.Commit();
                        }
                      


                    }
                    catch (Exception ex)
                    {

                        result = new
                        {
                            IsSucces = false,
                        };
                        tran.Rollback();
                    }


                }
            }
            return result;
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
        public async Task<object> InsertBookingRequest(BookingRoom booking,Guid userID)
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

                        var user = listUser.FirstOrDefault(x=>x.UserID==userID);
                        var role = listRole.FirstOrDefault(x => x.RoleID == user.RoleID);
                        if (role.RoleValue ==(int) RoleOption.Admin)
                        {
                            booking.StatusBooking =(int) OptionRequest.Approve;
                        }
                        else { 
                        booking.StatusBooking = (int)OptionRequest.Await;
                        }
                        List<BookingRoom> bookings = new List<BookingRoom>();
                       
                        bookings.Add(booking);
                        List<BookingError> errors = new List<BookingError>();
                        //2. Check phòng đã được sử dụng hay chưa
                        bool checkRoom = await CheckRoomIsUsed(bookings, cnn, tran, errors);
                        
                      
                        //2.1. Nếu phòng chưa được sử dụng
                        if (checkRoom)
                        {
                            
                            // Thực hiện insert
                            var resBooking = await _repository.Insert(booking, cnn,tran);
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
                                }) ;

                            }
                            var resTimeBooking = await _repoTimeBooking.InsertMulti(lstTimeBooking, tran, cnn);
                            var res =( resBooking == true && resTimeBooking == true)?true: false ;

                            if (res)
                            {
                                result = new
                                {
                                    IsSusses=res,
                                    Data= errors
                                };
                                tran.Commit();
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
    }
}
