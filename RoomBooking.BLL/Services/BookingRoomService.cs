using Dapper;
using ExcelDataReader;
using MySqlConnector;
using Newtonsoft.Json;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
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
        IWeekRepository _repoWeek;
        public BookingRoomService(IBookingRoomRepository repository, IWeekRepository repoWeek) : base(repository)
        {
            _repository = repository;
            _repoWeek = repoWeek;
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

            // Check xem các phòng trong list đã có trong db chưa
            var listRoomEmpty = await _repository.CheckRoom(lst);
           // Nếu chưa có
            if (listRoomEmpty.Count>0)
            {
                return new
                {

                    IsSuccess = false,
                    Data = listRoomEmpty // mảng chứa tên phòng chưa có trong csdl
                };
            }
            // Nếu có rồi
            else
            {
                // Thực hiện thêm tuần vào db
                var weekStart = DateTime.ParseExact(scheduleItems[0].Time.Split('-')[0], "dd/MM/yyyy", null);
                var weekEnd = DateTime.ParseExact(scheduleItems[0].Time.Split('-')[1], "dd/MM/yyyy", null);
                Week week = new Week
                {
                    WeekID = Guid.NewGuid(),
                    WeekCode = scheduleItems[0].Week
                    ,
                    WeekName = "Tuần " + scheduleItems[0].Week,
                    StartDate = weekStart,
                    EndDate = weekEnd
                };
                Object result = new();
                using (MySqlConnection cnn = _repository.GetOpenConnection())
                {
                  
                    using (MySqlTransaction tran = cnn.BeginTransaction())
                    {
                        try
                        {
                            bool isInsertSucessWeek = await _repoWeek.Insert(week, cnn, tran);

                            //Thêm tuần thành công
                            if (isInsertSucessWeek)
                            {
                                var res = await _repository.InsertMulti(lst, tran, cnn);
                                tran.Commit();
                                result = new
                                {

                                    IsSuccess = res,
                                    Data = listRoomEmpty
                                };
                            }
                            // Thêm tuần thất bại (TH đã thêm tuần => đã thêm dữ liệu vào 1 lần)
                            else
                            {
                                tran.Rollback();
                                result = new
                                {

                                    IsSuccess = false,
                                    Data = listRoomEmpty // mảng trống
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
                    scheduleList[i].DayOfWeek = "8";
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

                list.AddRange(ConverDataToTimeSlot(item, periods, item.Times));
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
                for (int i = 0; i < lstPeriods.Length; i += 3)
                {
                    int ca = i / 3 + slot; // Tính toán số ca tương ứng với tiết
                    if (item.SlotTime.StartsWith("13"))
                    {
                        ca = 5;
                        i += 2;
                    }
                    else
                    {
                        if (lstPeriods.Length <= 3)
                        {
                            if (item.SlotTime.StartsWith("1,"))
                            {
                                ca = 1;
                            }
                            else if (item.SlotTime.StartsWith("4"))
                            {
                                ca = 2;
                            }
                            else if (item.SlotTime.StartsWith("7"))
                            {
                                ca = 3;
                            }
                            else if (item.SlotTime.StartsWith("10"))
                            {
                                ca = 4;
                            }
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
        public async Task<object> GetPaging(int pageSize, int pageIndex, int type,string week, string? keyWord, Guid? roomID, Guid? buildingID, Guid? timeSlotID)
        {
            object result = null;
            // List<Week> weeks = new List<Week>();
            int[] weekDays = { 2, 3, 4, 5, 6, 7, 8 };
            var datetimes = new List<DateTime>();
            using (MySqlConnection cnn = _repository.GetOpenConnection())
            {
                var lstWeek = await cnn.QueryAsync<Week>("SELECT * FROM Week;");
                var weekCurent = lstWeek.FirstOrDefault(x => x.WeekCode == week);
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
                var res = await _repository.GetPaging(pageSize, pageIndex, type, jsonDate,cnn, keyWord, roomID, buildingID, timeSlotID);
                result = res; 
            }
            
            return result;
        }
    }
}
