using System;
using DevExpress.Xpf.Core;

namespace PdfViewer
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ApplicationThemeHelper.ApplicationThemeName = "Office2019White";
            DXSplashScreen.Show<SplashView>();

            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
