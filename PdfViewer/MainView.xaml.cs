using DevExpress.Xpf.Core;
using DevExpress.Xpf.Ribbon;

namespace PdfViewer
{
    public partial class MainView : DXRibbonWindow
    {
        public MainView()
        {
            InitializeComponent();

            Loaded += (s, e) => { DXSplashScreen.Close(); };
        }
    }
}
