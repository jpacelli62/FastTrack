using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Faaast.Metadata
{
    public class DefaultObjectMapper : IObjectMapper
    {
        private Dictionary<Type, IDtoClass> Definitions { get; } = new Dictionary<Type, IDtoClass>();

        private readonly ReadWriteSync _sync = new();

        public IDtoClass Get(Type type)
        {
            using (_sync.ReadAccess(10000))
            {
                if (!this.Definitions.ContainsKey(type))
                {
                    using (_sync.UpgradeToWriteAccess(10000))
                    {
                        if (!this.Definitions.ContainsKey(type))
                        {
                            this.Definitions.Add(type, Build(type));
                        }
                    }
                }

                return this.Definitions[type];
            }
        }

        internal static bool IsNullableType(Type type, out Type nullableUnderlyingType)
        {
            nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            return nullableUnderlyingType != null || type.IsClass || type.IsInterface;
        }

        public static IDtoClass Build(Type type)
        {
            var result = new LambdaDto(type);
            var constructor = type.GetConstructor(Array.Empty<Type>());
            result.Lambda = GenerateActivator(type, constructor);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyType = property.PropertyType;
                var newProp = new DtoProperty(property.Name, propertyType);
                var get = property.GetGetMethod();
                if (property.CanRead && get != null && !get.IsPrivate)
                {
                    newProp.ReadFunc = GenerateGetter(type, property);
                    newProp.CanRead = true;
                }
                else
                {
                    newProp.ReadFunc = x => throw new InvalidOperationException();
                }

                var set = property.GetSetMethod();
                if (property.CanWrite && set != null && !set.IsPrivate)
                {
                    newProp.WriteFunc = GenerateSetter(type, property);
                    newProp.CanWrite = true;
                }
                else
                {
                    newProp.WriteFunc = (x, y) => throw new InvalidOperationException();
                }

                newProp.Nullable = IsNullableType(propertyType, out var nullType);
                newProp.NullableUnderlyingType = nullType;
                result[property.Name] = newProp;
            }

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                result[field.Name] = new DtoProperty(field.Name, field.FieldType)
                {
                    ReadFunc = GenerateGetter(type, field),
                    WriteFunc = GenerateSetter(type, field),
                    CanRead = true,
                    CanWrite = true
                };
            }

            return result;
        }

        public static Func<object> GenerateActivator(Type type, ConstructorInfo constructor)
        {
            if (constructor == null)
            {
                return () => throw new InvalidOperationException($"No parameterless constructor on type \"{type.FullName}\"");
            }

            var callNew = Expression.New(constructor);
            var cast = Expression.Convert(callNew, typeof(object));
            var exp = (Func<object>)Expression.Lambda(cast).Compile();
            return exp;
        }

        public static Func<object, object> GenerateGetter(Type type, MemberInfo member)
        {
            var instance = Expression.Parameter(typeof(object), "x");
            var castedInstance = Expression.Convert(instance, type);

            var call = member is PropertyInfo property
                ? Expression.Call(castedInstance, property.GetGetMethod(true))
                : (Expression)Expression.MakeMemberAccess(castedInstance, member);
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
                var fieldExp = Expression.Field(castedInstance, (FieldInfo)member);
                call = Expression.Assign(fieldExp, castedValue);
            }

            var exp = Expression.Lambda<Action<object, object>>(call, instance, value).Compile();
            return exp;
        }
    }
}
