using DotNetOpenAuth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UploadVideoWPF {
    /// <summary>
    /// Interaction logic for Authorize2.xaml
    /// </summary>
    public partial class Authorize2 : Window {
        internal Authorize2(UserAgentClient client) {
            this.InitializeComponent();
            this.clientAuthorizationView.Client = client;
        }

        public IAuthorizationState Authorization {
            get { return this.clientAuthorizationView.Authorization; }
        }

        private void clientAuthorizationView_Completed(object sender, ClientAuthorizationCompleteEventArgs e) {
            try {
                this.DialogResult = e.Authorization != null;
                this.Close();
            } catch { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            clientAuthorizationView.Completed += clientAuthorizationView_Completed;
            Task.Factory.StartNew(delegate {
                Dispatcher.Invoke((Action)delegate () {
                    //Hide();
                    Left = 10;
                    Top = 10;
                    Visibility = Visibility.Visible;
                });

            });
        }


    }
}
