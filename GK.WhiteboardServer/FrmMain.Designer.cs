namespace GK.WhiteboardServer
{
    partial class FrmMain
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.notifier = new System.Windows.Forms.NotifyIcon(this.components);
            this.cmsItems = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.StartCalibration = new System.Windows.Forms.ToolStripMenuItem();
            this.LanguageType = new System.Windows.Forms.ToolStripMenuItem();
            this.UseSimplifiedChinese = new System.Windows.Forms.ToolStripMenuItem();
            this.UseTraditionalChinese = new System.Windows.Forms.ToolStripMenuItem();
            this.UseEnglish = new System.Windows.Forms.ToolStripMenuItem();
            this.UseGerman = new System.Windows.Forms.ToolStripMenuItem();
            this.AboutInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.AppExit = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsItems.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifier
            // 
            this.notifier.ContextMenuStrip = this.cmsItems;
            resources.ApplyResources(this.notifier, "notifier");
            this.notifier.Click += new System.EventHandler(this.notifier_Click);
            // 
            // cmsItems
            // 
            this.cmsItems.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StartCalibration,
            this.LanguageType,
            this.AboutInfo,
            this.AppExit});
            this.cmsItems.Name = "cmsItems";
            resources.ApplyResources(this.cmsItems, "cmsItems");
            // 
            // StartCalibration
            // 
            this.StartCalibration.Name = "StartCalibration";
            resources.ApplyResources(this.StartCalibration, "StartCalibration");
            this.StartCalibration.Click += new System.EventHandler(this.StartCalibration_Click);
            // 
            // LanguageType
            // 
            this.LanguageType.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UseSimplifiedChinese,
            this.UseTraditionalChinese,
            this.UseEnglish,
            this.UseGerman});
            this.LanguageType.Name = "LanguageType";
            resources.ApplyResources(this.LanguageType, "LanguageType");
            // 
            // UseSimplifiedChinese
            // 
            this.UseSimplifiedChinese.Name = "UseSimplifiedChinese";
            resources.ApplyResources(this.UseSimplifiedChinese, "UseSimplifiedChinese");
            this.UseSimplifiedChinese.Click += new System.EventHandler(this.UseSimplifiedChinese_Click);
            // 
            // UseTraditionalChinese
            // 
            this.UseTraditionalChinese.Name = "UseTraditionalChinese";
            resources.ApplyResources(this.UseTraditionalChinese, "UseTraditionalChinese");
            this.UseTraditionalChinese.Click += new System.EventHandler(this.UseTraditionalChinese_Click);
            // 
            // UseEnglish
            // 
            this.UseEnglish.Name = "UseEnglish";
            resources.ApplyResources(this.UseEnglish, "UseEnglish");
            this.UseEnglish.Click += new System.EventHandler(this.UseEnglish_Click);
            // 
            // UseGerman
            // 
            this.UseGerman.Name = "UseGerman";
            resources.ApplyResources(this.UseGerman, "UseGerman");
            this.UseGerman.Click += new System.EventHandler(this.UseGerman_Click);
            // 
            // AboutInfo
            // 
            this.AboutInfo.Name = "AboutInfo";
            resources.ApplyResources(this.AboutInfo, "AboutInfo");
            this.AboutInfo.Click += new System.EventHandler(this.AboutInfo_Click);
            // 
            // AppExit
            // 
            this.AppExit.Name = "AppExit";
            resources.ApplyResources(this.AppExit, "AppExit");
            this.AppExit.Click += new System.EventHandler(this.AppExit_Click);
            // 
            // FrmMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "FrmMain";
            this.ShowInTaskbar = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.cmsItems.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifier;
        private System.Windows.Forms.ContextMenuStrip cmsItems;
        private System.Windows.Forms.ToolStripMenuItem StartCalibration;
        private System.Windows.Forms.ToolStripMenuItem LanguageType;
        private System.Windows.Forms.ToolStripMenuItem UseSimplifiedChinese;
        private System.Windows.Forms.ToolStripMenuItem UseTraditionalChinese;
        private System.Windows.Forms.ToolStripMenuItem UseEnglish;
        private System.Windows.Forms.ToolStripMenuItem AboutInfo;
        private System.Windows.Forms.ToolStripMenuItem AppExit;
        private System.Windows.Forms.ToolStripMenuItem UseGerman;
    }
}

