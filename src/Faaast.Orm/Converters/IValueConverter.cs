using System;

namespace Faaast.Orm.Converters
{
    public interface IValueConverter
    {
        object FromDb(object value, Type targetType);

        object ToDb(object value, Type sourceType);
    }
}
