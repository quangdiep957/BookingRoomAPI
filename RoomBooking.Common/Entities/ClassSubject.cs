using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// Thông tin lớp
    /// </summary>
    ///  Created by: bqdiep (07/03/2023)
    public class ClassSubject
    {
        /// <summary>
        /// Khóa chính lớp
        /// </summary>
        ///  Created by: bqdiep (07/03/2023)
        public Guid ClassID { get; set; }

        /// <summary>
        /// Mã lớp
        /// </summary>
        ///  Created by: bqdiep (07/03/2023)
        public string ClassCode { get; set; } = string.Empty;

        public string ClassName { get; set; } = string.Empty;
       
       
        public int BudgerYear { get; set; }

        /// <summary>
        /// Khóa chính lớp
        /// </summary>
        ///  Created by: bqdiep (07/03/2023)
        public Guid SubjectID { get; set; }

        /// <summary>
        /// Mã lớp
        /// </summary>
        ///  Created by: bqdiep (07/03/2023)
        public string SubjectCode { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

    }
}
