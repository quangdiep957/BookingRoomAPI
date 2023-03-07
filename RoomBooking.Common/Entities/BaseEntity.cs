using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    public class BaseEntity
    {
        /// <summary>
        /// Người tạo
        /// </summary>
        ///  Created by: PTTAM (30/08/2022)
        [ForBinding]

        public string CreatedBy { get; set; } = "pttam@gmail.com";

        /// <summary>
        /// Ngày tạo
        /// </summary>
        ///  Created by: PTTAM (30/08/2022)
        [ForBinding]

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Người sửa
        /// </summary>
        ///  Created by: PTTAM (30/08/2022)
        [ForBinding]
        ///  
        public string ModifiedBy { get; set; } = "pttam@gmail.com";

        /// <summary>
        /// Ngày sửa
        /// </summary>
        ///  Created by: PTTAM (30/08/2022)
        [ForBinding]
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
    }
}
