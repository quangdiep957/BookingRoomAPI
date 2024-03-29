﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportBooking.Models
{
    public class ParamReport
    {
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }
        public  string BookingRoomID { get; set; }

        public string DateBooking { get; set; }

        public string RoomName { get; set; }
        public string EquipmentName { get; set; }

        public string TimeSlotName { get; set; }
        public int Quantity { get; set; }
        public int Capacity { get; set; }
        public DateTime StartDate { get; set; }
        public string AdminName { get; set; }
        public string DepartmentName { get; set; }
    }
}