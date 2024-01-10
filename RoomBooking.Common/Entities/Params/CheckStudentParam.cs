using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities.Params
{
    public class CheckStudentParam
    {
        public Guid UserID { get; set; }

        public DateTime DateRequest { get; set; }

        public Guid? SubjectID { get; set; }
        public Guid? ClassID { get; set; }

        public Guid? BookingRoomID { get; set; }

    }

    public class CheckStudentDto
    {
        public Guid UserID { get; set; }

        public DateTime DateRequest { get; set; }
        public Guid? SubjectID { get; set; }

        public string SubjectName { get; set; }
        public Guid? ClassID { get; set; }

        public Guid? BookingRoomID { get; set; }

        public string StudentCode { get; set; }

        public string FullName { get; set; }

        public string ClassCode { get; set; }

        public string ClassName { get; set; }

        public Boolean Status { get; set; }

        public Guid? StudentID { get; set; }

        public DateTime StartDate { get; set; }

        public string TimeSlots { get; set; }

        public string Subject { get; set; }
    }

    public class StudentCheckDto
    {
        public string UserID { get; set; }
        [ForGetting]
        [ForBinding]
        public DateTime DateRequest { get; set; }
        [ForGetting]
        [ForBinding]
        public string SubjectID { get; set; }
        public string SubjectName { get; set; }
        [ForGetting]
        [ForBinding]
        public string ClassID { get; set; }
        [ForGetting]
        [ForBinding]
        public string BookingRoomID { get; set; }
        [ForGetting]
        [ForBinding]
        public string StudentCode { get; set; }

        public string FullName { get; set; }

        public string ClassCode { get; set; }
        public string ClassName { get; set; }
        [ForGetting]
        [ForBinding]
        public bool Status { get; set; }

        [ForGetting]
        [ForBinding]
        public string StudentID { get; set; }

        public DateTime StartDate { get; set; }

        public string TimeSlots { get; set; }

        public string Subject { get; set; }
    }


    public class MyObject
    {
        public List<object> Classe { get; set; }

        public List<object> Subjects { get; set; }

        public List<object> Booking { get; set; }

        public List<CheckStudentDto> Data { get; set; }


    }

    public class ParamStudent
    {
        public string Param { get; set; }


    }
}
