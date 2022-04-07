using System;

namespace Faaast.Orm.Converters
{
    public class EnumToIntValueConverter<TEnum> : IValueConverter where TEnum : Enum 
    {
        public object FromDb(object value, Type targetType)
        {
            if(value != DBNull.Value)
            { 
                return Enum.ToObject(typeof(TEnum), value);
            }

            return DBNull.Value;
        }

        public object ToDb(object value, Type sourceType)
        {
            return (int)value;
        }
    }
}
