using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            Log("程序启动");
            FindTavernBankFiles();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log("程序退出");
        }

        private void Log(string text)
        {
            LogTextBox.Text += $"[{DateTime.Now.ToString("F")}] {text}\r\n";
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
                        Log(directory.Name);
                    }
                }
            }
            else
            {
                Log("Accounts 目录不存在：" + accountsDirectory.FullName);
            }
        }

    }
}
