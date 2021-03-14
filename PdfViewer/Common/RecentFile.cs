using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace PdfViewer
{
    [DataContract]
    public class RecentFile
    {
        [DataMember]
        public int PageNumber { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string InputLanguage { get; set; }

        [DataMember]
        public string OutputLanguage { get; set; }
    }

    [CollectionDataContract]
    public class RecentFileCollection : List<RecentFile>
    {
        public void SaveToFile()
        {
            string fileName = "Recent.xml";

            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(RecentFileCollection));
                serializer.WriteObject(fileStream, this);
            }
        }

        public static RecentFileCollection LoadFromFile()
        {
            string fileName = "Recent.xml";

            if (!File.Exists(fileName))
                return new RecentFileCollection();

            try
            {
                using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(RecentFileCollection));
                    return serializer.ReadObject(fileStream) as RecentFileCollection;
                }
            }
            catch { return new RecentFileCollection(); }
        }
    }
}
