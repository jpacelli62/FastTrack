using System;
using System.Runtime.Serialization;

namespace Faaast.Orm
{
    [Serializable]
    public class FaaastOrmException : Exception
    {
        public FaaastOrmException(string message): base(message)
        {
        }

        public FaaastOrmException(string message, Exception inner) : base(message, inner)
        {
        }

        protected FaaastOrmException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
