using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Core;
using Faaast.Orm;
using Xunit;

namespace Faaast.Tests
{
    public class ExceptionTests
    {
        [Fact]
        public void FaaastOrmException_Serialize()
        {
            var ex = new FaaastOrmException("Lorem ipsum");
            var clone = TestException(ex);
            Assert.Equal(ex.Message, clone.Message);
        }

        [Fact]
        public void RequestException_Serialize()
        {
            var ex = new RequestException("param", "get");
            var clone = TestException(ex);
            Assert.Equal(ex.ParameterName, clone.ParameterName);
            Assert.Equal(ex.ExpectedMethod, clone.ExpectedMethod);
        }

        public TException TestException<TException>(TException ex) where TException : Exception
        {
            using var stream = new MemoryStream();
#pragma warning disable SYSLIB0011 // Le type ou le membre est obsolète
            new BinaryFormatter().Serialize(stream, ex);
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            var myBinaryFormatter = new BinaryFormatter
            {
                Binder = new ExceptionBinder<TException>()
            };

            var exClone = (TException)myBinaryFormatter.Deserialize(stream);
#pragma warning restore SYSLIB0011 // Le type ou le membre est obsolète
            return exClone;
        }

        private class ExceptionBinder<TException> : SerializationBinder where TException : Exception
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                var type = typeof(TException);
                if (typeName != type.FullName)
                {
                    throw new SerializationException("Bad type"); // Compliant
                }

                return type;
            }
        }
    }
}
