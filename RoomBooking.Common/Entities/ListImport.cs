using RoomBooking.Common.AttributeCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities
{
    /// <summary>
    /// </summary>
    ///  Created by: bqdiep (07/03/2023)
    public class ListImport 
    {
        public List<Class> classImport { get; set; }

        public List<Subject> subjectImport { get; set; }
        public List<Building> buildingImport { get; set; }
        public List<Room> roomImport { get; set; }
    }
}
