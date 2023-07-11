using System.Data;

namespace Faaast.Orm.Reader
{
    public struct CommandParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public ParameterDirection Direction { get; set; }
        public int Size { get; set; }
        public bool IsNullable{ get; set; }
        public DbType DbType { get; set; }
    }
}
