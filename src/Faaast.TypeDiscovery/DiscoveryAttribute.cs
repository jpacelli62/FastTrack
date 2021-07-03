using System;

namespace Faaast.TypeDiscovery
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DiscoveryAttribute : Attribute
    {
        public string Usage { get; private set; }

        public DiscoveryAttribute(string usage = "Default")
        {
            this.Usage = usage;
        }
    }
}
