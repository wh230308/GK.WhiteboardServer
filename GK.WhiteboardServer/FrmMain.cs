using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace GK.WhiteboardServer
{
    public partial class FrmMain : Form
    {
        /// <summary>
        /// 设备连接状态
        /// </summary>
        private enum ConnectStatus
        {
            None,
            Connected,
            Disconnected
        }

        //光标的位置
        private enum CrossPosition
        {
            firstCross,
            SecondCross,
            ThirdCross,
            FourthCross,
            FifthCross
        }

        //快捷键标定箭头的位置
        private enum ShortcutsPosition
        {
            TopLeft,
            TopRight,
            BottomRight,
            BottomLeft
        }

        #region // 常量
        private const int WM_USER = 0x400;
        private const int USER1 = WM_USER + 300;
        private const int USER2 = WM_USER + 1022;
        #endregion

        #region // 字段
        private int product_id = 0x6666;
        private int vendor_id = 0x8888;
        private int pixelH;
        private int pixelW;
        private Guid device_class;
        private IntPtr usb_event_handle;
        private SpecifiedDevice specified_device;
        private IntPtr handle;
        private System.Timers.Timer trPollingDev = new System.Timers.Timer(1000);
        private IntPtr targetFormHandle;
        private string filePath; //配置文件路径
        private Process shutDown = new Process();
        private Assembly asm = Assembly.GetEntryAssembly();
        private WhiteboardForm whiteboardFrmMy = new WhiteboardForm();
        private ShortcutsForm shortcutsFrmMy = new ShortcutsForm();
        private BackgroundWorker bgwReceiveData = new BackgroundWorker();
        private BackgroundWorker bgwSetPosition = new BackgroundWorker();
        private byte[] inputBufferReport = new byte[9];
        private CrossPosition crossPos;
        private ShortcutsPosition shortcutsPos;
        private ConnectStatus devConnectStatus;
        private bool SDFlag;
        #endregion

        #region // 事件
        public event EventHandler OnSpecifiedDeviceArrived;
        public event EventHandler OnSpecifiedDeviceRemoved;
        #endregion 

        #region // API函数
        //服务程序响应
        [DllImport("user32.dll", EntryPoint = "ReplyMessage")]
        private static extern bool ReplyMessage(int state);
        //发送消息
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        //设置窗体为最顶层窗口
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        //发送按键消息
        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        //查找窗口句柄
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        #endregion


        public FrmMain()
        {
            InitializeComponent();

            device_class = Win32Usb.HIDGuid;
            devConnectStatus = ConnectStatus.None;

            this.whiteboardFrmMy.ExceptionNotifition += new EventHandler(whiteboardFrmMy_ExceptionNotifition);
            this.shortcutsFrmMy.ExceptionNotifition += new EventHandler(shortcutsFrmMy_ExceptionNotifition);

            this.OnSpecifiedDeviceArrived += new EventHandler(FrmMain_OnSpecifiedDeviceArrived);
            this.OnSpecifiedDeviceRemoved += new EventHandler(FrmMain_OnSpecifiedDeviceRemoved);

            trPollingDev.Elapsed += new System.Timers.ElapsedEventHandler(trPollingDev_Elapsed);

            bgwReceiveData.WorkerSupportsCancellation = true;
            bgwReceiveData.DoWork += new DoWorkEventHandler(bgwReceiveData_DoWork);

            bgwSetPosition.WorkerSupportsCancellation = true;
            bgwSetPosition.DoWork += new DoWorkEventHandler(bgwSetPosition_DoWork);
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            //启动定时器轮询
            trPollingDev.Enabled = true;

            //设置默认语言
            GetDefaultLanguage();
        }

        /// <summary>
        /// 获取默认语言设置，默认语言设置为了English
        /// </summary>
        private void GetDefaultLanguage()
        {
            FileStream fs;
            StreamReader sr;
            string defaultLanguage;

            //设置配置文件路径
            filePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\GKBoard Software";

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            fs = new FileStream(filePath + @"\LanguageConfig.ini", FileMode.OpenOrCreate);
            sr = new StreamReader(fs, Encoding.Default);
            defaultLanguage = sr.ReadLine();
            sr.Close();
            fs.Close();

            switch (defaultLanguage)
            {
                case "zh-CN":
                    this.UseSimplifiedChinese.Checked = true;
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");
                    this.ChangeLanguage();
                    this.notifier.Text = "设备未连接！";
                    break;

                case "zh-TW":
                    this.UseTraditionalChinese.Checked = true;
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-TW");
                    this.ChangeLanguage();
                    this.notifier.Text = "設備未連接！";
                    break;

                case "en":
                    this.UseEnglish.Checked = true;
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
                    this.ChangeLanguage();
                    this.notifier.Text = "Device is not connected！";
                    break;

                case "de":
                    this.UseGerman.Checked = true;
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("de");
                    this.ChangeLanguage();
                    this.notifier.Text = "Ger\x00e4t ist nicht angeschlossen！";
                    break;

                default:
                    this.UseSimplifiedChinese.Checked = true;
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");
                    this.ChangeLanguage();
                    this.notifier.Text = "设备未连接！";
                    break;


            }
        }

        /// <summary>
        /// 切换程序界面语言语言
        /// </summary>
        private void ChangeLanguage()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(FrmMain));

            foreach (Control control in base.Controls)
            {
                manager.ApplyResources(control, control.Name);
            }
            foreach (ToolStripItem item in this.cmsItems.Items)
            {
                manager.ApplyResources(item, item.Name);
            }
            foreach (ToolStripItem item in this.LanguageType.DropDownItems)
            {
                manager.ApplyResources(item, item.Name);
            }
            //foreach (ToolStripItem item in this.ShortcutsCalibration.DropDownItems)
            //{
            //    manager.ApplyResources(item, item.Name);
            //}
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            usb_event_handle = Win32Usb.RegisterForUsbEvents(Handle, device_class);
            this.handle = Handle;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == USER1)
            {
                if (m.WParam.ToInt32() == 100)
                {
                    this.targetFormHandle = m.LParam;
                }

                ReplyMessage(1);
            }

            if (m.Msg == Win32Usb.WM_DEVICECHANGE)
            {
                OnDeviceChange(m);
            }

            base.WndProc(ref m);
        }

        private void OnDeviceChange(Message m)
        {
            try
            {
                if ((m.WParam.ToInt32() == Win32Usb.DEVICE_ARRIVAL))
                {
                    //if (MyDeviceManagement.DeviceNameMatch(m, myDevicePathName))
                    //{
                    CheckDevicePresent();
                    //}

                }
                else if (m.WParam.ToInt32() == Win32Usb.DEVICE_REMOVECOMPLETE)
                {
                    //if (MyDeviceManagement.DeviceNameMatch(m, myDevicePathName))
                    //{
                    CheckDevicePresent();
                    //}
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void trPollingDev_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Invoke(new System.Timers.ElapsedEventHandler(trPollingDev_Elapsed), new object[] { sender, e });
            }
            else
            {
                bool result = CheckDevicePresent();
                if (result)
                {
                    trPollingDev.Enabled = false;
                }
            }
        }

        //设备添加通知
        private void FrmMain_OnSpecifiedDeviceArrived(object sender, EventArgs e)
        {
            devConnectStatus = ConnectStatus.Connected;

            if (UseSimplifiedChinese.Checked)
            {
                try
                {
                    this.notifier.ShowBalloonTip(1, "交互式电子白板", "设备已连接!", ToolTipIcon.Info);
                    this.notifier.Text = "服务正在运行！";
                    this.notifier.Icon = new Icon(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.greenNotify.ico"));
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
            else if (UseTraditionalChinese.Checked)
            {
                this.notifier.ShowBalloonTip(1, "交互式電子白板", "設備已連接!", ToolTipIcon.Info);
                this.notifier.Text = "服务正在運行！";
                this.notifier.Icon = new Icon(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.greenNotify.ico"));
            }
            else if (UseEnglish.Checked)
            {
                this.notifier.ShowBalloonTip(1, "Interactive whiteboard", "Device is connected!", ToolTipIcon.Info);
                this.notifier.Text = "Service is running！";
                this.notifier.Icon = new Icon(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.greenNotify.ico"));
            }
            else if (UseGerman.Checked)
            {
                this.notifier.ShowBalloonTip(1, "Interactive Whiteboard", "Gerät ist angeschlossen!", ToolTipIcon.Info);
                this.notifier.Text = "Service läuft！";
                this.notifier.Icon = new Icon(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.greenNotify.ico"));
            }

        }

        //设备移除通知
        private void FrmMain_OnSpecifiedDeviceRemoved(object sender, EventArgs e)
        {
            devConnectStatus = ConnectStatus.Disconnected;

            if (this.UseSimplifiedChinese.Checked)
            {
                this.notifier.ShowBalloonTip(1, "交互式电子白板", "设备连接断开!", ToolTipIcon.Error);
                this.notifier.Text = "服务停止运行！";
                this.notifier.Icon = new Icon(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.grayNotify.ico"));
            }
            else if (this.UseTraditionalChinese.Checked)
            {
                this.notifier.ShowBalloonTip(1, "交互式電子白板", "設備連接斷開!", ToolTipIcon.Error);
                this.notifier.Text = "服務停止运行！";
                this.notifier.Icon = new Icon(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.grayNotify.ico"));
            }
            else if (this.UseEnglish.Checked)
            {
                this.notifier.ShowBalloonTip(1, "Interactive whiteboard", "Device is disconnected!", ToolTipIcon.Error);
                this.notifier.Text = "Service stops running！";
                this.notifier.Icon = new Icon(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.grayNotify.ico"));
            }
            else if (this.UseGerman.Checked)
            {
                this.notifier.ShowBalloonTip(1, "Interactive Whiteboard", "Gerät ist nicht angeschlossen!", ToolTipIcon.Error);
                this.notifier.Text = "Gerät ist getrennt！";
                this.notifier.Icon = new Icon(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.grayNotify.ico"));
            }
        }

        //检查当前设备
        public bool CheckDevicePresent()
        {

            //Mind if the specified device existed before.
            bool history = false;
            if (specified_device != null)
            {
                history = true;
            }

            specified_device = SpecifiedDevice.FindSpecifiedDevice(this.vendor_id, this.product_id);
            if (specified_device != null)	// did we find it?
            {
                StartCalibration.Enabled = true;
                //SingleSideCalibration.Enabled = true;
                //DoubleSideCalibration.Enabled = true;

                if (OnSpecifiedDeviceArrived != null)
                {
                    this.OnSpecifiedDeviceArrived(this, new EventArgs());
                    specified_device.DataRecieved += new DataRecievedEventHandler(specified_device_DataRecieved);
                }
                return true;
            }
            else
            {
                StartCalibration.Enabled = false;
                //SingleSideCalibration.Enabled = false;
                //DoubleSideCalibration.Enabled = false;

                if (OnSpecifiedDeviceRemoved != null && history)
                {
                    this.OnSpecifiedDeviceRemoved(this, new EventArgs());
                }
                return false;
            }



            
        }

        private void specified_device_DataRecieved(object sender, DataRecievedEventArgs args)
        {
            if (!bgwReceiveData.IsBusy)
            {
                try
                {
                    Array.Clear(inputBufferReport, 0, inputBufferReport.Length);
                    inputBufferReport = args.data;
                    bgwReceiveData.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }

        private void bgwReceiveData_DoWork(object sender, DoWorkEventArgs e)
        {

            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new DoWorkEventHandler(bgwReceiveData_DoWork), new object[] { sender, e });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                //白板校准
                CalibrateWhiteBoard(inputBufferReport);

                //快捷键标定
                CalibrateShortcuts(inputBufferReport);

                //发送快捷键命令
                SendShortcutsValue(inputBufferReport);
            }

        }

        //重置光标颜色
        private void ResetIconColor()
        {
            whiteboardFrmMy.pic1Image = null;
            whiteboardFrmMy.pic2Image = null;
            whiteboardFrmMy.pic3Image = null;
            whiteboardFrmMy.pic4Image = null;
            whiteboardFrmMy.pic5Image = null;
            whiteboardFrmMy.pic6Image = null;
            whiteboardFrmMy.pic7Image = null;
            whiteboardFrmMy.pic8Image = null;
            whiteboardFrmMy.pic9Image = null;
        }

        //白板校准
        private void CalibrateWhiteBoard(byte[] inputReport)
        {
            whiteboardFrmMy.IsComplete = false;

            if (inputReport[1] == 0x07)
            {
                
                crossPos = CrossPosition.firstCross;
                return;
            }

            switch (crossPos)
            {
                case CrossPosition.firstCross:
                    if (inputReport[1] == 0x08)
                    {
                        //显示第二个光标时，第一个置空
                        whiteboardFrmMy.pic1Image = null;

                        //设置第二个光标的位置，并显示光标
                        whiteboardFrmMy.pic2Location = new Point((pixelW / 8) * 7 - 50, pixelH / 8 - 50);
                        whiteboardFrmMy.pic2Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.blackCross.ico"));

                        //设置第一 个箭头的位置，并显示箭头
                        whiteboardFrmMy.pic6Location = new Point(pixelW / 2 - 48, pixelH / 8 - 16);
                        whiteboardFrmMy.pic6Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.Arrow-right.bmp"));

                        crossPos = CrossPosition.SecondCross;

                    }
                    break;

                case CrossPosition.SecondCross:
                    if (inputReport[1] == 0x09)
                    {
                        whiteboardFrmMy.pic2Image = null;

                        whiteboardFrmMy.pic3Location = new Point((pixelW / 8) * 7 - 50, (pixelH / 8) * 7 - 50);
                        whiteboardFrmMy.pic3Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.blackCross.ico"));

                        whiteboardFrmMy.pic6Image = null;

                        whiteboardFrmMy.pic7Location = new Point((pixelW / 8) * 7 - 16, pixelH / 2 - 48);
                        whiteboardFrmMy.pic7Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.Arrow-down.bmp"));

                        crossPos = CrossPosition.ThirdCross;
                    }
                    break;

                case CrossPosition.ThirdCross:
                    if (inputReport[1] == 0x0a)
                    {
                        whiteboardFrmMy.pic3Image = null;

                        whiteboardFrmMy.pic4Location = new Point(pixelW / 8 - 50, (pixelH / 8) * 7 - 50); 
                        whiteboardFrmMy.pic4Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.blackCross.ico"));

                        whiteboardFrmMy.pic7Image = null;

                        whiteboardFrmMy.pic8Location = new Point(pixelW / 2 - 48, (pixelH / 8) * 7 + 16);
                        whiteboardFrmMy.pic8Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.Arrow-left.bmp"));

                        crossPos = CrossPosition.FourthCross;
                    }
                    break;

                case CrossPosition.FourthCross:
                    if (inputReport[1] == 0x0b)
                    {
                        whiteboardFrmMy.pic4Image = null;

                        whiteboardFrmMy.pic5Location = new Point(pixelW / 2 - 50, pixelH / 2 - 50);
                        whiteboardFrmMy.pic5Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.blackCross.ico"));

                        whiteboardFrmMy.pic8Image = null;

                        whiteboardFrmMy.pic9Location = new Point((pixelW / 16) * 5 - 24, (pixelH / 16) * 11 - 24);
                        whiteboardFrmMy.pic9Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.Arrow-next.bmp"));

                        crossPos = CrossPosition.FifthCross;
                    }
                    break;

                case CrossPosition.FifthCross:
                    if (inputReport[1] == 0x0c)
                    {
                        whiteboardFrmMy.pic5Image = null;
                        whiteboardFrmMy.pic9Image = null;

                        crossPos = CrossPosition.FifthCross;

                        //刷新显示
                        whiteboardFrmMy.Refresh();

                        //校准完成，设置标志为true，并发送正常退出报表
                        whiteboardFrmMy.IsComplete = true;
                        byte[] data = new byte[9];
                        data[0] = 0x00;
                        data[1] = 0x02;
                        specified_device.SendData(data);

                        //1秒后退出，并重置光标颜色
                        Thread.Sleep(1000);
                        whiteboardFrmMy.Hide();
                        ResetIconColor();
                    }
                    break;

            }



        }

        //白板校准异常退出
        private void whiteboardFrmMy_ExceptionNotifition(object sender, EventArgs e)
        {
            //发送异常退出报表
            byte[] data = new byte[9];
            data[0] = 0x01;
            data[1] = 0x10;
            specified_device.SendData(data);

            ResetIconColor();
        }

        private void ResetArrow()
        {
            shortcutsFrmMy.pic1Image = null;
            shortcutsFrmMy.pic2Image = null;
            shortcutsFrmMy.pic3Image = null;
            shortcutsFrmMy.pic4Image = null;
        }

        //快捷键标定
        private void CalibrateShortcuts(byte[] inputReport)
        {
            shortcutsFrmMy.IsComplete = false;

            if (inputReport[1] == 0x10)
            {
                shortcutsPos = ShortcutsPosition.TopRight;
                string notifyString = null;
                using (Graphics gh = shortcutsFrmMy.CreateGraphics())
                {
                    gh.Clear(shortcutsFrmMy.BackColor);
                    if (UseSimplifiedChinese.Checked)
                    {
                        notifyString = "提示：请点击右侧第一个快捷键中心！";
                    }
                    else if (UseTraditionalChinese.Checked)
                    {
                        notifyString = "提示：請點擊右側第一個快捷鍵中心！";
                    }
                    else if (UseEnglish.Checked)
                    {
                        notifyString = "Hint: Please click the center of the first shortcut key at right！";
                    }
                    Font font = new Font("Arial", 20);
                    SizeF sizef = gh.MeasureString(notifyString, font);
                    gh.DrawString(notifyString, font, new SolidBrush(Color.Black), new PointF(pixelW / 2 - sizef.Width / 2, pixelH / 2 - 200));
                }

                shortcutsFrmMy.pic1Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.Arrow-right.bmp"));
                shortcutsFrmMy.pic1Location = new Point((pixelW / 8) * 7, pixelH / 8);
                return;
            }

            switch (shortcutsPos)
            {

                case ShortcutsPosition.TopRight:
                    #region
                    if (inputReport[1] == 0x1b)
                    {
                        shortcutsPos = ShortcutsPosition.BottomRight;
                        string notifyString = null;

                        using (Graphics gh = shortcutsFrmMy.CreateGraphics())
                        {
                            gh.Clear(shortcutsFrmMy.BackColor);
                            if (UseSimplifiedChinese.Checked)
                            {
                                notifyString = "提示：请点击右侧最后一个快捷键中心！";
                            }
                            else if (UseTraditionalChinese.Checked)
                            {
                                notifyString = "提示：請點擊右側最後一個快捷鍵中心！";
                            }
                            else if (UseEnglish.Checked)
                            {
                                notifyString = "Hint: Please click the center of the last shortcut key at right ！";
                            }
                            Font font = new Font("Arial", 20);
                            SizeF sizef = gh.MeasureString(notifyString, font);
                            gh.DrawString(notifyString, font, new SolidBrush(Color.Black), new PointF(pixelW / 2 - sizef.Width / 2, pixelH / 2 - 200));
                        }

                        shortcutsFrmMy.pic1Image = null;
                        shortcutsFrmMy.pic2Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.Arrow-right.bmp"));
                        shortcutsFrmMy.pic2Location = new Point((pixelW / 8) * 7, (pixelH / 8) * 7 - 16);
                    }
                    #endregion
                    break;

                case ShortcutsPosition.BottomRight:
                    #region
                    if (inputReport[1] == 0x1c)
                    {
                        string notifyString = null;
                        //单侧校准SDFlag = false，完成后退出
                        if (!SDFlag)
                        {
                            using (Graphics gh = shortcutsFrmMy.CreateGraphics())
                            {
                                gh.Clear(shortcutsFrmMy.BackColor);
                                if (UseSimplifiedChinese.Checked)
                                {
                                    notifyString = "提示：快捷键标定完成！";
                                }
                                else if (UseTraditionalChinese.Checked)
                                {
                                    notifyString = "提示：快捷鍵標定完成！";
                                }
                                else if (UseEnglish.Checked)
                                {
                                    notifyString = "Hint: Shortcut key calibration finished!";
                                }
                                Font font = new Font("Arial", 20);
                                SizeF sizef = gh.MeasureString(notifyString, font);
                                gh.DrawString(notifyString, font, new SolidBrush(Color.Black), new PointF(pixelW / 2 - sizef.Width / 2, pixelH / 2 - 200));
                                shortcutsFrmMy.pic1Image = null;
                                shortcutsFrmMy.pic2Image = null;
                            }

                            //标定完成，发送正常退出报表
                            shortcutsFrmMy.IsComplete = true;
                            byte[] data = new byte[9];
                            data[1] = 0x02;
                            specified_device.SendData(data);

                            shortcutsFrmMy.Hide();
                            break;
                        }

                        shortcutsPos = ShortcutsPosition.TopLeft;

                        using (Graphics gh = shortcutsFrmMy.CreateGraphics())
                        {
                            gh.Clear(shortcutsFrmMy.BackColor);
                            if (UseSimplifiedChinese.Checked)
                            {
                                notifyString = "提示：请点击左侧第一个快捷键中心！";
                            }
                            else if (UseTraditionalChinese.Checked)
                            {
                                notifyString = "提示：請點擊左側第一個快捷鍵中心！";
                            }
                            else if (UseEnglish.Checked)
                            {
                                notifyString = "Hint: Please click the center of the first shortcut key at left！";
                            }
                            Font font = new Font("Arial", 20);
                            SizeF sizef = gh.MeasureString(notifyString, font);
                            gh.DrawString(notifyString, font, new SolidBrush(Color.Black), new PointF(pixelW / 2 - sizef.Width / 2, pixelH / 2 - 200));
                        }
                        shortcutsFrmMy.pic1Image = null;
                        shortcutsFrmMy.pic2Image = null;
                        shortcutsFrmMy.pic3Location = new Point(pixelW / 8 - 96, pixelH / 8);
                    }
                    #endregion
                    break;

                case ShortcutsPosition.TopLeft:
                    #region
                    if (inputReport[1] == 0x1d)
                    {
                        shortcutsPos = ShortcutsPosition.BottomLeft;
                        string notifyString = null;
                        using (Graphics gh = shortcutsFrmMy.CreateGraphics())
                        {
                            gh.Clear(shortcutsFrmMy.BackColor);
                            if (UseSimplifiedChinese.Checked)
                            {
                                notifyString = "提示：请点击左侧最后一个快捷键中心！";
                            }
                            else if (UseTraditionalChinese.Checked)
                            {
                                notifyString = "提示：請點擊左側最後一個快捷鍵中心！";
                            }
                            else if (UseEnglish.Checked)
                            {
                                notifyString = "Hint: Please click the center of the last shortcut key at left！";
                            }
                            Font font = new Font("Arial", 20);
                            SizeF sizef = gh.MeasureString(notifyString, font);
                            gh.DrawString(notifyString, font, new SolidBrush(Color.Black), new PointF(pixelW / 2 - sizef.Width / 2, pixelH / 2 - 200));
                        }

                        shortcutsFrmMy.pic1Image = null;
                        shortcutsFrmMy.pic2Image = null;
                        shortcutsFrmMy.pic3Image = null;
                        shortcutsFrmMy.pic4Location = new Point(pixelW / 8 - 96, (pixelH / 8) * 7 - 16);
                    }
                    #endregion
                    break;

                case ShortcutsPosition.BottomLeft:
                    #region
                    if (inputReport[1] == 0x1e)
                    {
                        string notifyString = null;
                        using (Graphics gh = shortcutsFrmMy.CreateGraphics())
                        {
                            gh.Clear(shortcutsFrmMy.BackColor);
                            if (UseSimplifiedChinese.Checked)
                            {
                                notifyString = "提示：快捷键标定完成！";
                            }
                            else if (UseTraditionalChinese.Checked)
                            {
                                notifyString = "提示：快捷鍵標定完成！";
                            }
                            else if (UseEnglish.Checked)
                            {
                                notifyString = "Hint: Shortcut key calibration finished!";
                            }
                            Font font = new Font("Arial", 20);
                            SizeF sizef = gh.MeasureString(notifyString, font);
                            gh.DrawString(notifyString, font, new SolidBrush(Color.Black), new PointF(pixelW / 2 - sizef.Width / 2, pixelH / 2 - 200));
                        }

                        shortcutsFrmMy.pic1Image = null;
                        shortcutsFrmMy.pic2Image = null;
                        shortcutsFrmMy.pic3Image = null;
                        shortcutsFrmMy.pic4Image = null;

                        //标定完成，发送正常退出报表
                        shortcutsFrmMy.IsComplete = true;
                        byte[] data = new byte[9];
                        data[1] = 0x02;
                        specified_device.SendData(data);

                        shortcutsFrmMy.Hide();
                    }
                    #endregion
                    break;

            }

        }

        //快捷键标定异常退出
        private void shortcutsFrmMy_ExceptionNotifition(object sender, EventArgs e)
        {
            //发送异常退出报表
            byte[] data = new byte[9];
            data[1] = 0x10;
            specified_device.SendData(data);

            ResetArrow();
        }

        //发送快捷键值
        private void SendShortcutsValue(byte[] inputReport)
        {
            IntPtr GKBoradHandle = GetProcessHandle("GKBoard Software");
            IntPtr PPTHandle;
            if (GetProcessHandle("POWERPNT") != IntPtr.Zero)
            {
                PPTHandle = GetProcessHandle("POWERPNT");
            }
            else if (GetProcessHandle("wpp") != IntPtr.Zero)
            {
                PPTHandle = GetProcessHandle("wpp");
            }
            else
            {
                PPTHandle = IntPtr.Zero;
            }

            

            #region // 应用程序默认快捷键
            switch (inputReport[1])
            {
                case 0xa0:
                    //控制模式
                    SendMessage(this.targetFormHandle, USER2, 20110902, 11);
                    break;

                case 0xa1:
                    //新建页
                    SendMessage(this.targetFormHandle, USER2, 20110902, 14);
                    break;

                case 0xa2:
                    //普通笔
                    SendMessage(this.targetFormHandle, USER2, 20110902, 10);
                    break;

                case 0xa3:
                    //荧光笔
                    SendMessage(this.targetFormHandle, USER2, 20110902, 7);
                    break;

                case 0xa4:
                    //排笔
                    SendMessage(this.targetFormHandle, USER2, 20110902, 8);
                    break;

                case 0xa5:
                    //毛笔
                    SendMessage(this.targetFormHandle, USER2, 20110902, 9);
                    break;

                case 0xa6:
                    //橡皮擦
                    SendMessage(this.targetFormHandle, USER2, 20110902, 6);
                    break;

                case 0xa7:
                    //页面放大
                    SendMessage(this.targetFormHandle, USER2, 20110902, 5);
                    break;

                case 0xa8:
                    //页面缩小
                    SendMessage(this.targetFormHandle, USER2, 20110902, 4);
                    break;

                case 0xa9:
                    //页面移动
                    SendMessage(this.targetFormHandle, USER2, 20110902, 12);
                    break;

                case 0xaa:
                    //黑板
                    SendMessage(this.targetFormHandle, USER2, 20110902, 13);
                    break;


                case 0xab:
                    //软键盘
                    SendMessage(this.targetFormHandle, USER2, 20110902, 1);
                    break;

                case 0xad:
                    //上一页
                    SendMessage(this.targetFormHandle, USER2, 20110902, 3);
                    break;
                case 0xae:
                    //下一页
                    SendMessage(this.targetFormHandle, USER2, 20110902, 2);
                    break;

                case 0xb0:
                    //自定义
                    //SendMessage(this.targetFormHandle, USER2, 20110902, 13);
                    break;

                case 0xb5:
                    //页面回放
                    //SendMessage(this.targetFormHandle, USER2, 20110902, 13);
                    break;

                case 0xb6:
                    //选择
                    SendMessage(this.targetFormHandle, USER2, 20110902, 13);
                    break;
            }
            #endregion

            #region // PPT
            if (PPTHandle != IntPtr.Zero)
            {
                switch (inputReport[1])
                {
                    case 0xac:
                        //PPT播放
                        SetForegroundWindow(PPTHandle);
                        keybd_event((byte)Keys.F5, 0, 0, 0);
                        keybd_event((byte)Keys.F5, 0, 2, 0);
                        break;

                    case 0xaf:
                        //PPT退出播放
                        SetForegroundWindow(PPTHandle);
                        keybd_event((byte)Keys.Escape, 0, 0, 0);
                        keybd_event((byte)Keys.Escape, 0, 2, 0);
                        break;

                    case 0xb2:
                        //PPT上一页
                        SetForegroundWindow(PPTHandle);
                        keybd_event((byte)Keys.PageUp, 0, 0, 0);
                        keybd_event((byte)Keys.PageUp, 0, 2, 0);
                        break;

                    case 0xb3:
                        //PPT下一页
                        SetForegroundWindow(PPTHandle);
                        keybd_event((byte)Keys.PageDown, 0, 0, 0);
                        keybd_event((byte)Keys.PageDown, 0, 2, 0);
                        break;
                }
            }
            #endregion

            #region // 启动程序
            if (inputReport[1] == 0xb1)
            {
                StartMainApp();
                return;
            }
            #endregion

            #region // 新增快捷键
            if (GKBoradHandle != IntPtr.Zero)
            {
                switch (inputReport[1])
                {
                    case 0xb4:
                        //窗口最小化
                        ShowWindow(GKBoradHandle, 2);
                        break;

                    case 0xb7:
                        //显示桌面
                        ShowDeskTop();
                        break;

                    case 0xb8:
                        //打印
                        SetForegroundWindow(GKBoradHandle);//打印
                        keybd_event((byte)Keys.ControlKey, 0, 0, 0);
                        keybd_event((byte)Keys.P, 0, 0, 0);
                        keybd_event((byte)Keys.ControlKey, 0, 2, 0);
                        keybd_event((byte)Keys.P, 0, 2, 0);
                        break;

                    case 0xb9:
                        //关闭计算机
                        shutDown.StartInfo.FileName = "shutdown";
                        shutDown.StartInfo.Arguments = "/s";
                        if (MessageBox.Show("是否关闭计算机？", "警告", MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                        {
                            shutDown.Start();
                        }
                        break;

                    case 0xba:
                        //定位

                        break;
                }
            }
            #endregion
        }

        //获取指定程序主窗口句柄
        private IntPtr GetProcessHandle(string myProcessName)
        {
            Process[] process = Process.GetProcesses();
            foreach (Process p in process)
            {
                //MessageBox.Show(p.ProcessName);
                if (p.ProcessName == myProcessName)
                {
                    return p.MainWindowHandle;

                }
            }
            return IntPtr.Zero;
        }

        //启动应用程序
        private void StartMainApp()
        {
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.FileName = "GKBoard Software.exe";
            Info.WorkingDirectory = Application.StartupPath;
            Info.WindowStyle = ProcessWindowStyle.Maximized;
            Process Proc;
            try
            {
                Proc = Process.Start(Info);
                System.Threading.Thread.Sleep(500);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        //显示桌面
        private void ShowDeskTop()
        {
            Type shellType = Type.GetTypeFromProgID("Shell.Application");
            object shellObject = System.Activator.CreateInstance(shellType);
            shellType.InvokeMember("ToggleDesktop", System.Reflection.BindingFlags.InvokeMethod, null, shellObject, null);
        }

        private void bgwSetPosition_DoWork(object sender, DoWorkEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Invoke(new DoWorkEventHandler(bgwSetPosition_DoWork), new object[] { sender, e });
            }
            else
            {
                //设置第一个光标的位置和内容
                whiteboardFrmMy.pic1Location = new Point(pixelW / 8 - 50, pixelH / 8 - 50);
                whiteboardFrmMy.pic1Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.blackCross.ico"));

                //设置提示文字位置和内容
                whiteboardFrmMy.pic10Location = new Point(pixelW / 2 - 325, pixelH / 4);

                if (UseSimplifiedChinese.Checked)
                {
                    whiteboardFrmMy.pic10Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.zh-CN.ico"));
                    //NotifyIcon
                }
                else if (UseTraditionalChinese.Checked)
                {
                    whiteboardFrmMy.pic10Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.zh-TW.ico"));
                }
                else if (UseEnglish.Checked)
                {
                    whiteboardFrmMy.pic10Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.en.ico"));
                }
                else if (UseGerman.Checked)
                {
                    whiteboardFrmMy.pic10Image = Image.FromStream(asm.GetManifestResourceStream("GK.WhiteboardServer.Image.de.ico"));
                }

            }
        }

        private void StartCalibration_Click(object sender, EventArgs e)
        {
            //获取屏幕分辨率,并设置第一个光标位置
            pixelH = Screen.PrimaryScreen.Bounds.Height;
            pixelW = Screen.PrimaryScreen.Bounds.Width;

            bgwSetPosition.RunWorkerAsync();

            whiteboardFrmMy.Show();

            //发送启动校准命令
            byte[] data = new byte[9];
            data[0] = 0x01;
            data[1] = 0x01;
            specified_device.SendData(data);
        }

        private void SingleSideCalibration_Click(object sender, EventArgs e)
        {
            pixelH = Screen.PrimaryScreen.Bounds.Height;
            pixelW = Screen.PrimaryScreen.Bounds.Width;
            
            //显示校准界面
            shortcutsFrmMy.Show();

            //单侧校准SDFlag = false,双侧校准SDFlag = true;
            SDFlag = false;

            //发送启动校准命令
            byte[] data = new byte[9];
            data[1] = 0x08;
            specified_device.SendData(data);
        }

        private void DoubleSideCalibration_Click(object sender, EventArgs e)
        {
            pixelH = Screen.PrimaryScreen.Bounds.Height;
            pixelW = Screen.PrimaryScreen.Bounds.Width;

            //显示校准界面
            shortcutsFrmMy.Show();

            //单侧校准SDFlag = false,双侧校准SDFlag = true;
            SDFlag = true;

            //发送启动校准命令
            byte[] data = new byte[9];
            data[1] = 0x08;
            specified_device.SendData(data);
        }

        private void UseSimplifiedChinese_Click(object sender, EventArgs e)
        {
            if (!UseSimplifiedChinese.Checked)
            {
                UseSimplifiedChinese.Checked = true;
                UseTraditionalChinese.Checked = false;
                UseEnglish.Checked = false;
                UseGerman.Checked = false;

                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");
                ChangeLanguage();

                if (devConnectStatus == ConnectStatus.None)
                {
                    notifier.Text = "设备未连接!";
                }
                else if (this.devConnectStatus == ConnectStatus.Connected)
                {
                    notifier.Text = "服务正在运行!";
                }
                else if (this.devConnectStatus == ConnectStatus.Disconnected)
                {
                    notifier.Text = "服务停止运行!";
                }
            }
        }

        private void UseTraditionalChinese_Click(object sender, EventArgs e)
        {
            if (!this.UseTraditionalChinese.Checked)
            {
                UseTraditionalChinese.Checked = true;
                UseSimplifiedChinese.Checked = false;
                UseEnglish.Checked = false;
                UseGerman.Checked = false;

                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-TW");
                ChangeLanguage();

                if (devConnectStatus == ConnectStatus.None)
                {
                    notifier.Text = "設備未連接!";
                }
                else if (devConnectStatus == ConnectStatus.Connected)
                {
                    notifier.Text = "服務正在運行!";
                }
                else if (devConnectStatus == ConnectStatus.Disconnected)
                {
                    notifier.Text = "服務停止運行!";
                }
            }
        }

        private void UseEnglish_Click(object sender, EventArgs e)
        {
            if (!UseEnglish.Checked)
            {
                UseEnglish.Checked = true;
                UseSimplifiedChinese.Checked = false;
                UseTraditionalChinese.Checked = false;
                UseGerman.Checked = false;

                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
                ChangeLanguage();

                if (devConnectStatus == ConnectStatus.None)
                {
                    notifier.Text = "Device is not connected!";
                }
                else if (devConnectStatus == ConnectStatus.Connected)
                {
                    notifier.Text = "Service is running!";
                }
                else if (devConnectStatus == ConnectStatus.Disconnected)
                {
                    notifier.Text = "Service stops running!";
                }
            }
        }

        private void UseGerman_Click(object sender, EventArgs e)
        {
            if (!UseGerman.Checked)
            {
                UseGerman.Checked = true;
                UseSimplifiedChinese.Checked = false;
                UseTraditionalChinese.Checked = false;
                UseEnglish.Checked = false;

                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("de");
                ChangeLanguage();

                if (devConnectStatus == ConnectStatus.None)
                {
                    notifier.Text = "Gerät ist nicht angeschlossen!";
                }
                else if (devConnectStatus == ConnectStatus.Connected)
                {
                    notifier.Text = "Service läuft!";
                }
                else if (devConnectStatus == ConnectStatus.Disconnected)
                {
                    notifier.Text = "Service aufgehört!";
                }
            }
        }


        private void AboutInfo_Click(object sender, EventArgs e)
        {
            AboutForm aboutFrom = new AboutForm();
            aboutFrom.Show();
        }

        private void AppExit_Click(object sender, EventArgs e)
        {
            if (this.UseSimplifiedChinese.Checked)
            {
                this.SavedSelectedLanguage("zh-CN");
            }
            else if (this.UseTraditionalChinese.Checked)
            {
                this.SavedSelectedLanguage("zh-TW");
            }
            else if (this.UseEnglish.Checked)
            {
                this.SavedSelectedLanguage("en");
            }
            else if (this.UseGerman.Checked)
            {
                this.SavedSelectedLanguage("de");
            }
            Application.Exit();
        }

        private void SavedSelectedLanguage(string culture)
        {
            try
            {
                try
                {
                    FileStream fs = new FileStream(filePath + @"\LanguageConfig.ini", FileMode.OpenOrCreate);
                    StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                    sw.WriteLine(culture);
                    sw.Close();
                    fs.Close();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
            finally
            {
            }
        }

        private void notifier_Click(object sender, EventArgs e)
        {
            NotifyIcon eventSource = null;
            Type nHandle = null;
            eventSource = (NotifyIcon)sender;
            nHandle = eventSource.GetType();

            nHandle.InvokeMember("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, eventSource, null);

        }

        

    }
}
