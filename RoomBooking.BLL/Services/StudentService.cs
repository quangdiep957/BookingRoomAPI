using Google.Type;
using MySqlConnector;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using RoomBooking.Common.Entities.Params;
using RoomBooking.Common.Enum;
using RoomBooking.Common.Exception;
using RoomBooking.Common.Functions;
using RoomBooking.Common.Resources;
using RoomBooking.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using XAct;
using XSystem.Security.Cryptography;
using static Dapper.SqlMapper;
using DateTime = System.DateTime;

namespace RoomBooking.BLL.Services
{
    public class StudentService : BaseService<Student>, IStudentService
    {
        IStudentRepository _repository;
        IRoleRepository _roleRepo;
        public StudentService(IStudentRepository repository,IRoleRepository roleRepository) : base(repository)
        {
            _repository = repository;
            _roleRepo = roleRepository;
        }

        public async Task<object> CheckAttendanceApp(ParamStudent param)
        {
            var isSucess = false;
            var dataStudent = JsonSerializer.Deserialize<List<StudentCheckDto>>(param.Param);
            List <StudentCheckDto> students = new List<StudentCheckDto>();
            if (dataStudent != null)
            {
                // Lấy thông tin giờ học
                var timeSlot = await _repository.GetTimeSlot();
                // Thực hiện thêm dữ liệu vào db
                using (MySqlConnection cnn = _repository.GetOpenConnection())
                {

                    using (MySqlTransaction tran = cnn.BeginTransaction())
                    {
                        try
                        {
                            // Thực hiện validate thời gian điểm danh 
                            // Lấy thông tin múi giờ của Việt Nam
                            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                            // Lấy ngày và giờ hiện tại theo múi giờ Việt Nam
                            DateTime timeNowVietNam = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

                            foreach (var item in dataStudent)
                            {
                                string timeID = item.TimeSlots.Split(',')[0];
                                var time = timeSlot.Find(x => x.TimeSlotID == Guid.Parse(timeID));
                                if (time != null)
                                {
                                    // Kết hợp ngày và thời gian + thêm 20p
                                    DateTime dateStartCheck = (item.StartDate.Date + time.StartTime).AddMinutes(20);
                                    DateTime dateEndCheck = (item.StartDate.Date + time.EndTime).AddMinutes(20);
                                    // Kiểm tra nếu lớn hơn hoặc bằng thời gian hiện tại thì không cho điểm danh 
                                    if (dateStartCheck < timeNowVietNam && dateEndCheck < timeNowVietNam)
                                    {
                                        tran.Rollback();
                                        return new { mess = "Vượt quá thời gian" };
                                    }
                                    else
                                    {
                                        students.Add(item);
                                    }
                                }
                            }
                            var res = await _repository.CheckAttendanceApp(students, tran,cnn);
                            if (!res)
                            {
                                isSucess = false;

                            }
                            tran.Commit();
                        }
                        catch (Exception)
                        {
                            isSucess = false;
                            tran.Rollback();


                        }
                        finally
                        {
                            cnn.Close();
                        }
                    }
                    _repository.CloseMyConnection();
                }
            }
            return isSucess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="StudentCode"></param>
        /// <returns></returns>
        public Task<Student> CheckFaceID(string StudentCode)
        {
            
            var res = new Student();
             return Task.FromResult(res);
        }

        public async Task<object> GetListCheck(CheckStudentParam param)
        {
            var list = new List<CheckStudentDto>();
             list = (List<CheckStudentDto>)await _repository.GetListCheck(param);

            // lấy ra 2 danh sách class và môn học

            List<object> listClass = list
                .GroupBy(x => x.ClassID)
                .Select(group => group.First())  
                .Cast<object>()
                .ToList();

            List<object> listSubject = list
                .GroupBy(x => x.SubjectID)
                .Select(group => group.First())  
                .Cast<object>()
                .ToList();

            List<object> listBooking = list
                .GroupBy(x => x.BookingRoomID)
                .Select(group => group.First())
                .Cast<object>()
                .ToList();

            return new MyObject
            {
                Classe = listClass,
                Subjects = listSubject,
                Booking = listBooking,
                Data = list
            };
        }


        /// <summary>
        /// Thực hiện nghiệp vụ khi phân trang
        /// </summary>
        /// <param name="pageSize">Số bản ghi / trang</param>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="keyWord">Từ khóa</param>
        /// <param name="roleId">Khóa chính vai trò</param>
        /// <returns>Object chứa danh sách người dùng lọc được theo yêu cầu</returns>
        ///  Created by: bqdiep(10/9/2022)
        public async Task<object> GetPaging(int pageSize, int pageIndex, string? keyWord, Guid? roleId)
        {
            var res = await _repository.GetPaging(pageSize, pageIndex, keyWord, roleId);
            return res;
        }

        /// <summary>
        /// Validate email, ngày sinh, ngày cấp của nhân viên
        /// </summary>
        /// <param name="entity">đối tượng nhân viên</param>
        /// <returns>true: nếu không có lỗi, false: có lỗi</returns>
        /// Created by: bqdiep(10/9/2022)
        protected override bool ValidateCustom(Student Student)
        {
            
            if (!CommonFunction.IsValidEmail(Student.Email))
            {
                isValidCustom = false;


                object error = new
                {
                    errorTitle = Resource.ErrorInput,
                    errorName = Resource.IsErrorEmail

                };
                errorList.Add(error);
            }

            return isValidCustom;
        }
        
    }
}
