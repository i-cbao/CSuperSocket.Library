using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;

namespace Dynamic.Core.Runtime
{
    /// <summary>
    /// 序列化辅助类
    /// </summary>
    public static class SerializationUtility
    {
        /// <summary>
        /// 序列化为XML字符串
        /// </summary>
        /// <param name="value">需要序列化的对象</param>
        /// <returns>序列化后的XML字符串</returns>
        public static string ToXmlString(object value)
        {
            if (value == null)
            {
                return String.Empty;
            }

            string xmlString = String.Empty;
            Type type = value.GetType();
           // string ns = getTypeNamespace(type);
            XmlSerializer serializer = new XmlSerializer(type);
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, value);

                ms.Flush();
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                {
                    xmlString = sr.ReadToEnd();
                }
            }

            return xmlString;
        }


        /// <summary>
        /// 序列化为XML字符串
        /// </summary>
        /// <param name="value">需要序列化的对象</param>
        /// <param name="stream">写入的流</param>
        public static void ToXmlString(object value, Stream stream)
        {
            if (value == null)
            {
                return;
            }

            Type type = value.GetType();
          //  string  ns = getTypeNamespace(type);
            XmlSerializer serializer = new XmlSerializer(type);
            try
            {
                serializer.Serialize(stream, value);
            }
            catch
            {
            }
            stream.Flush();
        }

        /// <summary>
        /// 序列化为二进制
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(object value)
        {
            if (value == null)
            {
                return null;
            }

            byte[] inMemoryBytes;
            using (MemoryStream inMemoryData = new MemoryStream())
            {
                new BinaryFormatter().Serialize(inMemoryData, value);
                inMemoryBytes = inMemoryData.ToArray();
            }

            return inMemoryBytes;
        }

        public static void ToBytes(object value, Stream stream)
        {
            if (value == null)
            {
                return;
            }

            new BinaryFormatter().Serialize(stream, value);
            stream.Flush();
        }

        /// <summary>
        /// 将XML字符串反序列化为对象
        /// </summary>
        /// <param name="xmlString">XML序列化字符串</param>
        /// <param name="objectType">对象类型</param>
        /// <returns>反序列化后的对象</returns>
        public static TValue ToObject<TValue>(string xmlString)
        {
            return ToObject<TValue>(xmlString, typeof(TValue));
        }

        public static T ToObject<T>(string xmlString, Type objectType)
        {
            XmlSerializer serializer = new XmlSerializer(objectType);
            T objectValue = default(T);
            using (MemoryStream ms = new MemoryStream())
            {
                StreamWriter sw = new StreamWriter(ms);
                sw.Write(xmlString);
                sw.Flush();

                ms.Position = 0;

                objectValue = serializer.Deserialize<T>(ms);
            }
            return objectValue;
        }

        /// <summary>
        /// 将XML字符串反序列化为对象
        /// </summary>
        /// <param name="xmlStream">XML序列化字符串所在的流</param>
        /// <param name="objectType">对象的类型</param>
        /// <returns>反序列化后的对象</returns>
        public static TValue ToObject<TValue>(Stream xmlStream)
        {
            return ToObject<TValue>(xmlStream, typeof(TValue));
        }

        public static T ToObject<T>(Stream xmlStream, Type objectType)
        {
            XmlSerializer serializer = new XmlSerializer(objectType);
            return serializer.Deserialize<T>(xmlStream);
        }

        public static T BytesToObject<T>(byte[] serializedObject)
        {
            if (serializedObject == null)
            {
                return default(T);
            }
            using (MemoryStream dataInMemory = new MemoryStream(serializedObject))
            {
                return new BinaryFormatter().Deserialize<T>(dataInMemory);
            }
        }
        public static T JsonToObject<T>(string objectStr)
        {
            if (!string.IsNullOrEmpty(objectStr))
            {
              return  Newtonsoft.Json.JsonConvert.DeserializeObject<T>(objectStr);
            }
            return default(T);
        }
        public static string ObjectToJson(object jsonObj)
        {
            if (jsonObj!=null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj);
            }
            return null;
        }
        public static T BytesToObject<T>(Stream stream)
        {
            if (stream == null)
                return default(T);

            return new BinaryFormatter().Deserialize<T>(stream);
        }
        private static string getTypeNamespace(Type type)
        {
            string typeName = type.FullName.Replace(".", "/");
            string prefix = Path.GetFileNameWithoutExtension(type.Assembly.Location);
            return String.Format("http://Dynamic/{0}/{1}", prefix, typeName);
        }
    }
}
