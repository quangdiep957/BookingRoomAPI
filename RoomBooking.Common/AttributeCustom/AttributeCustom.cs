using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.Common.AttributeCustom
{
    #region Attribute
  
    /// <summary>
    /// Attribute dùng để validate các trường dữ liệu không được để trống
    /// </summary>
    ///  created by: PTTAM (08/03/2023)
    [AttributeUsage(AttributeTargets.Property)]
    public class NotEmpty : Attribute
    {

    }

    /// <summary>
    /// Attribute dùng để lấy các trường dữ liệu 
    /// </summary>
    ///  created by: PTTAM (08/03/2023)
    [AttributeUsage(AttributeTargets.Property)]
    public class ForGetting : Attribute
    {

    }

    /// <summary>
    /// Attribute dùng để lấy các trường dữ liệu 
    /// </summary>
    ///  created by: PTTAM (08/03/2023)
    [AttributeUsage(AttributeTargets.Property)]
    public class ForBinding : Attribute
    {

    }
    /// <summary>
    /// Attribute dùng để xóa theo key
    /// </summary>
    ///  created by: PTTAM (08/03/2023)
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyDelete : Attribute
    {

    }
    /// <summary>
    /// Attribute dùng để lấy trường dữ liệu là khóa ngoại
    /// </summary>
    ///  created by: PTTAM (08/03/2023)
    [AttributeUsage(AttributeTargets.Property)]
    public class Ambiguous : Attribute
    {

    }

    /// <summary>
    /// Attribute dùng để validate mã phải là duy nhất
    /// </summary>
    /// created by: PTTAM (08/03/2023)
    [AttributeUsage(AttributeTargets.Property)]
    public class Unique : Attribute
    {

    }

    /// <summary>
    /// Attribute dùng để validate mã phải là duy nhất
    /// </summary>
    /// created by: PTTAM (08/03/2023)
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKey : Attribute
    {

    }

    /// <summary>
    /// Attribute dùng để hiển thị tên prop
    /// </summary>
    /// created by: PTTAM (8/8/2022)
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyNameDisplay : Attribute
    {
        public string? displayName = string.Empty;
        public PropertyNameDisplay(string? propName = null)
        {
            displayName = propName;
        }

    }

    /// <summary>
    /// Validate độ dài của property không vượt quá Length
    /// </summary>
    /// created by: PTTAM
    [AttributeUsage(AttributeTargets.Property)]
    public class DataLength : Attribute
    {
        public int Length { get; set; }

        public DataLength(int length)
        {
            Length = length;
        }
    }

    /// <summary>
    /// Attribute dùng để validate ngày
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateField : Attribute
    {

    }
    #endregion
}
