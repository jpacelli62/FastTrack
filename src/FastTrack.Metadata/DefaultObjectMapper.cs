using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace FastTrack.Metadata
{
    public class DefaultObjectMapper : IObjectMapper
    {
        private Dictionary<Type, DtoClass> Definitions { get; set; } = new Dictionary<Type, DtoClass>();

        private readonly ReadWriteSync _sync = new ReadWriteSync();

        public DtoClass Get(Type type)
        {
            using (_sync.ReadAccess(10000))
            {
                if (!Definitions.ContainsKey(type))
                {
                    using (_sync.UpgradeToWriteAccess(10000))
                    {
                        if (!Definitions.ContainsKey(type))
                        {
                            Definitions.Add(type, Build(type));
                        }
                    }

                }

                return Definitions[type];
            }
        }


        public DtoClass Build(Type type)
        {
            var result = new DtoClass(type);
            var constructor = type.GetConstructor(new Type[0]);
            result.Activator = GenerateActivator(type, constructor);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var newProp = new DtoProperty(property.Name, property.PropertyType);
                if (property.CanRead)
                {
                    newProp.Read = GenerateGetter(type, property);
                    newProp.CanRead = true;
                }

                if (property.CanWrite)
                {
                    newProp.Write = GenerateSetter(type, property);
                    newProp.CanWrite = true;
                }

                result[property.Name] = newProp;
            }

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                result[field.Name] = new DtoProperty(field.Name, field.FieldType)
                {
                    Read = GenerateGetter(type, field),
                    Write = GenerateSetter(type, field),
                    CanRead = true,
                    CanWrite = true
                };
            }

            return result;
        }

        public static Func<object> GenerateActivator(Type type, ConstructorInfo constructor)
        {
            var callNew = Expression.New(constructor);
            var cast = Expression.Convert(callNew, typeof(object));
            var exp = (Func<object>)Expression.Lambda(cast).Compile();
            return exp;
        }

        public static Func<object, object> GenerateGetter(Type type, MemberInfo member)
        {
            var instance = Expression.Parameter(typeof(object), "x");
            var castedInstance = Expression.Convert(instance, type);

            Expression call;
            if (member is PropertyInfo property)
                call = Expression.Call(castedInstance, property.GetGetMethod(true));
            else
                call = Expression.MakeMemberAccess(castedInstance, member);

            var cast = Expression.Convert(call, typeof(object));
            var exp = (Func<object, object>)Expression.Lambda(cast, instance).Compile();
            return exp;
        }

        public static Action<object, object> GenerateSetter(Type type, MemberInfo member)
        {
            var value = Expression.Parameter(typeof(object), "value");
            var instance = Expression.Parameter(typeof(object), "x");
            var castedInstance = Expression.Convert(instance, type);

            Expression call;
            if (member is PropertyInfo property)
            {
                var castedValue = Expression.Convert(value, property.PropertyType);
                call = Expression.Call(castedInstance, property.GetSetMethod(true), castedValue);
            }
            else
            {
                var fieldType = ((FieldInfo)member).FieldType;
                Expression castedValue = fieldType.IsValueType ? Expression.Unbox(value, fieldType) : Expression.Convert(value, fieldType);
                MemberExpression fieldExp = Expression.Field(castedInstance, (FieldInfo)member);
                call = Expression.Assign(fieldExp, castedValue);
            }

            var exp = Expression.Lambda<Action<object, object>>(call, instance, value).Compile();
            return exp;
        }
    }
}
