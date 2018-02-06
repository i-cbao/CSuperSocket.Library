using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.IO;
using System;

namespace Dynamic.Core.Runtime
{

    public interface ISerialize
    {
        void Serialize(Stream ms, object value);
        object Deserialize(Stream st);
    }


    public class BinaryFormatter:ISerialize
    {
        public BinaryFormatter()
        {

        }

        public object Deserialize(Stream st)
        {
            throw new NotImplementedException();
        }

        public void Serialize(Stream ms, object value)
        {

            ms.Position = 0;
            using (BsonWriter writer = new BsonWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, value);
            }

        }
        public byte[] Serialize(object value)
        {
            byte[] buffer = null;
            using (var ms = new MemoryStream())
            {
                //Obsolete
                using (BsonWriter writer = new BsonWriter(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, value);
                    buffer=ms.ToArray();
                }
            }
            return buffer;

        }
    }
}