using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

// Lock-key popup: light rounded card at top center, per-pixel alpha (smooth corners).
class Overlay : Form {
    [DllImport("user32.dll")] static extern short GetKeyState(int k);
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] static extern uint GetDpiForSystem();
    [DllImport("user32.dll")] static extern bool UpdateLayeredWindow(IntPtr h, IntPtr dst, ref Point pptDst, ref Size psize, IntPtr src, ref Point pptSrc, int key, ref BLEND b, int flags);
    [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr h);
    [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr h, IntPtr dc);
    [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr dc, IntPtr o);
    [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr o);
    [StructLayout(LayoutKind.Sequential)] struct BLEND { public byte Op, Flags, Alpha, Format; }

    const int WS_EX_TOOLWINDOW = 0x80, WS_EX_NOACTIVATE = 0x8000000, WS_EX_TRANSPARENT = 0x20, WS_EX_LAYERED = 0x80000;
    float sc = 1f; int pw, ph;
    Bitmap card; Point pos; string text = ""; bool on; int hold; byte alpha;
    Timer poll = new Timer { Interval = 60 }, fade = new Timer { Interval = 30 };
    bool lastCaps, lastNum, lastScroll;

    public Overlay() {
        FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false; TopMost = true;
        StartPosition = FormStartPosition.Manual;
        sc = GetDpiForSystem() / 96f; pw = (int)(160 * sc); ph = (int)(112 * sc);
        lastCaps = Get(0x14); lastNum = Get(0x90); lastScroll = Get(0x91);
        poll.Tick += (s, e) => Check(); poll.Start();
        fade.Tick += (s, e) => Fade(); fade.Start();
    }
    bool Get(int k) { return (GetKeyState(k) & 1) != 0; }
    protected override bool ShowWithoutActivation { get { return true; } }
    protected override CreateParams CreateParams { get { var cp = base.CreateParams;
        cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT | WS_EX_LAYERED; return cp; } }

    void Check() {
        bool c = Get(0x14), n = Get(0x90), s = Get(0x91);
        if (c != lastCaps) Pop("Caps Lock", "AA", c); else if (n != lastNum) Pop("Num Lock", "123", n); else if (s != lastScroll) Pop("Scroll Lock", "SL", s);
        lastCaps = c; lastNum = n; lastScroll = s;
    }
    void Pop(string name, string glyph, bool state) {
        on = state; text = name + (state ? " On" : " Off");
        if (card != null) card.Dispose();
        card = Render(glyph);
        var wa = Screen.PrimaryScreen.WorkingArea;
        pos = new Point(wa.Left + (wa.Width - pw) / 2, wa.Top + (int)(90 * sc));
        hold = 40; alpha = 255;
        if (!IsHandleCreated) CreateHandle();
        Push(); ShowWindow(Handle, 4);
    }
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr h, int c);
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr h, IntPtr a, int x, int y, int cx, int cy, uint f);

    Bitmap Render(string glyph) {
        var bmp = new Bitmap(pw, ph, PixelFormat.Format32bppPArgb);
        using (var g = Graphics.FromImage(bmp)) {
            g.SmoothingMode = SmoothingMode.AntiAlias; g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.Clear(Color.Transparent); g.ScaleTransform(sc, sc);
            using (var bg = RoundRect(new RectangleF(0, 0, 160, 112), 44))
            using (var b = new SolidBrush(Color.FromArgb(242, 242, 242))) g.FillPath(b, bg);

            // glyph and caption as paths, so their exact bounds drive the layout
            var gp = new GraphicsPath(); var cp = new GraphicsPath();
            gp.AddString(glyph, new FontFamily("Segoe UI Light"), 0, 42, new PointF(0, 0), StringFormat.GenericTypographic);
            cp.AddString(text, new FontFamily("Segoe UI"), 0, 13.5f, new PointF(0, 0), StringFormat.GenericTypographic);
            var gb = gp.GetBounds(); var cb = cp.GetBounds();
            float gap = 12, total = gb.Height + gap + cb.Height, top = (112 - total) / 2;
            var m = new Matrix(); m.Translate((160 - gb.Width) / 2 - gb.X, top - gb.Y); gp.Transform(m);
            m = new Matrix(); m.Translate((160 - cb.Width) / 2 - cb.X, top + gb.Height + gap - cb.Y); cp.Transform(m);
            g.FillPath(Brushes.Black, gp);
            g.FillPath(new SolidBrush(Color.FromArgb(90, 90, 90)), cp);
            if (!on) {
                var r = gp.GetBounds();
                using (var pen = new Pen(Color.Black, 2.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawLine(pen, r.Left - 5, r.Bottom + 4, r.Right + 5, r.Top - 4);
            }
        }
        return bmp;
    }
    static GraphicsPath RoundRect(RectangleF r, float d) { var p = new GraphicsPath();
        p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90); p.CloseFigure(); return p; }

    void Push() {
        if (card == null) return;
        IntPtr screen = GetDC(IntPtr.Zero), mem = CreateCompatibleDC(screen), hb = card.GetHbitmap(Color.FromArgb(0));
        IntPtr old = SelectObject(mem, hb);
        var size = new Size(pw, ph); var src = new Point(0, 0); var p = pos;
        var bl = new BLEND { Op = 0, Flags = 0, Alpha = alpha, Format = 1 };
        UpdateLayeredWindow(Handle, screen, ref p, ref size, mem, ref src, 0, ref bl, 2);
        SelectObject(mem, old); DeleteObject(hb); DeleteDC(mem); ReleaseDC(IntPtr.Zero, screen);
    }
    void Fade() {
        if (!IsWindowVisible(Handle)) return;
        if (hold > 0) { hold--; return; }
        if (alpha <= 16) { alpha = 0; ShowWindow(Handle, 0); return; }
        alpha -= 16; Push();
    }

    [STAThread] static void Main() {
        SetProcessDPIAware();
        bool created; var mx = new System.Threading.Mutex(true, "CapsPopupSingle", out created); if (!created) return;
        Application.EnableVisualStyles();
        var o = new Overlay(); o.CreateControl(); var h = o.Handle;
        var tray = new NotifyIcon { Icon = SystemIcons.Information, Text = "Lock key popup", Visible = true };
        var menu = new ContextMenu();
        var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
        var auto = new MenuItem("Start with Windows") { Checked = key.GetValue("CapsPopup") != null };
        auto.Click += (s, e) => { if (auto.Checked) key.DeleteValue("CapsPopup", false); else key.SetValue("CapsPopup", "\"" + Application.ExecutablePath + "\""); auto.Checked = !auto.Checked; };
        menu.MenuItems.Add(auto); menu.MenuItems.Add("Exit", (s, e) => { tray.Visible = false; Application.Exit(); });
        tray.ContextMenu = menu;
        Application.Run();
    }
}
