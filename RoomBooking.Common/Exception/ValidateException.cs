using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.Exception
{
    public class ValidateException : IOException
    {
        Dictionary<string, object> MISAData = new Dictionary<string, object>(); // chứa lỗi và đối tượng data chứa lỗi
        public ValidateException(Dictionary<string, object> data)
        {
            MISAData = data;
        }

        public override IDictionary Data
        {
            get { return MISAData; }
        }

    }
}
