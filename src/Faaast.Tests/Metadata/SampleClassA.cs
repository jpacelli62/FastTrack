using System;

namespace Faaast.Tests.Metadata
{
    public class SampleClassA
    {
        public int MyProperty { get; set; }

        public Object MyComplexMember;

        public SampleClassA(int myProperty) => this.MyProperty = myProperty;
    }
}
