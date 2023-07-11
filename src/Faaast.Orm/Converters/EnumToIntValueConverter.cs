using System;

namespace Faaast.Orm.Converters
{
    public class EnumToIntValueConverter<TEnum> : IValueConverter where TEnum : Enum 
    {
        public object FromDb(object value, Type targetType) =>
            value != DBNull.Value ? Enum.ToObject(typeof(TEnum), value) : DBNull.Value;

        public object ToDb(object value, Type sourceType) => 
            value == null ? 0 : (object)(int)value;
    }
}
