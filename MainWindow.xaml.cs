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
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using RestSharp;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;

namespace UploadVideoWPF {

    public partial class MainWindow : Window {

        [DllImport("user32.dll")]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        private string startupPath;
        private bool trig = false;
        private string app_id = "f48b1b34a20d96e5b76928067a160f40ce87b89da6171777f9c652e0a16c1246";
        private string app_secret = "16c85130613ae674520253f97a0de7862d58f93fab175a1e4a6f49cd1d4ab2ef";
        private string access_token;
        private FileSystemWatcher fileWatcher;

        public MainWindow() {
            InitializeComponent();

            // Change HOSTS file
            // 127.0.0.1 coub-callback-url.com
            /*string tmp = File.ReadAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"));
            if (tmp.Contains("coub-callback-url.com") == false) {
                using (StreamWriter writer = File.AppendText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"))) {
                    writer.WriteLine("127.0.0.1 coub-callback-url.com");
                }
            }*/

            Left = -15000;
            ShowInTaskbar = false;
            ShowActivated = false;

            startupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);


            // TEST

            try {
                var authServer = new DotNetOpenAuth.OAuth2.AuthorizationServerDescription {
                    AuthorizationEndpoint = new Uri("https://coub.com/oauth/authorize"),
                };
                authServer.TokenEndpoint = new Uri("https://coub.com/oauth/token");

                var client = new DotNetOpenAuth.OAuth2.UserAgentClient(authServer, app_id, app_secret);

                var authorizePopup = new Authorize2(client);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e) {
            //var nancyHost = new NancyHost(new Uri("http://localhost:80"));
            //nancyHost.Start();

            // Authorization
            var authServer = new DotNetOpenAuth.OAuth2.AuthorizationServerDescription {
                AuthorizationEndpoint = new Uri("https://coub.com/oauth/authorize"),
            };
            authServer.TokenEndpoint = new Uri("https://coub.com/oauth/token");

            try {
                var client = new DotNetOpenAuth.OAuth2.UserAgentClient(authServer, app_id, app_secret);

                var authorizePopup = new Authorize2(client);
                authorizePopup.Authorization.Scope.AddRange(OAuthUtilities.SplitScopes("create"));
                //authorizePopup.Authorization.Callback = new Uri("http://coub-callback-url.com");
                authorizePopup.Authorization.Callback = new Uri("http://dtj4e44nrej4j3jej4ujwsdu4.com");
                authorizePopup.Owner = this;

                bool? result = authorizePopup.ShowDialog();

                if (result.HasValue && result.Value) {
                    if (trig == false)
                        trig = true;
                    else
                        return;

                    //nancyHost.Stop();

                    access_token = authorizePopup.Authorization.AccessToken;

                    StartWatch();


                } else {
                    Environment.Exit(0);
                    return;
                }
            } catch (DotNetOpenAuth.Messaging.ProtocolException ex) {
                //Environment.Exit(0);
            } catch (WebException ex) {
                string responseText = string.Empty;
                if (ex.Response != null) {
                    using (var responseReader = new StreamReader(ex.Response.GetResponseStream())) {
                        responseText = responseReader.ReadToEnd();
                    }
                }
                //Environment.Exit(0);
            }
        }

        private void StartWatch() {
            fileWatcher = new FileSystemWatcher();

            //Set the filter to only catch TXT files.
            fileWatcher.Filter = "*.mp4";

            //Subscribe to the Created event.
            fileWatcher.Created += watcher_FileEvent;
            fileWatcher.Changed += watcher_FileEvent;

            //Set the path to C:\Temp\
            fileWatcher.Path = startupPath + "\\upload";

            //Enable the FileSystemWatcher events.
            fileWatcher.EnableRaisingEvents = true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e) {
            throw new NotImplementedException();
        }

        protected virtual bool IsFileLocked(FileInfo file) {
            FileStream stream = null;

            try {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            } catch (IOException) {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            } finally {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        void watcher_FileEvent(object sender, FileSystemEventArgs e) {

            Thread.Sleep(5000);

            fileWatcher.Created -= watcher_FileEvent;
            fileWatcher.Changed -= watcher_FileEvent;

            //// Upload
            var restClient = new RestClient("http://coub.com");

            var request2 = new RestRequest("/api/v2/coubs/init_upload", Method.POST);
            request2.AddQueryParameter("access_token", access_token);


            restClient.ExecuteAsync(request2, response => {
                Console.WriteLine(response.Content);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content);


                while (true) {
                    if (IsFileLocked(new FileInfo(@"upload\result.mp4")) == false) {
                        using (MyWebClient client2 = new MyWebClient()) {
                            try {
                                client2.Headers.Add("Content-Type", "video/flv");
                                client2.QueryString.Add("access_token", access_token);
                                client2.UploadData("http://coub.com/api/v2/coubs/" + jsonObj.id + "/upload_video", File.ReadAllBytes(@"upload\result.mp4"));
                            } catch (Exception ex) {
                                MessageBox.Show(ex.Message, "Ошибка выгрузки");
                            }
                        }
                        break;
                    } else {
                        MessageBox.Show("file locked");
                    }
                    Thread.Sleep(500);
                }


                var request3 = new RestRequest("/api/v2/coubs/" + jsonObj.id + "/finalize_upload", Method.POST);
                request3.AddQueryParameter("access_token", access_token);
                request3.AddQueryParameter("sound_enabled", "true");
                request3.AddQueryParameter("title", "Test");
                request3.AddQueryParameter("tags", "coubscreencaster");
                request3.AddQueryParameter("original_visibility_type", "public");

                restClient.Execute(request3);

                Console.Beep(1000, 500);
                fileWatcher.Created += watcher_FileEvent;
                fileWatcher.Changed += watcher_FileEvent;
            });
        }

        public void PostMultipleFiles(string url, string[] files) {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;
            Stream memStream = new System.IO.MemoryStream();
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            string formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition:  form-data; name=\"{0}\";\r\n\r\n{1}";
            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n Content-Type: application/octet-stream\r\n\r\n";
            memStream.Write(boundarybytes, 0, boundarybytes.Length);
            for (int i = 0; i < files.Length; i++) {
                string header = string.Format(headerTemplate, "file" + i, files[i]);
                //string header = string.Format(headerTemplate, "uplTheFile", files[i]);
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                memStream.Write(headerbytes, 0, headerbytes.Length);
                FileStream fileStream = new FileStream(files[i], FileMode.Open,
                FileAccess.Read);
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0) {
                    memStream.Write(buffer, 0, bytesRead);
                }
                memStream.Write(boundarybytes, 0, boundarybytes.Length);
                fileStream.Close();
            }
            httpWebRequest.ContentLength = memStream.Length;
            Stream requestStream = httpWebRequest.GetRequestStream();
            memStream.Position = 0;
            byte[] tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Close();
            requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            requestStream.Close();
            try {
                WebResponse webResponse = httpWebRequest.GetResponse();
                Stream stream = webResponse.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                string var = reader.ReadToEnd();

            } catch (Exception ex) {
                //response.InnerHtml = ex.Message;
            }
            httpWebRequest = null;
        }


    }

    public class MyWebClient : WebClient {



        protected override WebRequest GetWebRequest(Uri uri) {
            WebHeaderCollection arr = base.Headers;
            //arr["Content-Type"] = "video/flv";



            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 1000 * 60 * 15;
            return w;
        }
    }


}
