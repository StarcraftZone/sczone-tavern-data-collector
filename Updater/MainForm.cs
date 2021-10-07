using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;

namespace SczoneTavernDataCollector.Updater
{
    public partial class MainForm : Form
    {
        private ILog Log = LogManager.GetLogger("Updater");

        public MainForm()
        {
            Log.Info("更新程序启动");
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var processes = Process.GetProcessesByName(Properties.Settings.Default.MainAppName);
                foreach (var process in processes)
                {
                    LogText("准备 kill: " + process.ProcessName);
                    Task.Run(() =>
                    {
                        Thread.Sleep(1000);
                        process.Kill();
                    });
                    process.WaitForExit();
                    LogText("killed: " + process.ProcessName);
                }

                LogText("正在准备更新文件...");
                Thread.Sleep(TimeSpan.FromSeconds(3));

                var updatesDirectory = new DirectoryInfo(Path.Combine(Application.StartupPath, "updates"));
                CopyDirectory(updatesDirectory.FullName, Application.StartupPath);
                updatesDirectory.Empty();

                UpdateProgress(100);

                LogText("文件更新成功");

                Process.Start(Properties.Settings.Default.MainAppName + ".exe");
                LogText("程序重启成功");

                Thread.Sleep(1000);
                LogText("准备退出更新程序");

                Application.Exit();
            });
        }

        private void UpdateProgress(int progress)
        {
            if (progress > 100)
            {
                progress = 100;
            }
            else if (progress < 0)
            {
                progress = 0;
            }
            updateProgressBar.BeginInvoke(new Action(() =>
            {
                updateProgressBar.Value = progress;
            }));
        }

        private void LogText(string text)
        {
            textLabel.BeginInvoke(new Action(() =>
            {
                textLabel.Text = text;
            }));
            Log.Info(text);
        }

        public void CopyDirectory(string sourceDirectoryPath, string destDirectoryPath)
        {
            try
            {
                if (!Directory.Exists(destDirectoryPath))
                {
                    Directory.CreateDirectory(destDirectoryPath);
                }
                var files = Directory.GetFiles(sourceDirectoryPath);
                var dirs = Directory.GetDirectories(sourceDirectoryPath);
                var progressStep = (int)Math.Floor((double)100 / (files.Length + dirs.Length));
                var progress = 0;

                foreach (string file in files)
                {
                    var destFilePath = Path.Combine(destDirectoryPath, Path.GetFileName(file));
                    File.Copy(file, destFilePath, true);
                    progress += progressStep;
                    UpdateProgress(progress);
                }

                foreach (string dir in dirs)
                {
                    CopyDirectory(dir, Path.Combine(destDirectoryPath, Path.GetFileName(dir)));
                    progress += progressStep;
                    UpdateProgress(progress);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.Exit();
            }
        }
    }
}
