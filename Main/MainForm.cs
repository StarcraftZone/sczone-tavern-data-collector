using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace SczoneTavernDataCollector.Main
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void SczoneLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(((LinkLabel)sender).Text) { UseShellExecute = true });
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Log($"程序启动 {Global.CurrentVersion}");
            VersionLabel.Text = $"版本号: V{Global.CurrentVersion}";
            FindTavernBankFiles();
            CheckNewVersion();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log("程序退出");
        }

        private void Log(string text)
        {
            LogTextBox.BeginInvoke(new Action(() =>
            {
                LogTextBox.Text += $"[{DateTime.Now.ToString("F")}] {text}\r\n";
            }));
            Global.Log.Info(text);
        }

        private void FindTavernBankFiles()
        {
            var starcraft2Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarCraft II");
            Log("星际2档案目录: " + starcraft2Path);
            var accountsDirectory = new DirectoryInfo(Path.Combine(starcraft2Path, "Accounts"));
            if (accountsDirectory.Exists)
            {
                foreach (var accountDirectory in accountsDirectory.GetDirectories())
                {
                    var accountNo = accountDirectory.Name;
                    foreach (var directory in accountDirectory.GetDirectories())
                    {
                        var profileFolderRegex = new Regex(@"(\d*)-S2-(\d*)-(\d*)");
                        if (profileFolderRegex.IsMatch(directory.Name))
                        {
                            var matches = profileFolderRegex.Match(directory.Name);
                            Log(directory.Name);
                            Log($"region: {matches.Groups[1].Value}, realm: {matches.Groups[2].Value}, profileNo: {matches.Groups[3].Value}");
                        }
                    }
                }
            }
            else
            {
                Log("Accounts 目录不存在：" + accountsDirectory.FullName);
            }
        }

        private void CheckNewVersion()
        {
            try
            {
                Task.Run(() =>
                {
                    var response = HttpHelper.Get("https://sc-api.yuanfen.net/app/code/tavern-data-collector");
                    if (response.code == 0 && response.data != null)
                    {
                        var latestVersion = new Version((string)response.data.latestVersion);
                        var latestPackageUrl = (string)response.data.latestPackageUrl;
                        if (latestVersion > Global.CurrentVersion)
                        {
                            Log($"最新版本: {latestVersion}, 需要更新, {latestPackageUrl}");
                            Log($"准备更新，当前：{Global.CurrentVersion}，最新：{latestVersion}");
                            if (!Global.UpdateDownloading)
                            {
                                Global.UpdateDownloading = true;
                                // 清空更新文件目录
                                var updatesDirectory = new DirectoryInfo("updates/");
                                if (updatesDirectory.Exists)
                                {
                                    updatesDirectory.Empty();
                                }

                                // 下载并解压更新包
                                if (latestPackageUrl != null)
                                {
                                    var webClient = new WebClient();
                                    var updateStream = webClient.OpenRead(latestPackageUrl);
                                    UnzipFromStream(updateStream, "updates/");
                                    KillProcess(Properties.Settings.Default.MainAppName);
                                    StartApp(Properties.Settings.Default.UpdaterAppName);
                                }
                                else
                                {
                                    Log("更新地址为空");
                                    Global.UpdateDownloading = false;
                                }
                            }
                            else
                            {
                                Log("更新下载中，跳过本次更新");
                            }
                        }
                        else
                        {
                            Log($"最新版本: {latestVersion}, 无需更新");
                        }

                    }
                    else
                    {
                        Log($"更新检查失败，code: {(int)response.code}，请手动前往 https://sc.yuanfen.net/tavern 检查更新");
                    }
                });
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                Global.Log.Error(ex.Message, ex);
            }
        }

        private void UnzipFromStream(Stream zipStream, string outFolder)
        {
            var zipInputStream = new ZipInputStream(zipStream);
            var zipEntry = zipInputStream.GetNextEntry();
            while (zipEntry != null)
            {
                var entryFileName = zipEntry.Name;
                var buffer = new byte[4096];
                var fullZipToPath = Path.Combine(outFolder, entryFileName);
                var directoryName = Path.GetDirectoryName(fullZipToPath);
                if (directoryName.Length > 0)
                {
                    Directory.CreateDirectory(directoryName);
                }
                var fileName = Path.GetFileName(fullZipToPath);
                if (fileName.Length == 0)
                {
                    zipEntry = zipInputStream.GetNextEntry();
                    continue;
                }
                using (var streamWriter = File.Create(fullZipToPath))
                {
                    StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                }
                zipEntry = zipInputStream.GetNextEntry();
            }
        }

        private void KillProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                Log("准备退出程序 " + processName);
                foreach (var process in processes)
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(1000);
                        process.Kill();
                        process.WaitForExit();
                    });
                }
            }
        }

        private void StartApp(string appName)
        {
            var processes = Process.GetProcessesByName(appName);
            if (processes.Length == 0)
            {
                Log("准备启动更新器 " + appName);
                Process.Start(Path.Combine(Application.StartupPath, appName + ".exe"));
            }
        }
    }
}
