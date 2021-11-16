using Faaast.Metadata;
using Faaast.Tests.Orm.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using Xunit;

namespace Faaast.Tests.Metadata
{
    public class MappingTests
    {
        private static readonly Metadata<DtoProperty, bool?> IsAmazing = new Metadata<DtoProperty, bool?>(nameof(IsAmazing));

        IObjectMapper Mapper { get; set; }

        SampleModelDto SampleModelDto { get; set; }

        DtoClass Dto { get; set; }

        public MappingTests()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddMetadata();
            Mapper = services.BuildServiceProvider().GetService<IObjectMapper>();
            SampleModelDto = new SampleModelDto();
            Dto = Mapper.Get(typeof(SampleModelDto));
            Assert.NotNull(Dto);
        }

        [Fact]
        public void Can_attach_value()
        {
            var property = Dto[nameof(SampleModelDto.ReadWriteProperty)];
            Assert.Null(property.Get(IsAmazing));
            property.Set(IsAmazing, true);
            Assert.True(property.Get(IsAmazing));
        }

        [Fact]
        public void Can_read_ReadWriteProperty()
        {
            var property = Dto[nameof(SampleModelDto.ReadWriteProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);

            int value = 98798654;
            property.Write(SampleModelDto, value);
            Assert.Equal(value, property.Read(SampleModelDto));
            Assert.Throws<NullReferenceException>(() => property.Write(SampleModelDto, null));
        }

        [Fact]
        public void Can_read_IntMember()
        {
            var property = Dto[nameof(SampleModelDto.IntMember)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Equal(234, (int)property.Read(SampleModelDto));
            int value = 567;
            property.Write(SampleModelDto, value);
            Assert.Equal(value, property.Read(SampleModelDto));
            Assert.Throws<NullReferenceException>(() => property.Write(SampleModelDto, null));
        }

        [Fact]
        public void Can_read_NullableBoolProperty()
        {
            var property = Dto[nameof(SampleModelDto.NullableBoolProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.True((bool?)property.Read(SampleModelDto));

            property.Write(SampleModelDto, null);
            Assert.Null(property.Read(SampleModelDto));

            property.Write(SampleModelDto, false);
            Assert.False((bool?)property.Read(SampleModelDto));
        }

        [Fact]
        public void Can_read_ReadOnlyProperty()
        {
            var property = Dto[nameof(SampleModelDto.ReadOnlyProperty)];
            Assert.True(property.CanRead);
            Assert.False(property.CanWrite);
            Assert.Equal(234, (int)property.Read(SampleModelDto));
            Assert.Throws<NullReferenceException>(() => property.Write(SampleModelDto, 456));
        }

        [Fact]
        public void Can_read_private_set()
        {
            var property = Dto[nameof(SampleModelDto.PrivateSetProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Equal(345, (int?)property.Read(SampleModelDto));
            property.Write(SampleModelDto, 456);
            Assert.Equal(456, (int)property.Read(SampleModelDto));
        }

        [Fact]
        public void Cant_read_PrivateProperty()
        {
            var property = Dto["PrivateProperty"];
            Assert.Null(property);
        }

        [Fact]
        public void Can_read_WriteProperty()
        {
            var property = Dto[nameof(SampleModelDto.WriteProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Equal(456, (int)property.Read(SampleModelDto));
            property.Write(SampleModelDto, 567);
            Assert.Equal(567, (int)property.Read(SampleModelDto));
            Assert.Throws<NullReferenceException>(() => property.Write(SampleModelDto, null));
        }

        [Fact]
        public void Can_read_RefProperty()
        {
            var property = Dto[nameof(SampleModelDto.RefProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Equal("Hello world", (string)property.Read(SampleModelDto));
            property.Write(SampleModelDto, "Lorem ipsum");
            Assert.Equal("Lorem ipsum", (string)property.Read(SampleModelDto));
            property.Write(SampleModelDto, null);
            Assert.Null((string)property.Read(SampleModelDto));
        }

        [Fact]
        public void Can_read_StructProperty()
        {
            var property = Dto[nameof(SampleModelDto.StructProperty)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Equal(DateTime.Today, (DateTime)property.Read(SampleModelDto));
            property.Write(SampleModelDto, DateTime.Today.AddDays(1));
            Assert.Equal(DateTime.Today.AddDays(1), (DateTime)property.Read(SampleModelDto));
        }

        [Fact]
        public void Can_read_ComplexType()
        {
            var property = Dto[nameof(SampleModelDto.ComplexType)];
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Null(property.Read(SampleModelDto));
            property.Write(SampleModelDto, new SampleModelDto());
            Assert.NotNull(property.Read(SampleModelDto));
        }

        [Fact]
        public void Can_enumerate_properties()
        {
            int nbProps = 0;
            foreach (var prop in Dto)
            {
                nbProps++;
            }
            Assert.Equal(9, nbProps);
        }

        [Fact]
        public void Can_enumerate_properties_default_enumerator()
        {
            int nbProps = 0;
            foreach (var prop in ((IEnumerable)Dto))
            {
                nbProps++;
            }
            Assert.Equal(9, nbProps);
        }
    }
}
