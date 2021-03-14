using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PdfViewer
{
    public static class Extensions
    {
        public static readonly Regex SingleWhiteSpacePattern = new Regex(@"\s+", RegexOptions.Compiled);

        public static readonly Regex AlphaPattern = new Regex(@"[^a-zA-Z-çığöşüÇİĞÖŞÜ]", RegexOptions.Compiled);

        public static bool IsUrl(this string input)
        {
            string trim = input.Trim();

            return trim.Contains(@"/") || trim.Contains(@"\") ||
                trim.StartsWith("www", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsNonAlpha(this string input)
        {
            if (String.IsNullOrWhiteSpace(input))
                return true;

            return String.IsNullOrWhiteSpace(AlphaPattern.Replace(input, String.Empty));
        }

        public static bool Contains(this string input, string value, StringComparison comparisonType)
        {
            return input?.IndexOf(value, comparisonType) >= 0;
        }

        public static string SingleWhiteSpace(this string input)
        {
            if (String.IsNullOrWhiteSpace(input))
                return String.Empty;

            return SingleWhiteSpacePattern.Replace(input, " ").Trim();
        }

        public static string GenerateHash(this string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                Guid guid = new Guid(hash);

                return guid.ToString();
            }
        }

        public static string Join<T>(this IEnumerable<T> input, string seperator)
        {
            if (input == null)
                return null;

            return String.Join(seperator, input);
        }

        public static string[] Split(this string input, string seperator)
        {
            return input.Split(new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static T FromJson<T>(this string value)
        {
            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
                {
                    return (T)serializer.ReadObject(stream);
                }
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static string ToJson(this object graph)
        {
            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(graph.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, graph);
                    byte[] json = stream.ToArray();

                    return Encoding.UTF8.GetString(json);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
