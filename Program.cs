using System;
using System.Windows;

namespace AramDashboard
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;
            app.Run(new MainWindow());
        }
    }
}
