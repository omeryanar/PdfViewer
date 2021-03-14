using System;
using System.Windows;
using System.Windows.Threading;
using DevExpress.Xpf.Core;

namespace PdfViewer
{
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;            

            Current.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            Current.DispatcherUnhandledException += Dispatcher_UnhandledException;
        }

        #region CurrentDomain

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            string loadedAssemblyName = args.LoadedAssembly.ManifestModule.Name;
            if (loadedAssemblyName.EndsWith("dll"))
                DXSplashScreen.SetState(loadedAssemblyName);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                string message = String.Format("A fatal error has occurred in the application.\nSorry for the inconvenience.\n\n{0}:\n{1}",
                        e.ExceptionObject.GetType(), e.ExceptionObject.ToString());

                if (e.ExceptionObject is Exception)
                    Journal.WriteLog(e.ExceptionObject as Exception, JournalEntryType.Fatal);
                else
                    Journal.WriteLog("Fatal Error: " + message, JournalEntryType.Fatal);

                DXMessageBox.Show(message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Environment.Exit(-1);
            }
        }

        #endregion

        #region Dispatcher

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            Journal.Shutdown();
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Journal.WriteLog(e.Exception, JournalEntryType.Error);

            DXMessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}
