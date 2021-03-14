using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PdfViewer.Yandex.Model;

namespace PdfViewer.Yandex
{
    public class YandexService
    {
        private static HttpClient DictionaryClient, SpellerClient, SpeechClient;

        static YandexService()
        {
            DictionaryClient = new HttpClient();
            DictionaryClient.Timeout = new TimeSpan(0, 0, 10);
            DictionaryClient.BaseAddress = new Uri("https://dictionary.yandex.net/api/v1/dicservice.json/");

            SpellerClient = new HttpClient();
            SpellerClient.Timeout = new TimeSpan(0, 0, 10);
            SpellerClient.BaseAddress = new Uri("http://speller.yandex.net/services/spellservice.json/");

            SpeechClient = new HttpClient();
            SpeechClient.Timeout = new TimeSpan(0, 0, 20);
            SpeechClient.BaseAddress = new Uri("https://tts.voicetech.yandex.net/");
        }

        public static async Task<string> Pronounce(string word, string language)
        {
            try
            {
                string apiKey = ConfigurationHelper.GetAppSetting("YandexSpeechApiKey");
                string requestUrl = String.Format("generate?key={0}&format=mp3&speaker=jane&lang={1}&text={2}",
                    apiKey, language, WebUtility.UrlEncode(word));

                string fileName = Path.Combine(Path.GetTempPath(), requestUrl.GenerateHash()) + ".mp3";
                if (File.Exists(fileName))
                    return fileName;

                using (FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (Stream stream = await SpeechClient.GetStreamAsync(requestUrl).ConfigureAwait(false))
                    {
                        stream.CopyTo(fileStream);
                        return fileStream.Name;
                    }    
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<string> Correct(string word, string language)
        {
            try
            {
                string requestUrl = String.Format("checkText?lang={0}&text={1}",
                    language, WebUtility.UrlEncode(word));

                using (Stream stream = await SpellerClient.GetStreamAsync(requestUrl).ConfigureAwait(false))
                {
                    SpellResult[] result = SerializationHelper.GetSerializedObject<SpellResult[]>(stream);
                    if (result != null && result.Length > 0)
                        return result[0].ToString();

                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<DictionaryResult> Lookup(string word, LanguagePair languagePair)
        {
            try
            {
                string apiKey = ConfigurationHelper.GetAppSetting("YandexDictionaryApiKey");
                string requestUrl = String.Format("lookup?key={0}&lang={1}&ui={2}&text={3}&flags=3",
                    apiKey, languagePair, languagePair.InputLanguage, WebUtility.UrlEncode(word));

                using (Stream stream = await DictionaryClient.GetStreamAsync(requestUrl).ConfigureAwait(false))
                {
                    return SerializationHelper.GetSerializedObject<DictionaryResult>(stream);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
