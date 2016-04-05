using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Linq;
using System.Windows.Forms;
using Shell32;

namespace JoinVideo {
    class Program {

        private static string startupPath;

        static void Main(string[] args) {
            try {
                List<FileInfo> filesInfo = new List<FileInfo>();

                startupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                //TimeSpan duration;
                TimeSpan selection;
                //TimeSpan len;
                string dur;

                //GetDuration(startupPath + "\\temp\\tmp.mp4", out duration);
                GetDuration2(startupPath + "\\temp\\tmp.mp4", out dur);

                //selection = duration.Subtract(new TimeSpan(0, 0, 0, 10, 0));
                //len = duration.Subtract(selection);

                string sec = dur.Split('.')[0];
                string milisec = dur.Split('.')[1];

                //string command = " -i \"" + startupPath + "\\temp\\tmp.mp4\" -ss " + selection.ToString() + " -t 00:00:15 -c copy -y \"" + startupPath + "\\upload\\result.mp4\"";

                selection = new TimeSpan(0, 0, 0, int.Parse(sec) - 10);

                string command = " -i \"" + startupPath + "\\temp\\tmp.mp4\" -ss " + selection.ToString() + "." + milisec + " -t 00:00:15 -c copy -y \"" + startupPath + "\\upload\\result.mp4\"";

                Debug.WriteLine(command);

                ProcessStartInfo proc = new ProcessStartInfo("\"" + startupPath + "\\ffmpeg\\bin\\ffmpeg.exe\"", command);

                proc.UseShellExecute = false;
                proc.CreateNoWindow = false;

                Process process = Process.Start(proc);
                process.PriorityClass = ProcessPriorityClass.Idle;
                process.EnableRaisingEvents = true;
                process.Exited += Process_Exited;

            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }

            while (true) {
                Thread.Sleep(1000);
            }
        }

        public static bool GetDuration2(string filename, out string ms) {
            try {
                string command = " -i \"" + filename + "\" -show_entries format=duration -v quiet -of csv=\"p=0\"";
                Debug.WriteLine(command);
                ProcessStartInfo proc = new ProcessStartInfo("\"" + startupPath + "\\ffmpeg\\bin\\ffprobe.exe\"", command);
                proc.UseShellExecute = false;
                proc.CreateNoWindow = false;
                proc.RedirectStandardOutput = true;

                Process process = Process.Start(proc);
                string result = process.StandardOutput.ReadLine();
                process.PriorityClass = ProcessPriorityClass.Idle;
                process.EnableRaisingEvents = true;
                process.WaitForExit();

                //string result = process.StandardOutput.ReadLine();

                ms = result;
                return true;
            } catch (Exception ex) {
                ms = null;
                return false;
            }
        }

        public static bool GetDuration(string filename, out TimeSpan duration) {
            try {
                var shl = new Shell();
                var fldr = shl.NameSpace(Path.GetDirectoryName(filename));
                var itm = fldr.ParseName(Path.GetFileName(filename));

                // Index 27 is the video duration [This may not always be the case]
                var propValue = fldr.GetDetailsOf(itm, 27);

                return TimeSpan.TryParse(propValue, out duration);
            } catch (Exception) {
                duration = new TimeSpan();
                return false;
            }
        }

        private static void FirstStep_Complete(object sender, EventArgs e) {
            try {
                List<FileInfo> filesInfo = new List<FileInfo>();

                startupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                TimeSpan duration;
                TimeSpan selection;
                TimeSpan len;

                GetDuration(startupPath + "\\upload\\result.mp4", out duration);

                selection = duration.Subtract(new TimeSpan(0, 0, 0, 5, 0));
                len = duration.Subtract(selection);

                string command = " -i \"" + startupPath + "\\upload\\result.mp4\" -ss " + selection.ToString() + " -t 00:00:15 -c copy -y \"" + startupPath + "\\upload\\result2.mp4\"";

                Debug.WriteLine(command);
                //MessageBox.Show(command);

                ProcessStartInfo proc = new ProcessStartInfo("\"" + startupPath + "\\ffmpeg\\bin\\ffmpeg.exe\"", command);

                proc.UseShellExecute = false;
                proc.CreateNoWindow = false;

                Process process = Process.Start(proc);
                process.PriorityClass = ProcessPriorityClass.Idle;
                process.EnableRaisingEvents = true;
                process.Exited += Process_Exited;

            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        private static void Process_Exited(object sender, EventArgs e) {
            UploadVideo();
        }

        private static void UploadVideo() {
            //Process.Start(startupPath + "\\UploadVideoWPF.exe");
            Environment.Exit(0);
        }
    }
}
