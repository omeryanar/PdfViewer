namespace PdfViewer
{
    public class DictionaryBookmark
    {
        public const string DictionaryEntry = "DictionaryEntry";

        public int PageNumber { get; set; }

        public string Name { get; set; }

        public string Word { get; set; }

        public string WordClass { get; set; }

        public string Definition { get; set; }
    }
}
