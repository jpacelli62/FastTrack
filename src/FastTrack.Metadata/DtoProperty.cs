using System;

namespace FastTrack.Metadata
{
    public class DtoProperty : MetaModel
	{
		public string Name { get; set; }

		public Type Type { get; set; }

		public bool CanRead { get; set; }

		public bool CanWrite { get; set; }

        public Func<object, object> Read { get; set; }

		public Action<object, object> Write { get; set; }

		public DtoProperty(string name, Type type)
		{
			Name = name;
			Type = type;
		}
	}
}
