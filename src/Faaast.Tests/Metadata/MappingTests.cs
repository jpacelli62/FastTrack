using System;
using System.Collections;
using Faaast.Metadata;
using Faaast.Tests.Orm.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Faaast.Tests.Metadata
{
    public class MappingTests
    {
        private static readonly Metadata<IDtoProperty, bool?> IsAmazing = new(nameof(IsAmazing));

        IObjectMapper Mapper { get; set; }

        SampleModelDto SampleModelDto { get; set; }

        IDtoClass Dto { get; set; }

        public MappingTests()
        {
            var services = new ServiceCollection();
            services.AddMetadata();
            this.Mapper = services.BuildServiceProvider().GetService<IObjectMapper>();
            this.SampleModelDto = new SampleModelDto();
            this.Dto = this.Mapper.Get(typeof(SampleModelDto));
            Assert.NotNull(this.Dto);
        }

        [Fact]
        public void Can_attach_value()
        {
            var property = this.Dto[nameof(this.SampleModelDto.ReadWriteProperty)];
            Assert.Null(property.Get(IsAmazing));
            property.Set(IsAmazing, true);
            Assert.True(property.Has(IsAmazing));
            Assert.True(property.Get(IsAmazing));
        }

        [Fact]
        public void Has_properties_filled()
        {
            Assert.Equal(nameof(this.SampleModelDto), this.Dto.Name);
            Assert.Equal(typeof(SampleModelDto), this.Dto.Type);
        }

        [Fact]
        public void Can_read_ReadWriteProperty()
        {
            var property = this.Dto[nameof(this.SampleModelDto.ReadWriteProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);

            var value = 98798654;
            property.Write(this.SampleModelDto, value);
            Assert.Equal(value, property.Read(this.SampleModelDto));
            Assert.Throws<NullReferenceException>(() => property.Write(this.SampleModelDto, null));
        }

        [Fact]
        public void Can_read_IntMember()
        {
            var property = this.Dto[nameof(this.SampleModelDto.IntMember)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Equal(234, (int)property.Read(this.SampleModelDto));
            var value = 567;
            property.Write(this.SampleModelDto, value);
            Assert.Equal(value, property.Read(this.SampleModelDto));
            Assert.Throws<NullReferenceException>(() => property.Write(this.SampleModelDto, null));
        }

        [Fact]
        public void Can_read_NullableBoolProperty()
        {
            var property = this.Dto[nameof(this.SampleModelDto.NullableBoolProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.True((bool?)property.Read(this.SampleModelDto));

            property.Write(this.SampleModelDto, null);
            Assert.Null(property.Read(this.SampleModelDto));

            property.Write(this.SampleModelDto, false);
            Assert.False((bool?)property.Read(this.SampleModelDto));
        }

        [Fact]
        public void Can_read_ReadOnlyProperty()
        {
            var property = this.Dto[nameof(this.SampleModelDto.ReadOnlyProperty)];
            Assert.True(property.CanRead);
            Assert.False(property.CanWrite);
            Assert.Equal(234, (int)property.Read(this.SampleModelDto));
            Assert.Throws<InvalidOperationException>(() => property.Write(this.SampleModelDto, 456));
        }

        [Fact]
        public void Cant_read_private_set()
        {
            var property = this.Dto[nameof(this.SampleModelDto.PrivateSetProperty)];
            Assert.True(property.CanRead);
            Assert.False(property.CanWrite);
            Assert.Equal(345, (int?)property.Read(this.SampleModelDto));
            Assert.Throws<InvalidOperationException>(() => property.Write(this.SampleModelDto, 456));
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
            var property = this.Dto[nameof(this.SampleModelDto.WriteProperty)];
            Assert.False(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Equal(456, this.SampleModelDto._writeProperty);
            property.Write(this.SampleModelDto, 567);
            Assert.Equal(567, this.SampleModelDto._writeProperty);
            Assert.Throws<InvalidOperationException>(() => property.Read(this.SampleModelDto));
        }

        [Fact]
        public void Throw_if_no_constructor()
        {
            var services = new ServiceCollection();
            services.AddMetadata();
            var mapper = services.BuildServiceProvider().GetService<IObjectMapper>();
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
            var property = this.Dto[nameof(this.SampleModelDto.RefProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Equal("Hello world", (string)property.Read(this.SampleModelDto));
            property.Write(this.SampleModelDto, "Lorem ipsum");
            Assert.Equal("Lorem ipsum", (string)property.Read(this.SampleModelDto));
            property.Write(this.SampleModelDto, null);
            Assert.Null((string)property.Read(this.SampleModelDto));
        }

        [Fact]
        public void Can_read_StructProperty()
        {
            var property = this.Dto[nameof(this.SampleModelDto.StructProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Equal(DateTime.Today, (DateTime)property.Read(this.SampleModelDto));
            property.Write(this.SampleModelDto, DateTime.Today.AddDays(1));
            Assert.Equal(DateTime.Today.AddDays(1), (DateTime)property.Read(this.SampleModelDto));
        }

        [Fact]
        public void Can_read_ComplexType()
        {
            var property = this.Dto[nameof(this.SampleModelDto.ComplexType)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Null(property.Read(this.SampleModelDto));
            property.Write(this.SampleModelDto, new SampleModelDto());
            Assert.NotNull(property.Read(this.SampleModelDto));
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
