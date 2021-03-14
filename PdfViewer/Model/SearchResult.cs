using System.Runtime.Serialization;

namespace PdfViewer.Model
{
    [DataContract]
    public class SearchResult
    {
        [DataMember]
        public string Word { get; set; }

        [DataMember]
        public string WordClass { get; set; }

        [DataMember]
        public string Definition { get; set; }

        [DataMember]
        public string Inflections { get; set; }

        public override string ToString()
        {
            return Definition;
        }
    }
}
