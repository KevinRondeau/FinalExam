using System.Threading;
using System.Windows;

namespace WeatherApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
  

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ApplicationView app = new ApplicationView();


            app.Show();

        }
        public App()
        {
            {
                var lang = WeatherApp.Properties.Settings.Default.language;
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);
            }
        }
    }
}
