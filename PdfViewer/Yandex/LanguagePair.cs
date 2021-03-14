using System;

namespace PdfViewer.Yandex
{
    public struct LanguagePair
    {
        public string InputLanguage { get; set; }
        public string OutputLanguage { get; set; }

        public LanguagePair(Language inputLanguage, Language outputLanguage)
        {
            InputLanguage = inputLanguage.ToString().ToLower();
            OutputLanguage = outputLanguage.ToString().ToLower();
        }

        public override string ToString()
        {
            return String.Format("{0}-{1}", InputLanguage, OutputLanguage);
        }
    }
}
