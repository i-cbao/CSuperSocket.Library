using System;
using System.IO;

namespace Dynamic.Core.Runtime
{
    internal class XmlSerializer : ISerialize
    {
        public XmlSerializer(Type type)
        {

        }
        public T Deserialize<T>(Stream st)
        {
            throw new NotImplementedException();
        }

        public void Serialize(Stream ms, object value)
        {
            throw new NotImplementedException();
        }
    }
}