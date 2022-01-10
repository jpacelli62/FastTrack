using System;
using System.Runtime.Serialization;

namespace Faaast.OAuth2Server.Core
{
    [Serializable]
    public class RequestException : Exception
    {
        public string ParameterName { get; set; }

        public string ExpectedMethod { get; set; }

        public RequestException(string parameterName, string expectedMethod)
        {
            this.ParameterName = parameterName;
            this.ExpectedMethod = expectedMethod;
        }

        protected RequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.ParameterName = info.GetString(nameof(this.ParameterName));
            this.ExpectedMethod = info.GetString(nameof(this.ExpectedMethod));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.ParameterName), this.ParameterName);
            info.AddValue(nameof(this.ExpectedMethod), this.ExpectedMethod);
        }
    }
}
