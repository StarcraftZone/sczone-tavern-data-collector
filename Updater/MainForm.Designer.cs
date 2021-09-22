
namespace SczoneTavernDataCollector.Updater
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.updateProgressBar = new System.Windows.Forms.ProgressBar();
            this.textLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // updateProgressBar
            // 
            this.updateProgressBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.updateProgressBar.Location = new System.Drawing.Point(20, 20);
            this.updateProgressBar.Margin = new System.Windows.Forms.Padding(4);
            this.updateProgressBar.Name = "updateProgressBar";
            this.updateProgressBar.Size = new System.Drawing.Size(436, 61);
            this.updateProgressBar.TabIndex = 0;
            // 
            // textLabel
            // 
            this.textLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textLabel.Location = new System.Drawing.Point(20, 118);
            this.textLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.textLabel.Name = "textLabel";
            this.textLabel.Size = new System.Drawing.Size(436, 28);
            this.textLabel.TabIndex = 1;
            this.textLabel.Text = "正在更新";
            this.textLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(168F, 168F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(476, 166);
            this.Controls.Add(this.textLabel);
            this.Controls.Add(this.updateProgressBar);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(500, 230);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(20);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "自动更新";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar updateProgressBar;
        private System.Windows.Forms.Label textLabel;
    }
}

