using System;
using System.IO;
using System.Windows;

namespace AramDashboard
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            string log = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup-error.log");
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                try { File.AppendAllText(log, DateTime.Now + " " + e.ExceptionObject + Environment.NewLine); } catch { }
            };
            try
            {
                Application app = new Application();
                app.ShutdownMode = ShutdownMode.OnMainWindowClose;
                app.DispatcherUnhandledException += delegate(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
                {
                    try { File.AppendAllText(log, DateTime.Now + " " + e.Exception + Environment.NewLine); } catch { }
                    MessageBox.Show(e.Exception.Message, "ARAM Dashboard");
                    e.Handled = true;
                };
                app.Run(new MainWindow());
            }
            catch (Exception ex)
            {
                try { File.AppendAllText(log, DateTime.Now + " " + ex + Environment.NewLine); } catch { }
                MessageBox.Show(ex.Message, "ARAM Dashboard");
            }
        }
    }
}
