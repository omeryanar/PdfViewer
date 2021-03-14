using System.Runtime.Serialization;

namespace PdfViewer.Yandex.Model
{
    [DataContract]
    public class SpellResult
    {
        [DataMember(Name = "s")]
        public string[] Steer { get; set; }

        public override string ToString()
        {
            return (Steer == null || Steer.Length == 0) ? "" : Steer[0];
        }
    }
}
