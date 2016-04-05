using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RestSharp;
using System.Threading.Tasks;
using System.Threading;

namespace UploadVideoWPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private const string OneInstanceMutexName = @"Global\UploadVideoWPF";
        private Mutex mutex;


        public App() {

        }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            
            // only one instance
            bool createdNew;
            mutex = new Mutex(true, OneInstanceMutexName, out createdNew);
            if (!createdNew) {
                Environment.Exit(0);
                return;
            }
        }
    }
}
