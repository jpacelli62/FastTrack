using System;

namespace Faaast.Tests.Orm.Fixtures
{
    public class SampleModelDto
    {
        public int IntMember = 234;
        public int ReadWriteProperty { get; set; } = 123;
        public bool? NullableBoolProperty { get; set; } = true;
        public int ReadOnlyProperty { get; } = 234;
        public int PrivateSetProperty { get; private set; } = 345;

#pragma warning disable IDE0051 // Supprimer les membres privés non utilisés
        private int PrivateProperty { get; set; } = 5445;
#pragma warning restore IDE0051 // Supprimer les membres privés non utilisés


        public int WriteProperty { private get; set; } = 456;
        public string RefProperty { get; set; } = "Hello world";
        public DateTime StructProperty { get; set; } = DateTime.Today;
        public SampleModelDto ComplexType { get; set; }
    }
}
