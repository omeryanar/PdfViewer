using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using PdfViewer.Yandex;
using PdfViewer.Yandex.Model;

namespace PdfViewer.Model
{
    public class DictionaryManager
    {
        public ILiteCollection<DictionaryEntry> Dictionary { get; private set; }

        public DictionaryManager(string databaseName)
        {
            DirectoryInfo directory = Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Dictionary"));
            LiteDatabase database = new LiteDatabase(Path.Combine(directory.FullName, databaseName));

            Dictionary = database.GetCollection<DictionaryEntry>("Dictionary");
            Dictionary.EnsureIndex(x => x.Word);
            Dictionary.EnsureIndex(x => x.Variations);
        }

        public async Task<string> Pronounce(string word, CultureInfo inputCulture)
        {
            return await YandexService.Pronounce(word, inputCulture.Name).ConfigureAwait(false);
        }

        public async Task<SearchResult[]> Search(string word, CultureInfo inputCulture, CultureInfo outputCulture)
        {
            if (word.IsNonAlpha())
                return null;

            word = word.SingleWhiteSpace();

            if (word.Length < 2 || word.Length > 25)
                return null;

            LanguagePair languagePair = CreateLangPair(inputCulture, outputCulture);
            DictionaryResult dictionaryResult = await Search(word, languagePair, inputCulture).ConfigureAwait(false);

            return dictionaryResult?.Definitions?.Select(x => new SearchResult
            {
                Word = x.Word,
                WordClass = x.WordClass,
                Inflections = x.Inflections,
                Definition = x.GetTranslations()
            }).ToArray();
        }

        private LanguagePair CreateLangPair(CultureInfo inputCulture, CultureInfo outputCulture)
        {
            Language inputLang = inputCulture.Name.ToEnum<Language>();
            Language outputLang = outputCulture.Name.ToEnum<Language>();

            return new LanguagePair(inputLang, outputLang);
        }

        private async Task<DictionaryResult> Search(string word, LanguagePair languagePair, CultureInfo culture)
        {
            word = word.ToLower(culture);

            DictionaryEntry entry = SearchOffline(word, languagePair);
            if (entry != null)
                return entry.Result;

            DictionaryResult result = await SearchOnline(word, languagePair, culture).ConfigureAwait(false);
            if (result?.Definitions?.Length > 0)
                return result;

            string correction = await YandexService.Correct(word, languagePair.InputLanguage).ConfigureAwait(false);
            if (!String.IsNullOrWhiteSpace(correction) && String.Compare(word, correction, true, culture) != 0)
                return await Search(correction, languagePair, culture).ConfigureAwait(false);

            return null;
        }

        private DictionaryEntry SearchOffline(string word, LanguagePair languagePair)
        {
            string language = languagePair.ToString();
            DictionaryEntry entry = Dictionary.FindOne(Query.And(Query.EQ("Language", language), Query.EQ("Word", word)));
            if (entry != null)
                return entry;

            return Dictionary.FindOne(Query.And(Query.EQ("Language", language), Query.Contains("Variations", String.Format("|{0}|", word))));
        }

        private async Task<DictionaryResult> SearchOnline(string word, LanguagePair languagePair, CultureInfo culture)
        {
            DictionaryResult result = await YandexService.Lookup(word, languagePair).ConfigureAwait(false);
            if (result?.Definitions?.Length > 0)
            {
                string returnedWord = result.Definitions[0].Word.ToLower(culture);
                DictionaryEntry entry = SearchOffline(returnedWord, languagePair);
                if (entry != null)
                {
                    entry.Variations += word + "|";
                    Dictionary.Update(entry);
                }
                else
                {
                    entry = new DictionaryEntry();
                    entry.Word = returnedWord;
                    entry.Result = result;
                    entry.Language = languagePair.ToString();

                    if (String.Compare(word, returnedWord, true, culture) != 0)
                        entry.Variations += word + "|";

                    Dictionary.Insert(entry);
                }
            }

            return result;
        }
    }
}
