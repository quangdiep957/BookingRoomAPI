using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Entities.Params
{
    public class PagingParam
    {
        public int pageSize;
        public int pageIndex;
        public int type;
        public string week;
        public string? keyWord;
        public Guid? roomID;
        public Guid? buildingID;
        public Guid? timeSlotID;
        public Guid? weekID;
        public string? equipmentIDs;
    }
}
