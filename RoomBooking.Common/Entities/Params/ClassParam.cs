using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities.Params
{
    public class ClassParam
    {
       public Guid UserID { get; set; }

        public int? BudgetYear { get; set; }

        public Guid? SubjectID { get; set; }


    }
}
