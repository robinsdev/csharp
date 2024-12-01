using System.Configuration;
using System.Data;
using System.Windows;

namespace MDGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    }

}
