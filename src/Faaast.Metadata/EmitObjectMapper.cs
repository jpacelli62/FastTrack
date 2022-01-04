using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Faaast.Metadata
{
    public class EmitObjectMapper : IObjectMapper
    {
        private readonly AssemblyBuilder _assembly;
        private readonly ModuleBuilder _module;
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
                            this.Definitions.Add(type, this.Build(type));
                        }
                    }
                }

                return this.Definitions[type];
            }
        }

        public EmitObjectMapper()
        {
            var aName = new AssemblyName("GeneratedObjectMapper");
#if  NET461
            _assembly = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            _module = _assembly.DefineDynamicModule(aName.Name, aName.Name + ".dll", false);
#elif NETSTANDARD2_0 || NET5_0
            _assembly = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            _module = _assembly.DefineDynamicModule(aName.Name);
#endif
        }

        public IDtoClass Build(Type type)
        {
            var dtoType = CreateDtoClass(type, _module);
            return (IDtoClass)Activator.CreateInstance(dtoType);
        }

        private static Type CreateDtoClass(Type type, ModuleBuilder module)
        {
            var baseType = typeof(DtoClass);
            var tb = module.DefineType(Guid.NewGuid().ToString("N"), TypeAttributes.Public, baseType);

            var propertyTypes = new Dictionary<MemberInfo, ConstructorBuilder>();
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                propertyTypes.Add(property, AddNestedType(property, type, tb));
            }

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                propertyTypes.Add(field, AddNestedType(field, type, tb));
            }

            AddConstructor(type, baseType, tb, propertyTypes);
            AddCreateInstanceMethod(type, tb);
            return tb.CreateTypeInfo();
        }

        private static void AddConstructor(Type type, Type baseType, TypeBuilder tb, Dictionary<MemberInfo, ConstructorBuilder> propertyTypes)
        {
            var baseConstructor = baseType.GetConstructor(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null, new Type[1] { typeof(Type) }, null);
            var constructor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, Type.EmptyTypes);
            var il = constructor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldtoken, type);
            il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[1] { typeof(RuntimeTypeHandle) }));
            il.Emit(OpCodes.Call, baseConstructor);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Nop);

            var dicoType = typeof(Dictionary<string, IDtoProperty>);
            var add = dicoType.GetMethod("Add");
            var baseDico = typeof(DtoClass).GetProperty("Properties", BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
            foreach (var property in propertyTypes)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, baseDico.GetMethod);
                il.Emit(OpCodes.Ldstr, property.Key.Name);
                il.Emit(OpCodes.Newobj, property.Value);
                il.Emit(OpCodes.Callvirt, add);
                il.Emit(OpCodes.Nop);
            }

            il.Emit(OpCodes.Ret);
        }

        private static void AddCreateInstanceMethod(Type type, TypeBuilder tb)
        {
            var method = tb.DefineMethod("CreateInstance", MethodAttributes.Public | MethodAttributes.Virtual, typeof(object), Type.EmptyTypes);
            var il = method.GetILGenerator();
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if(ctor == null)
            {
                ThrowInvalidOperationException(il);
            }
            else
            {
                il.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Ret);
            }

            tb.DefineMethodOverride(method, typeof(DtoClass).GetMethod("CreateInstance"));
        }

        private static ConstructorBuilder AddNestedType(MemberInfo member, Type type, TypeBuilder tb)
        {
            var baseType = typeof(DtoProperty);
            var subclass = tb.DefineNestedType(member.Name + "Property", TypeAttributes.Class | TypeAttributes.NestedPublic, baseType);

            subclass.AddInterfaceImplementation(typeof(IDtoProperty));
            AddReadMethod(member, type, subclass);
            AddWriteMethod(member, type, subclass);

            var nestedConstructor = subclass.DefineConstructor(
                MethodAttributes.Public |
                MethodAttributes.HideBySig |
                MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName,
                CallingConventions.HasThis,
                Type.EmptyTypes);

            bool canRead, canWrite;
            Type membertype;
            if (member is PropertyInfo property)
            {
                canRead = property.CanRead && !(property.GetGetMethod()?.IsPrivate != false);
                canWrite = property.CanWrite && !(property.GetSetMethod()?.IsPrivate != false);
                membertype = property.PropertyType;
            }
            else
            {
                canRead = canWrite = true;
                membertype = ((FieldInfo)member).FieldType;
            }

            var ctorIl = nestedConstructor.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldstr, member.Name);
            ctorIl.Emit(OpCodes.Ldtoken, membertype);
            ctorIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[1] { typeof(RuntimeTypeHandle) }));
            ctorIl.Emit(OpCodes.Call, baseType.GetConstructor(new Type[] { typeof(string), typeof(Type) }));
            ctorIl.Emit(OpCodes.Nop);
            ctorIl.Emit(OpCodes.Nop);

            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(canRead ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            ctorIl.Emit(OpCodes.Call, baseType.GetProperty(nameof(DtoProperty.CanRead)).SetMethod);
            ctorIl.Emit(OpCodes.Nop);

            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(canWrite ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            ctorIl.Emit(OpCodes.Call, baseType.GetProperty(nameof(DtoProperty.CanWrite)).SetMethod);
            ctorIl.Emit(OpCodes.Nop);

            ctorIl.Emit(OpCodes.Ret);
            subclass.CreateTypeInfo();

            return nestedConstructor;
        }

        private static void AddReadMethod(MemberInfo member, Type type, TypeBuilder tb)
        {
            // public object Read(object instance) => ((Customer)instance).Id;
            var method = tb.DefineMethod("Read", MethodAttributes.Public | MethodAttributes.Virtual, typeof(object), new Type[] { typeof(object) });
            var il = method.GetILGenerator();
            if (member is PropertyInfo property)
            {
                if (property.CanRead && !(property.GetGetMethod()?.IsPrivate != false))
                {
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Castclass, type);
                    il.Emit(OpCodes.Callvirt, property.GetGetMethod(true));
                    il.Emit(OpCodes.Box, property.PropertyType);
                    il.Emit(OpCodes.Ret);
                }
                else
                {
                    ThrowInvalidOperationException(il);
                }
            }
            else
            {
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, type);
                il.Emit(OpCodes.Ldfld, (FieldInfo)member);
                il.Emit(OpCodes.Box, ((FieldInfo)member).FieldType);
                il.Emit(OpCodes.Ret);
            }

            tb.DefineMethodOverride(method, typeof(DtoProperty).GetMethod("Read"));
        }

        private static void AddWriteMethod(MemberInfo member, Type type, TypeBuilder tb)
        {
            // public object Read(object instance) => ((Customer)instance).Id;
            var method = tb.DefineMethod("Write", MethodAttributes.Public | MethodAttributes.Virtual, null, new Type[] { typeof(object), typeof(object) });
            var il = method.GetILGenerator();
            if (member is PropertyInfo property)
            {
                if (property.CanWrite && !(property.GetSetMethod()?.IsPrivate != false))
                {
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Castclass, type);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Unbox_Any, property.PropertyType);
                    il.Emit(OpCodes.Callvirt, property.GetSetMethod(true));
                    il.Emit(OpCodes.Ret);
                }
                else
                {
                    ThrowInvalidOperationException(il);
                }
            }
            else
            {
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, type);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Unbox_Any, ((FieldInfo)member).FieldType);
                il.Emit(OpCodes.Stfld, (FieldInfo)member);
                il.Emit(OpCodes.Ret);

            }

            tb.DefineMethodOverride(method, typeof(DtoProperty).GetMethod("Write"));
        }

        private static void ThrowInvalidOperationException(ILGenerator il)
        {
            il.Emit(OpCodes.Newobj, typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Throw);
        }
    }
}
