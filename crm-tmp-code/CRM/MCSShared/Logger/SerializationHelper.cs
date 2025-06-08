using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;

namespace MCS.ApplicationInsights
{
    public class SerializationHelper
    {
        public static string Serialize<T>(T instance)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, instance);
                stream.Position = 0;
                var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();

                return json;
            }
        }

        public static T DeserializeJson<T>(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                using (var ms = new MemoryStream(jsonBytes))
                {
                    var settings = new DataContractJsonSerializerSettings
                    {
                        DateTimeFormat = new System.Runtime.Serialization.DateTimeFormat("s"),
                        KnownTypes = Assembly.GetExecutingAssembly().DefinedTypes
                    };
                    var serializer = new DataContractJsonSerializer(typeof(T), settings);
                    return (T)serializer.ReadObject(ms);
                }
            }
            return default(T);

        }

        public static T Deserialize<T>(string json)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                var writer = new StreamWriter(stream);
                writer.Write(json);
                writer.Flush();
                stream.Position = 0;
                T instance = (T)serializer.ReadObject(stream);

                return instance;
            }
        }

        private static string SerializeDictionary(Dictionary<string, string> dictionary)
        {
            if (dictionary == null)
                return null;
            var t2 = "";
            var quote = @"""";
            for (var i = 0; i < dictionary.Count; i++)
            {
                t2 += quote + dictionary.ToArray()[i].Key + quote;
                t2 += ":";
                var val = dictionary.ToArray()[i].Value;
                if (val == null)
                    t2 += quote + val + quote;
                else if (val.FirstOrDefault() == '{' && val.LastOrDefault() == '}')
                    t2 += Serialize(val);
                else
                    t2 += quote + val + quote;
                t2 += ",";
            }

            t2 = "{" + t2.Trim(',') + "}";
            return t2;
        }

        private static string SerializeDictionaryMeasures(Dictionary<string, double> dictionary)
        {
            if (dictionary == null)
                return null;
            var t2 = "";
            var quote = @"""";
            for (var i = 0; i < dictionary.Count; i++)
            {
                t2 += quote + dictionary.ToArray()[i].Key + quote;
                t2 += ":";
                t2 += dictionary.ToArray()[i].Value;
                t2 += ",";
            }

            t2 = "{" + t2.Trim(',') + "}";
            return t2;
        }

        public static string ManageBaseDataMeasures(string input, Dictionary<string, double> dictionary)
        {
            var s = @"""****THIS IS A CUSTOMMEASUREMENTS STRING TO BE REPLACED****""";
            var customDimensions = SerializeDictionaryMeasures(dictionary);
            var retVal = input.Replace(s, customDimensions);
            return retVal;
        }

        public static string ManageBaseData(string input, Dictionary<string, string> dictionary)
        {
            var s = @"""****THIS IS A CUSTOMDIMENSION STRING TO BE REPLACED****""";
            var customDimensions = SerializeDictionary(dictionary);
            if (!string.IsNullOrEmpty(customDimensions))
                customDimensions = customDimensions.Replace(System.Environment.NewLine, "||");
            var retVal = input.Replace(s, customDimensions);
            return retVal;
        }

    }
}