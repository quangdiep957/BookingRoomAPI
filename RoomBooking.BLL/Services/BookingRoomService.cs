using Dapper;
using ExcelDataReader;
using MySqlConnector;
using Newtonsoft.Json;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using RoomBooking.Common.Enum;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
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
        IWeekRepository _repoWeek;
        public BookingRoomService(IBookingRoomRepository repository, IWeekRepository repoWeek, IBookingHistoryRepository historyRepository) : base(repository)
        {
            _repository = repository;
            _repoWeek = repoWeek;
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
            List<Week> weeks = new List<Week>();
            // int[] weekDays = { 2, 3, 4, 5, 6, 7, 8 };
            // var dates = new List<DateTime>();
            //foreach (var weekday in weekDays)
            //{
            //    // Tính ngày bắt đầu và kết thúc của tuần

            //    var weekStart = DateTime.ParseExact(scheduleItems[0].Time.Split('-')[0], "dd/MM/yyyy", null);
            //    var weekEnd = DateTime.ParseExact(scheduleItems[0].Time.Split('-')[1], "dd/MM/yyyy", null);

            //    // Tìm ngày trong tuần tương ứng với ngày thứ weekday
            //    var diff = weekday - 1 - (int)weekStart.DayOfWeek;
            //    var date = weekStart.AddDays(diff);
            //    dates.Add(date);
            //}
            // Thực hiện convert lại dữ liệu
            List<BookingRoom> lst = ConvertScheduleList(scheduleItems);


            // Thực hiện thêm tuần vào db
            var weekStart = DateTime.ParseExact(scheduleItems[0].Time.Split('-')[0], "dd/MM/yyyy", null);
            var weekEnd = DateTime.ParseExact(scheduleItems[0].Time.Split('-')[1], "dd/MM/yyyy", null);


            Object result = new();
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {

                using (MySqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        List<BookingError> errors = new List<BookingError>();
                        int count = lst.Count;
                        bool checkRoom = await CheckRoom(lst, cnn, tran, errors);
                        if (checkRoom) {
                            var res = await _repository.InsertMulti(lst, tran, cnn);
                            tran.Commit();
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
        private async Task<bool> CheckRoom(List<BookingRoom> lst,MySqlConnection cnn, MySqlTransaction tran, List<BookingError> errors)
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
                    room.TimeSlotID = itemTimeSlot.TimeSlotID;
                    room.Subject = "Lịch học tuần " + room.Week;
                    room.UserID = new Guid("1283753d-5374-5932-8ffd-ed7281085324");
                    room.YearPlan = room.DateBooking.Year;
                    room.DayOfWeek = room.DayOfWeek == "1" ? "CN" : room.DayOfWeek;
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
            List<BookingRoom> lstBookingRoom = (List<BookingRoom>)await cnn.QueryAsync<BookingRoom>("SELECT * FROM BookingRoom;", transaction: tran);
            foreach (BookingRoom room in lst)
            {

                var itemRoom = lstBookingRoom.Where(x => x.RoomID == room.RoomID && room.TimeSlotID == x.TimeSlotID && x.DateBooking == room.DateBooking).FirstOrDefault();

                if (itemRoom != null)
                {

                    errors.Add(new BookingError
                    {
                        Error = "Đã có dữ liệu",
                        DescriptionError = $"Phòng {room.Room} ca {room.Times} ngày {room.DateBooking.ToString("dd/MM/yyyy")} đã được đặt."
                    });
                    checkRoom = false;
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
                    var weekStart = DateTime.ParseExact(scheduleList[i].Time.Split('-')[0], "dd/MM/yyyy", null);
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
            // List<Week> weeks = new List<Week>();
            int[] weekDays = { 2, 3, 4, 5, 6, 7, 8 };
            var datetimes = new List<DateTime>();
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                var lstWeek = await cnn.QueryAsync<Week>("SELECT * FROM Week;");
                var weekCurent = lstWeek.FirstOrDefault(x => x.WeekCode == param.week);
                foreach (var weekday in weekDays)
                {
                    // Tính ngày bắt đầu và kết thúc của tuần
                    //var weekStart = DateTime.ParseExact(scheduleItems[0].Time.Split('-')[0], "dd/MM/yyyy", null);
                    var weekStart = DateTime.ParseExact(weekCurent.StartDate.ToString(), "M/d/yyyy h:mm:ss tt", null);
                    var weekEnd = DateTime.ParseExact(weekCurent.EndDate.ToString(), "M/d/yyyy h:mm:ss tt", null);

                    // Tìm ngày trong tuần tương ứng với ngày thứ weekday
                    var diff = weekday - 1 - (int)weekStart.DayOfWeek;
                    var date = weekStart.AddDays(diff);
                    datetimes.Add(date);
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
        public async Task<object> RequestBookingRoom(Guid requestID, int option)
        {
            object result = null;

            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                using (MySqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        var requests = await cnn.QueryAsync<BookingRoom>("SELECT * FROM BookingRoom");
                        var booking = requests.FirstOrDefault(x => x.BookingRoomID == requestID);
                        booking.StatusBooking = option;
                        //1. Update lại trạng thái đặt phòng trong bảng BookingRoom
                        var isUpdateBookingRequest = await _repository.Update(booking, booking.BookingRoomID, cnn, tran);


                        BookingHistory bookingHistory = new BookingHistory
                        {
                            BookingRoomID = booking.BookingRoomID,
                            UserID = booking.UserID,
                            RoomID = booking.RoomID,
                            TimeSlotID = booking.TimeSlotID,
                            WeekID = booking.WeekID,
                            DateBooking = booking.DateRequest,
                            Day = booking.Day,
                            Subject = booking.Subject,
                            YearPlan = booking.YearPlan,
                            Description = booking.Description,
                            StatusRequest = booking.StatusBooking,
                            DayOfWeek = booking.DayOfWeek
                        };
                        // Nếu là trạng thái phê duyệt
                        if (option == (int)OptionRequest.Approve)
                        {
                            result = await ApproveRequestBookingRoom(booking, cnn, tran, isUpdateBookingRequest, bookingHistory);

                        }
                        else if (option == (int)OptionRequest.Reject)
                        {

                            // 2.Thêm vào lịch sử đặt phòng
                            var isInsertHistory = await _historyRepository.Insert(bookingHistory, cnn, tran);

                            if (!isUpdateBookingRequest || !isInsertHistory)
                            {
                                result = new
                                {
                                    IsSucces = false,
                                    StatusRoom = (int)StatusRoom.Active,
                                    Description = "Có lỗi xảy ra"
                                };
                                tran.Rollback();
                            }
                            else
                            {
                                result = new
                                {
                                    IsSucces = true,
                                    StatusRoom = (int)StatusRoom.Active
                                };
                                tran.Commit();
                            }

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
        /// Nhấn đồng ý phê duyệt
        /// </summary>
        /// <param name="booking"></param>
        /// <param name="result"></param>
        /// <param name="cnn"></param>
        /// <param name="tran"></param>
        /// <param name="isUpdateBookingRequest"></param>
        /// <param name="bookingHistory"></param>
        /// <returns></returns>
        private async Task<object> ApproveRequestBookingRoom(BookingRoom booking, MySqlConnection cnn, MySqlTransaction tran, bool isUpdateBookingRequest, BookingHistory bookingHistory)
        {
            object result = null;

            BookingRoom bookingRoom = new BookingRoom
            {
                BookingRoomID = booking.BookingRoomID,
                UserID = booking.UserID,
                RoomID = booking.RoomID,
                TimeSlotID = booking.TimeSlotID,
                WeekID = booking.WeekID,
                DateBooking = booking.DateRequest,
                Day = booking.Day,
                Subject = booking.Subject,
                YearPlan = booking.YearPlan,
                Description = booking.Description,
                DayOfWeek = booking.DayOfWeek
            };
            // 3. Thêm mới phòng
            var isInsertBookingRoom = await _repository.Insert(bookingRoom, cnn, tran);
            // 4. Thêm vào lịch sử 
            var isInsertHistory = await _historyRepository.Insert(bookingHistory, cnn, tran);

            // Kiểm tra update, thêm mới có lỗi gì không, nếu có:
            if (!isUpdateBookingRequest || !isInsertHistory || isInsertBookingRoom)
            {
                result = new
                {
                    IsSucces = false,
                    StatusRoom = StatusRoom.Empty,
                    Description = "Có lỗi xảy ra"
                };
                tran.Rollback();

            }
            else
            {
                result = new
                {
                    IsSucces = true,
                    StatusRoom = StatusRoom.Empty,
                    Description = "Thành công"
                };
                tran.Commit();
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
    }
}
