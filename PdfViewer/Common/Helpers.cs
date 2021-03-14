using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace PdfViewer
{
    public class SerializationHelper
    {
        public static T GetSerializedObject<T>(Stream stream)
        {
            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(stream);
            }
            catch (Exception ex)
            {
                Journal.WriteLog(ex, JournalEntryType.Error);
                return default(T);
            }
        }
    }

    public class ConfigurationHelper
    {
        public static string GetAppSetting(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
                return ConfigurationManager.AppSettings[key];

            return String.Empty;
        }
    }
}
