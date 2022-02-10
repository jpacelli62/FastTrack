using System;

namespace Faaast.Orm.Converters
{
    public class EnumToIntValueConverter<TEnum> : IValueConverter where TEnum : Enum 
    {
        public object FromDb(object value, Type targetType) => Enum.ToObject(typeof(TEnum), value);

        public object ToDb(object value, Type sourceType) => (int)value;
    }
}
