using System;
using System.Collections;
using System.Collections.Generic;
using Faaast.Metadata;
using Xunit;

namespace Faaast.Tests.Metadata
{
    public  abstract class DtoTests
    {
        public class ParameterizedContructorClass
        {
            public int MyProperty { get; set; }

            public ParameterizedContructorClass(int myProperty) => this.MyProperty = myProperty;
        }

        public class ParameterlessContructorClass
        {
            public int MyProperty { get; set; }

            public ParameterlessContructorClass()
            {
                // do nothing
            }
        }

        private static readonly Metadata<IDtoProperty, bool?> IsAmazing = new(nameof(IsAmazing));

        public IObjectMapper Mapper { get; set; }

        public IDtoClass Dto { get; set; }


        public int InfField = 234;
        public bool? NullableBoolProperty { get; set; } = true;


        public DateTime ReadOnlyValueTypeProperty { get; } = DateTime.Today;
        public ParameterlessContructorClass ClassField;
        public ParameterlessContructorClass ClassProperty { get; set; } = null;
        public int PrivateSetProperty { get; private set; } = 345;
        public int PrivateGetProperty { private get;  set; } = 3456;

#pragma warning disable IDE0051 // Supprimer les membres privés non utilisés
        private int PrivateProperty { get; set; } = 5445;
#pragma warning restore IDE0051 // Supprimer les membres privés non utilisés

        protected DtoTests(IObjectMapper mapper)
        {
            this.Mapper = mapper;
            this.Dto = mapper.Get(typeof(DtoTests));
        }

        [Fact]
        public void Get_Set_Has_Meta()
        {
            var property = this.Dto[nameof(this.ReadOnlyValueTypeProperty)];
            Assert.False(property.Has(IsAmazing));
            property.Set(IsAmazing, true);
            Assert.True(property.Has(IsAmazing));
            Assert.True(property.Get(IsAmazing));
        }

        [Fact]
        public void Read_Write_NullableProperty()
        {
            var property = this.Dto[nameof(this.NullableBoolProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.True(property.Nullable);
            Assert.Equal(typeof(bool?), property.Type);

            this.NullableBoolProperty = false;
            Assert.False((bool?)property.Read(this));

            property.Write(this, true);
            Assert.True((bool?)property.Read(this));
            Assert.True(this.NullableBoolProperty);

            property.Write(this, null);
            Assert.Null((bool?)property.Read(this));
            Assert.Null(this.NullableBoolProperty);
        }

        [Fact]
        public void Read_Write_Field()
        {
            var property = this.Dto[nameof(this.InfField)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite); 
            Assert.False(property.Nullable);
            Assert.Equal(typeof(int), property.Type);

            this.InfField = 24;
            Assert.Equal(24, (int)property.Read(this));

            property.Write(this, 12);
            Assert.Equal(12, (int)property.Read(this));
            Assert.Equal(12, this.InfField);

            Assert.Throws<NullReferenceException>(() => property.Write(this, null));
        }

        [Fact]
        public void Read_ReadOnlyPropery()
        {
            var property = this.Dto[nameof(this.ReadOnlyValueTypeProperty)];
            Assert.True(property.CanRead);
            Assert.False(property.CanWrite);
            Assert.False(property.Nullable);
            Assert.Equal(typeof(DateTime), property.Type);

            Assert.Equal(DateTime.Today, (DateTime)property.Read(this));
            Assert.Throws<InvalidOperationException>(() => property.Write(this, DateTime.Now));
        }

        [Fact]
        public void Read_ReadOnlyPropery_PrivateSet()
        {
            var property = this.Dto[nameof(this.PrivateSetProperty)];
            Assert.True(property.CanRead);
            Assert.False(property.CanWrite);
            Assert.False(property.Nullable);
            Assert.Equal(typeof(int), property.Type);

            Assert.Equal(345, (int)property.Read(this));
            Assert.Throws<InvalidOperationException>(() => property.Write(this, 456));
        }

        [Fact]
        public void Write_WriteOnlyPropery_PrivateGet()
        {
            var property = this.Dto[nameof(this.PrivateGetProperty)];
            Assert.False(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.False(property.Nullable);
            Assert.Equal(typeof(int), property.Type);

            Assert.Equal(3456, this.PrivateGetProperty);
            property.Write(this, 999);
            Assert.Equal(999, this.PrivateGetProperty);
            Assert.Throws<InvalidOperationException>(() => property.Read(this));
        }

        [Fact]
        public void Miss_PrivateProperty()
        {
            var property = this.Dto[nameof(this.PrivateProperty)];
            Assert.Null(property);
        }

        void TestPropertiesCollection(ICollection<string> props)
        {
            Assert.All(new string[] {
                nameof(InfField),
                nameof(this.NullableBoolProperty),
                nameof(this.ReadOnlyValueTypeProperty),
                nameof(this.ClassField),
                nameof(this.PrivateSetProperty),
                nameof(this.PrivateGetProperty)
            }, x => props.Contains(x));
        }
        
        [Fact]
        public void GetEnumerator()
        {
            List<string> props = new();
            foreach (var prop in this.Dto)
            {
                props.Add(prop.Name);
            }
            TestPropertiesCollection(props);
        }

        [Fact]
        public void IEnumerableGetEnumerator()
        {
            List<string> props = new();
            foreach (var prop in (IEnumerable)this.Dto)
            {
                props.Add(((IDtoProperty)prop).Name);
            }

            TestPropertiesCollection(props);
        }

        [Fact]
        public void Name()
        {
            Assert.Equal(nameof(DtoTests), this.Dto.Name);
        }

        [Fact]
        public void Type()
        {
            Assert.Equal(typeof(DtoTests), this.Dto.Type);
        }

        [Fact]
        public void CreateInstance()
        {
            var dto = this.Mapper.Get(typeof(ParameterlessContructorClass));
            Assert.NotNull(dto.CreateInstance());
            Assert.Equal(1, dto.PropertiesCount);
        }

        [Fact]
        public void CreateInstance_ParameterizedContructor()
        {
            var dto = this.Mapper.Get(typeof(ParameterizedContructorClass));
            Assert.Throws<InvalidOperationException>(() => dto.CreateInstance());
        }


    }
}
