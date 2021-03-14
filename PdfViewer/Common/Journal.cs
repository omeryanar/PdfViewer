using System;
using NLog;

namespace PdfViewer
{
    public class Journal
    {
        public static void WriteLog(string message, JournalEntryType entryType)
        {
            switch (entryType)
            {
                case JournalEntryType.Info:
                    logger.Info(message);
                    break;
                case JournalEntryType.Warning:
                    logger.Warn(message);
                    break;
                case JournalEntryType.Error:
                    logger.Error(message);
                    break;
                case JournalEntryType.Fatal:
                    logger.Fatal(message);
                    break;
            }
        }

        public static void WriteLog(Exception ex, JournalEntryType entryType)
        {
            switch (entryType)
            {
                case JournalEntryType.Info:
                    logger.Info(ex);
                    break;
                case JournalEntryType.Warning:
                    logger.Warn(ex);
                    break;
                case JournalEntryType.Error:
                    logger.Error(ex);
                    break;
                case JournalEntryType.Fatal:
                    logger.Fatal(ex);
                    break;
            }
        }

        public static void Shutdown()
        {
            LogManager.Shutdown();
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    }

    public enum JournalEntryType
    {
        Info,
        Warning,
        Error,
        Fatal
    }
}
