using System;
using System.Collections;
using Faaast.Metadata;
using Faaast.Tests.Orm.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Faaast.Tests.Metadata
{
    public class MappingEmitTests
    {
        private static readonly Metadata<IDtoProperty, bool?> IsAmazing = new(nameof(IsAmazing));

        IObjectMapper Mapper { get; set; }

        SampleDto SampleDto { get; set; }

        IDtoClass Dto { get; set; }

        public MappingEmitTests()
        {
            this.Mapper = new EmitObjectMapper();
            this.SampleDto = new SampleDto();
            this.Dto = this.Mapper.Get(typeof(SampleDto));
            Assert.NotNull(this.Dto);
        }

        [Fact]
        public void Can_attach_value()
        {
            var property = this.Dto[nameof(this.SampleDto.ReadWriteProperty)];
            Assert.Null(property.Get(IsAmazing));
            property.Set(IsAmazing, true);
            Assert.True(property.Has(IsAmazing));
            Assert.True(property.Get(IsAmazing));
        }

        [Fact]
        public void Has_properties_filled()
        {
            Assert.Equal(typeof(SampleDto).Name, this.Dto.Name);
            Assert.Equal(typeof(SampleDto), this.Dto.Type);
        }

        [Fact]
        public void Can_read_ReadWriteProperty()
        {
            var property = this.Dto[nameof(this.SampleDto.ReadWriteProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);

            var value = 98798654;
            property.Write(this.SampleDto, value);
            Assert.Equal(value, property.Read(this.SampleDto));
            Assert.Throws<NullReferenceException>(() => property.Write(this.SampleDto, null));
        }

        [Fact]
        public void Can_read_IntMember()
        {
            var property = this.Dto[nameof(this.SampleDto.IntMember)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Equal(234, (int)property.Read(this.SampleDto));
            var value = 567;
            property.Write(this.SampleDto, value);
            Assert.Equal(value, property.Read(this.SampleDto));
            Assert.Throws<NullReferenceException>(() => property.Write(this.SampleDto, null));
        }

        [Fact]
        public void Can_read_NullableBoolProperty()
        {
            var property = this.Dto[nameof(this.SampleDto.NullableBoolProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.True((bool?)property.Read(this.SampleDto));

            property.Write(this.SampleDto, null);
            Assert.Null(property.Read(this.SampleDto));

            property.Write(this.SampleDto, false);
            Assert.False((bool?)property.Read(this.SampleDto));
        }

        [Fact]
        public void Can_read_ReadOnlyProperty()
        {
            var property = this.Dto[nameof(this.SampleDto.ReadOnlyProperty)];
            Assert.True(property.CanRead);
            Assert.False(property.CanWrite);
            Assert.Equal(234, (int)property.Read(this.SampleDto));
            Assert.Throws<InvalidOperationException>(() => property.Write(this.SampleDto, 456));
        }

        [Fact]
        public void Cant_read_private_set()
        {
            var property = this.Dto[nameof(this.SampleDto.PrivateSetProperty)];
            Assert.True(property.CanRead);
            Assert.False(property.CanWrite);
            Assert.Equal(345, (int?)property.Read(this.SampleDto));
            Assert.Throws<InvalidOperationException>(() => property.Write(this.SampleDto, 456));
        }

        [Fact]
        public void Cant_read_PrivateProperty()
        {
            var property = this.Dto["PrivateProperty"];
            Assert.Null(property);
        }

        [Fact]
        public void Cant_read_WriteProperty()
        {
            var property = this.Dto[nameof(this.SampleDto.WriteProperty)];
            Assert.False(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Equal(456, this.SampleDto._writeProperty);
            property.Write(this.SampleDto, 567);
            Assert.Equal(567, this.SampleDto._writeProperty);
            Assert.Throws<InvalidOperationException>(() => property.Read(this.SampleDto));
        }

        [Fact]
        public void Throw_if_no_constructor()
        {
            var dto = this.Mapper.Get(typeof(SampleClassA));
            Assert.NotNull(dto);
            var member = dto["MyComplexMember"];
            Assert.NotNull(member);
            Assert.Equal(nameof(SampleClassA.MyComplexMember), member.Name);
            Assert.Equal(typeof(object), member.Type);
            Assert.Throws<InvalidOperationException>(() => dto.CreateInstance());
        }

        [Fact]
        public void Can_read_RefProperty()
        {
            var property = this.Dto[nameof(this.SampleDto.RefProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Equal("Hello world", (string)property.Read(this.SampleDto));
            property.Write(this.SampleDto, "Lorem ipsum");
            Assert.Equal("Lorem ipsum", (string)property.Read(this.SampleDto));
            property.Write(this.SampleDto, null);
            Assert.Null((string)property.Read(this.SampleDto));
        }

        [Fact]
        public void Can_read_StructProperty()
        {
            var property = this.Dto[nameof(this.SampleDto.StructProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Equal(DateTime.Today, (DateTime)property.Read(this.SampleDto));
            property.Write(this.SampleDto, DateTime.Today.AddDays(1));
            Assert.Equal(DateTime.Today.AddDays(1), (DateTime)property.Read(this.SampleDto));
        }

        [Fact]
        public void Can_read_ComplexType()
        {
            var property = this.Dto[nameof(this.SampleDto.ComplexType)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Null(property.Read(this.SampleDto));
            property.Write(this.SampleDto, new SampleDto());
            Assert.NotNull(property.Read(this.SampleDto));
        }

        [Fact]
        public void Can_enumerate_properties()
        {
            var nbProps = 0;
            foreach (var prop in this.Dto)
            {
                nbProps++;
            }

            Assert.Equal(10, nbProps);
        }

        [Fact]
        public void Can_enumerate_properties_default_enumerator()
        {
            var nbProps = 0;
            foreach (var prop in (IEnumerable)this.Dto)
            {
                nbProps++;
            }

            Assert.Equal(10, nbProps);
        }
    }
}
