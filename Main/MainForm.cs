using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace SczoneTavernDataCollector.Main
{
    public partial class MainForm : Form
    {
        private const string TavernModeStandardRanked = "StandardRanked";
        private const string TavernModeStandardUnranked = "StandardUnranked";
        private const string TavernModeAutoRanked = "AutoRanked";
        private const string TavernModeAutoUnranked = "AutoUnranked";
        private static DateTime LastEditTime;
        private bool needClose = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void AddToStartup()
        {
            var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            registryKey.SetValue(Application.ProductName, Application.ExecutablePath);
        }


        private void Log(string text)
        {
            LogTextBox.BeginInvoke(new Action(() =>
            {
                LogTextBox.Text += $"[{DateTime.Now.ToString("F")}] {text}\r\n";
            }));
            Global.Log.Info(text);
        }

        private string GetTavernBankFilePath(string profilePath)
        {
            var banksDirectory = new DirectoryInfo(Path.Combine(profilePath, "Banks"));
            if (banksDirectory.Exists)
            {
                foreach (var bankDirectory in banksDirectory.GetDirectories())
                {
                    foreach (var bankFile in bankDirectory.GetFiles())
                    {
                        if (bankFile.Name == "SCBar.SC2Bank")
                        {
                            return bankFile.FullName;
                        }
                    }
                }
            }
            return null;
        }

        private void UploadAndWatchTavernBankFiles()
        {
            var starcraft2Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarCraft II");
            Log("星际2档案目录: " + starcraft2Path);
            var accountsDirectory = new DirectoryInfo(Path.Combine(starcraft2Path, "Accounts"));
            if (accountsDirectory.Exists)
            {
                foreach (var accountDirectory in accountsDirectory.GetDirectories())
                {
                    var accountNo = accountDirectory.Name;
                    foreach (var accountSubDirectory in accountDirectory.GetDirectories())
                    {
                        var profileFolderRegex = new Regex(@"(\d*)-S2-(\d*)-(\d*)");
                        if (profileFolderRegex.IsMatch(accountSubDirectory.Name))
                        {
                            var matches = profileFolderRegex.Match(accountSubDirectory.Name);
                            var regionNo = int.Parse(matches.Groups[1].Value);
                            var realmNo = int.Parse(matches.Groups[2].Value);
                            var profileNo = long.Parse(matches.Groups[3].Value);
                            if (realmNo > 0)
                            {
                                var tavernBankFilePath = GetTavernBankFilePath(accountSubDirectory.FullName);
                                if (tavernBankFilePath != null)
                                {
                                    UploadBankData(tavernBankFilePath, regionNo, realmNo, profileNo);
                                }
                                var watcher = new FileSystemWatcher();
                                watcher.Path = accountSubDirectory.FullName;
                                watcher.IncludeSubdirectories = true;
                                watcher.NotifyFilter = NotifyFilters.LastWrite;
                                watcher.Filter = "SCBar.SC2Bank";
                                watcher.Changed += (object sender, FileSystemEventArgs e) =>
                                {
                                    if (LastEditTime == null || DateTime.Now - LastEditTime > TimeSpan.FromSeconds(5))
                                    {
                                        // 处理重复触发的问题，5 秒内只处理一次
                                        LastEditTime = DateTime.Now;
                                        Thread.Sleep(1000);
                                        UploadBankData(e.FullPath, regionNo, realmNo, profileNo);
                                    }
                                };
                                watcher.EnableRaisingEvents = true;
                            }
                        }
                    }
                }
            }
            else
            {
                Log("Accounts 目录不存在：" + accountsDirectory.FullName);
            }
        }

        private void UploadBankData(string filePath, int regionNo, int realmNo, long profileNo)
        {
            var regionName = GetRegionNameFromRegionNo(regionNo);
            Log($"【{regionName}】酒馆存档文件：{filePath}");
            var xmlString = File.ReadAllText(filePath, Encoding.UTF8);
            var doc = XDocument.Parse(xmlString);
            var sections = doc.Descendants("Section");
            var dataList = new List<TavernData>();
            dataList.Add(GetTravernDataFromSection(sections, TavernModeStandardRanked, regionNo, realmNo, profileNo));
            dataList.Add(GetTravernDataFromSection(sections, TavernModeStandardUnranked, regionNo, realmNo, profileNo));
            dataList.Add(GetTravernDataFromSection(sections, TavernModeAutoRanked, regionNo, realmNo, profileNo));
            dataList.Add(GetTravernDataFromSection(sections, TavernModeAutoUnranked, regionNo, realmNo, profileNo));

            foreach (var data in dataList)
            {
                if (data != null)
                {
                    HttpHelper.Post($"{Properties.Settings.Default.ApiOrigin}/tavern/upload", data);
                    Log($"{regionNo}-S2-{realmNo}-{profileNo} 数据上传: {JsonConvert.SerializeObject(data)}");
                }
            }
        }

        private string GetRegionNameFromRegionNo(int regionNo)
        {
            if (regionNo == 1)
            {
                return "美服";
            }
            if (regionNo == 2)
            {
                return "欧服";
            }
            if (regionNo == 3)
            {
                return "韩服";
            }
            if (regionNo == 5)
            {
                return "国服";
            }
            return "未知";
        }

        private TavernData GetTravernDataFromSection(IEnumerable<XElement> sections, string sectionName, int regionNo, int realmNo, long profileNo)
        {
            var section = sections.FirstOrDefault(s => s.Attribute("name").Value == sectionName);
            if (section != null)
            {
                var elements = section.Elements();
                return new TavernData
                {
                    mode = sectionName,
                    regionNo = regionNo,
                    realmNo = realmNo,
                    profileNo = profileNo,
                    top1 = (int)elements.First(n => n.Attribute("name").Value == "1st").Element("Value").Attribute("int"),
                    top4 = (int)elements.First(n => n.Attribute("name").Value == "wins").Element("Value").Attribute("int"),
                    games = (int)elements.First(n => n.Attribute("name").Value == "games").Element("Value").Attribute("int"),
                    elo = (double)elements.First(n => n.Attribute("name").Value == "elo").Element("Value").Attribute("fixed"),
                    code = (long)elements.First(n => n.Attribute("name").Value == "code").Element("Value").Attribute("int"),
                    code2 = (long?)elements.FirstOrDefault(n => n.Attribute("name").Value == "code2")?.Element("Value").Attribute("int"),
                    osVersion = Environment.OSVersion.Version.ToString(),
                    collectorVersion = Global.CurrentVersion.ToString()
                };
            }
            else
            {
                return null;
            }
        }

        private void CheckNewVersion()
        {
            try
            {
                Task.Run(() =>
                {
                    var response = HttpHelper.Get($"{Properties.Settings.Default.ApiOrigin}/app/code/tavern-data-collector");
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
                                    StartProcess(Properties.Settings.Default.UpdaterAppName);
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
                        Log($"更新检查失败，code: {(int)response.code}，请手动前往 https://starcraft.zone/tavern 检查更新");
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

        private void StartProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                Log("准备启动更新器 " + processName);
                Process.Start(Path.Combine(Application.StartupPath, processName + ".exe"));
            }
        }

        private void RestoreMainForm()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Log($"从托盘恢复");
                Show();
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Log($"程序启动 {Global.CurrentVersion}");
            VersionLabel.Text = $"版本号: V{Global.CurrentVersion}";
            UploadAndWatchTavernBankFiles();
            CheckNewVersion();
            AddToStartup();
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (needClose)
            {
                Log("程序退出");
            }
            else
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                Hide();
                ShowInTaskbar = false;
                Log("程序退出到托盘");
            }
        }

        private void SczoneLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(((LinkLabel)sender).Text) { UseShellExecute = true });
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            UploadAndWatchTavernBankFiles();
        }

        private void CheckVersionButton_Click(object sender, EventArgs e)
        {
            CheckNewVersion();
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                ShowInTaskbar = false;
            }
        }

        private void AppNotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            RestoreMainForm();
        }

        private void AppNotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                RestoreMainForm();
            }
        }

        private void AppNotifyIconToolStripMenuItemShowForm_Click(object sender, EventArgs e)
        {
            RestoreMainForm();
        }

        private void AppNotifyIconToolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            needClose = true;
            Close();
        }
    }
}
