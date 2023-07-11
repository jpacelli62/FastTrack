using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Faaast.Metadata;

namespace Faaast.Orm.Reader
{
    public static class Extensions
    {
        private static readonly Dictionary<Type, DbType> TypeMap = new()
        {
            [typeof(byte)] = DbType.Byte,
            [typeof(sbyte)] = DbType.SByte,
            [typeof(short)] = DbType.Int16,
            [typeof(ushort)] = DbType.UInt16,
            [typeof(int)] = DbType.Int32,
            [typeof(uint)] = DbType.UInt32,
            [typeof(long)] = DbType.Int64,
            [typeof(ulong)] = DbType.UInt64,
            [typeof(float)] = DbType.Single,
            [typeof(double)] = DbType.Double,
            [typeof(decimal)] = DbType.Decimal,
            [typeof(bool)] = DbType.Boolean,
            [typeof(string)] = DbType.String,
            [typeof(char)] = DbType.StringFixedLength,
            [typeof(Guid)] = DbType.Guid,
            [typeof(DateTime)] = DbType.DateTime,
            [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
            [typeof(TimeSpan)] = DbType.Time,
            [typeof(byte[])] = DbType.Binary,
            [typeof(byte?)] = DbType.Byte,
            [typeof(sbyte?)] = DbType.SByte,
            [typeof(short?)] = DbType.Int16,
            [typeof(ushort?)] = DbType.UInt16,
            [typeof(int?)] = DbType.Int32,
            [typeof(uint?)] = DbType.UInt32,
            [typeof(long?)] = DbType.Int64,
            [typeof(ulong?)] = DbType.UInt64,
            [typeof(float?)] = DbType.Single,
            [typeof(double?)] = DbType.Double,
            [typeof(decimal?)] = DbType.Decimal,
            [typeof(bool?)] = DbType.Boolean,
            [typeof(char?)] = DbType.StringFixedLength,
            [typeof(Guid?)] = DbType.Guid,
            [typeof(DateTime?)] = DbType.DateTime,
            [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
            [typeof(TimeSpan?)] = DbType.Time,
            [typeof(object)] = DbType.Object
        };

        public static DbType ToDbType(this Type type) => TypeMap.ContainsKey(type) ?
            TypeMap[type] :
            throw new ArgumentException($"Type \"{type.Name}\" cannot be converted to System.Data.DbType");

        public static string Sanitize(this string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                switch (name[0])
                {
                    case '@':
                    case ':':
                    case '?':
                        return name.Substring(1);
                }
            }

            return name;
        }

        public static DbParameter AddParameter(this DbCommand command, string name, object value, Type valueType, ParameterDirection direction)
        {
            var dbType = valueType != null && value != DBNull.Value ? valueType.ToDbType() : (DbType)0;
            return AddParameter(
                command,
                name,
                value,
                dbType,
                value is null || value == DBNull.Value,
                dbType == DbType.String ? Encoding.Unicode.GetByteCount((string)value) : 0,
                direction);
        }

        public static DbParameter AddParameter(this DbCommand command, string name, object value, DbType dbType, bool isNullable, int size, ParameterDirection direction)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name.Sanitize();
            parameter.Value = value ?? DBNull.Value;
            parameter.Direction = direction;
            parameter.Size = size;
            parameter.IsNullable = isNullable;
            parameter.DbType = dbType;

            if (value == null || value == DBNull.Value)
            {
                parameter.IsNullable = true;
            }
            else
            {
                if (parameter.DbType == DbType.String)
                {
                    parameter.Size = Encoding.Unicode.GetByteCount((string)value);
                }
            }

            command.Parameters.Add(parameter);
            return parameter;
        }

        public static void AddParameters(this DbCommand command, object parameters, IObjectMapper mapper)
        {
            if (parameters != null)
            {
                if (parameters is IDictionary dictionary)
                {
                    foreach (var key in dictionary.Keys)
                    {
                        var value = dictionary[key];
                        command.AddParameter(key.ToString(), value, value?.GetType(), ParameterDirection.Input);
                    }
                }
                else
                {
                    var map = mapper.Get(parameters.GetType());
                    foreach (var property in map)
                    {
                        command.AddParameter(property.Name, property.Read(parameters), property.Type, ParameterDirection.Input);
                    }
                }
            }
        }
    }
}
