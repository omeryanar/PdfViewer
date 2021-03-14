using System;
using System.Linq;
using System.Runtime.Serialization;

namespace PdfViewer.Yandex.Model
{
    [DataContract]
    public class DictionaryResult
    {
        [DataMember(Name = "def")]
        public Definition[] Definitions { get; set; }

        public override string ToString()
        {
            return Definitions.Join(Environment.NewLine);
        }
    }

    [DataContract]
    public class Definition
    {
        [DataMember(Name = "text")]
        public string Word { get; set; }

        [DataMember(Name = "pos")]
        public string WordClass { get; set; }

        [DataMember(Name = "fl")]
        public string Inflections { get; set; }

        [DataMember(Name = "tr")]
        public Translation[] Translations { get; set; }

        public string GetTranslations()
        {
            if (Translations == null)
                return String.Empty;

            return Translations.Select((x, i) => String.Format("{0}. {1}", i + 1, x)).Join(Environment.NewLine);
        }

        public override string ToString()
        {
            return String.Format("{0} ({1}){2}{3}", Word, WordClass, Environment.NewLine, GetTranslations());
        }
    }

    [DataContract]
    public class Translation
    {
        [DataMember(Name = "text")]
        public string Word { get; set; }

        [DataMember(Name = "syn")]
        public Meaning[] Synonyms { get; set; }

        public override string ToString()
        {
            string description = Word;

            if (Synonyms != null)
                description = String.Format("{0}, {1}", description, Synonyms.Join(", "));

            return description;
        }
    }

    [DataContract]
    public class Meaning
    {
        [DataMember(Name = "text")]
        public string Word { get; set; }

        public override string ToString()
        {
            return Word;
        }
    }
}
