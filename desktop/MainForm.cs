using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using SmartVidkonAutomation.Helpers;

namespace SmartVidkonAutomation
{
    public partial class MainForm : Form
    {
        // ── TOP NAVIGATION BAR CONTROLS ─────────────────────────────────────
        private Panel _pnlTopNav;
        private Label _lblTopNavTitle;
        private RoundedButton _btnTopNavFolder;
        private RoundedButton _btnTopNavPlay;
        private RoundedButton _btnTopNavRefresh;
        private RoundedButton _btnTopNavLiveConsole;
        private RoundedButton _btnTopNavLicense;

        // ── LEFT SIDEBAR CONTROLS ───────────────────────────────────────────
        private Panel _pnlSidebar;
        private ModernSidebarButton _navStudio;
        private ModernSidebarButton _navFootageVertical;
        private ModernSidebarButton _navFootageLandscape;
        private ModernSidebarButton _navAudio;
        private ModernSidebarButton _navGrading;
        private ModernSidebarButton _navProjects;
        private ModernSidebarButton _navApi;
        private ModernSidebarButton _navSettings;

        // ── MULTI-VIEW CONTAINER & ACTIVE VIEW ──────────────────────────────
        private Panel _pnlViewContainer;
        private string _currentActiveView = "Studio";

        // View Panels
        private Panel _viewStudio;
        private Panel _viewFootageVertical;
        private Panel _viewFootageLandscape;
        private Panel _viewAudio;
        private Panel _viewGrading;
        private Panel _viewProjects;
        private Panel _viewApi;
        private Panel _viewSettings;

        public MainForm()
        {
            this.Text = "Smart Vidkon Automation — Mesin Perakitan Video Vertikal Edukasi 9:16";
            this.ClientSize = new Size(1280, 850);
            this.MinimumSize = new Size(1050, 700);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UIFactory.BgCanvas;

            // Load Application Icon
            try
            {
                string iconPath = AppPaths.AppIconPath;
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
                else
                {
                    this.Icon = SystemIcons.Application;
                }
            }
            catch { }

            InitializeMainLayout();
            SwitchSidebarView("Studio");
        }

        private void InitializeMainLayout()
        {
            this.Controls.Clear();

            // 1. TOP NAVIGATION BAR (H = 58px, Dock = Top, BackColor = White)
            _pnlTopNav = new Panel
            {
                Height = 58,
                Dock = DockStyle.Top,
                BackColor = Color.White
            };
            _pnlTopNav.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen p = new Pen(UIFactory.BorderColor, 1f))
                {
                    e.Graphics.DrawLine(p, 0, _pnlTopNav.Height - 1, _pnlTopNav.Width, _pnlTopNav.Height - 1);
                }
            };
            this.Controls.Add(_pnlTopNav);

            _lblTopNavTitle = new Label
            {
                Text = "Studio Video Vertikal 9:16",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = UIFactory.BrandDark,
                Location = new Point(20, 16),
                AutoSize = true,
                UseMnemonic = false
            };
            _pnlTopNav.Controls.Add(_lblTopNavTitle);

            // Top Header Action Buttons (Right Aligned)
            _btnTopNavFolder = UIFactory.CreateButton("Folder Hasil", 0, 12, 105, 34, UIFactory.BgSubtle, radius: 8);
            _btnTopNavFolder.Click += delegate(object s, EventArgs e) { OpenStudioFolder(); };
            _pnlTopNav.Controls.Add(_btnTopNavFolder);

            _btnTopNavPlay = UIFactory.CreateButton("Putar Video", 0, 12, 100, 34, UIFactory.BgSubtle, radius: 8);
            _btnTopNavPlay.Click += delegate(object s, EventArgs e) { ToggleVideoBigPreview(); };
            _pnlTopNav.Controls.Add(_btnTopNavPlay);

            _btnTopNavRefresh = UIFactory.CreateButton("Segarkan", 0, 12, 85, 34, UIFactory.BgSubtle, radius: 8);
            _btnTopNavRefresh.Click += delegate(object s, EventArgs e) { ResetStudioSettingsToDefault(); };
            _pnlTopNav.Controls.Add(_btnTopNavRefresh);

            _btnTopNavLiveConsole = UIFactory.CreateButton("Live Console", 0, 12, 110, 34, UIFactory.BgSubtle, radius: 8);
            _btnTopNavLiveConsole.Click += delegate(object s, EventArgs e) { LiveConsoleForm.Instance.Show(); LiveConsoleForm.Instance.BringToFront(); };
            _pnlTopNav.Controls.Add(_btnTopNavLiveConsole);

            string licStatus = LicenseManager.IsLicenseValid() ? "LISENSI AKTIF" : "TRIAL";
            Color licColor = LicenseManager.IsLicenseValid() ? UIFactory.AccentGreen : UIFactory.AccentAmber;
            _btnTopNavLicense = UIFactory.CreateButton(licStatus, 0, 12, 95, 34, licColor, radius: 8);
            _btnTopNavLicense.NormalText = Color.White;
            _btnTopNavLicense.Click += delegate(object s, EventArgs e) { SwitchSidebarView("Settings"); };
            _pnlTopNav.Controls.Add(_btnTopNavLicense);

            Action layoutTopNav = delegate
            {
                int curRight = _pnlTopNav.ClientSize.Width - 16;
                int minLeftLimit = (_lblTopNavTitle != null) ? _lblTopNavTitle.Right + 16 : 240;

                if (_btnTopNavLicense != null)
                {
                    _btnTopNavLicense.Top = 12;
                    _btnTopNavLicense.Left = Math.Max(minLeftLimit, curRight - _btnTopNavLicense.Width);
                    curRight = _btnTopNavLicense.Left - 8;
                }
                if (_btnTopNavLiveConsole != null)
                {
                    _btnTopNavLiveConsole.Top = 12;
                    _btnTopNavLiveConsole.Left = Math.Max(minLeftLimit, curRight - _btnTopNavLiveConsole.Width);
                    curRight = _btnTopNavLiveConsole.Left - 6;
                }
                if (_btnTopNavRefresh != null)
                {
                    _btnTopNavRefresh.Top = 12;
                    _btnTopNavRefresh.Left = Math.Max(minLeftLimit, curRight - _btnTopNavRefresh.Width);
                    curRight = _btnTopNavRefresh.Left - 6;
                }
                if (_btnTopNavPlay != null)
                {
                    _btnTopNavPlay.Top = 12;
                    _btnTopNavPlay.Left = Math.Max(minLeftLimit, curRight - _btnTopNavPlay.Width);
                    curRight = _btnTopNavPlay.Left - 6;
                }
                if (_btnTopNavFolder != null)
                {
                    _btnTopNavFolder.Top = 12;
                    _btnTopNavFolder.Left = Math.Max(minLeftLimit, curRight - _btnTopNavFolder.Width);
                }
            };

            _pnlTopNav.Resize += delegate(object s, EventArgs e) { layoutTopNav(); };
            layoutTopNav();

            // 2. LEFT SIDEBAR NAVIGATION (W = 220px, Dock = Left, BackColor = White)
            _pnlSidebar = new Panel
            {
                Width = 220,
                Dock = DockStyle.Left,
                BackColor = Color.White
            };
            _pnlSidebar.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen p = new Pen(UIFactory.BorderColor, 1f))
                {
                    e.Graphics.DrawLine(p, _pnlSidebar.Width - 1, 0, _pnlSidebar.Width - 1, _pnlSidebar.Height);
                    e.Graphics.DrawLine(p, 16, 120, _pnlSidebar.Width - 16, 120);
                }
            };
            this.Controls.Add(_pnlSidebar);

            // Sidebar Brand & Header
            Panel pnlSidebarHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(220, 120),
                BackColor = Color.White
            };

            PictureBox picSidebarLogo = new PictureBox
            {
                Size = new Size(52, 52),
                Location = new Point((220 - 52) / 2, 8),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            try
            {
                string pngLogo = Path.Combine(AppPaths.BrandingDir, "SmartVidkon_Logo.png");
                if (File.Exists(pngLogo))
                {
                    // Gunakan FileStream agar file PNG tidak terkunci di disk
                    using (FileStream fs = new FileStream(pngLogo, FileMode.Open, FileAccess.Read))
                    {
                        picSidebarLogo.Image = Image.FromStream(fs);
                    }
                }
                else if (this.Icon != null)
                {
                    picSidebarLogo.Image = this.Icon.ToBitmap();
                }
            }
            catch { }
            pnlSidebarHeader.Controls.Add(picSidebarLogo);

            Label lblSidebarBrand = new Label
            {
                Text = "SmartVidkon",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = UIFactory.BrandDark,
                Location = new Point(0, 62),
                Size = new Size(220, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlSidebarHeader.Controls.Add(lblSidebarBrand);

            Label lblSidebarSub = new Label
            {
                Text = "Powered by Fokus Konten",
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = UIFactory.TextSecondary,
                Location = new Point(0, 84),
                Size = new Size(220, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlSidebarHeader.Controls.Add(lblSidebarSub);

            _pnlSidebar.Controls.Add(pnlSidebarHeader);

            // Menu Items
            int btnY = 118;
            int btnH = 36;
            int btnGap = 3;
            int btnW = 192;
            int btnX = 14;

            _navStudio = new ModernSidebarButton { Text = "Studio Video", IconSymbol = "🎬", Location = new Point(btnX, btnY), Size = new Size(btnW, btnH), IsActive = true };
            _navStudio.Click += delegate(object s, EventArgs e) { SwitchSidebarView("Studio"); };
            _pnlSidebar.Controls.Add(_navStudio);
            btnY += btnH + btnGap;

            _navFootageVertical = new ModernSidebarButton { Text = "Footage Vertikal", IconSymbol = "📱", Location = new Point(btnX, btnY), Size = new Size(btnW, btnH) };
            _navFootageVertical.Click += delegate(object s, EventArgs e) { SwitchSidebarView("FootageVertical"); };
            _pnlSidebar.Controls.Add(_navFootageVertical);
            btnY += btnH + btnGap;

            _navFootageLandscape = new ModernSidebarButton { Text = "Footage Landscape", IconSymbol = "🖥️", Location = new Point(btnX, btnY), Size = new Size(btnW, btnH) };
            _navFootageLandscape.Click += delegate(object s, EventArgs e) { SwitchSidebarView("FootageLandscape"); };
            _pnlSidebar.Controls.Add(_navFootageLandscape);
            btnY += btnH + btnGap;

            _navAudio = new ModernSidebarButton { Text = "Musik & Audio", IconSymbol = "🎵", Location = new Point(btnX, btnY), Size = new Size(btnW, btnH) };
            _navAudio.Click += delegate(object s, EventArgs e) { SwitchSidebarView("Audio"); };
            _pnlSidebar.Controls.Add(_navAudio);
            btnY += btnH + btnGap;

            _navGrading = new ModernSidebarButton { Text = "Color Grading", IconSymbol = "🎨", Location = new Point(btnX, btnY), Size = new Size(btnW, btnH) };
            _navGrading.Click += delegate(object s, EventArgs e) { SwitchSidebarView("Grading"); };
            _pnlSidebar.Controls.Add(_navGrading);
            btnY += btnH + btnGap;

            _navProjects = new ModernSidebarButton { Text = "Riwayat Proyek", IconSymbol = "📋", Location = new Point(btnX, btnY), Size = new Size(btnW, btnH) };
            _navProjects.Click += delegate(object s, EventArgs e) { SwitchSidebarView("Projects"); };
            _pnlSidebar.Controls.Add(_navProjects);
            btnY += btnH + btnGap;

            _navApi = new ModernSidebarButton { Text = "Kunci Token & API", IconSymbol = "🔑", Location = new Point(btnX, btnY), Size = new Size(btnW, btnH) };
            _navApi.Click += delegate(object s, EventArgs e) { SwitchSidebarView("Api"); };
            _pnlSidebar.Controls.Add(_navApi);
            btnY += btnH + btnGap;

            _navSettings = new ModernSidebarButton { Text = "Pengaturan & Lisensi", IconSymbol = "⚙️", Location = new Point(btnX, btnY), Size = new Size(btnW, btnH) };
            _navSettings.Click += delegate(object s, EventArgs e) { SwitchSidebarView("Settings"); };
            _pnlSidebar.Controls.Add(_navSettings);

            // Footer Sidebar
            Label lblSidebarFooter = new Label
            {
                Text = "Smart Vidkon Automation v1.0.0\n• GPU NVENC: Siap",
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = UIFactory.TextMuted,
                Dock = DockStyle.Bottom,
                Height = 44,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _pnlSidebar.Controls.Add(lblSidebarFooter);

            // 3. MULTI-VIEW CONTAINER (Dock = Fill, BackColor = #F8F9FA)
            _pnlViewContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIFactory.BgCanvas,
                Padding = new Padding(16, 12, 16, 16)
            };
            this.Controls.Add(_pnlViewContainer);
            _pnlViewContainer.BringToFront();

            // Initialize all 8 View Panels
            InitializeStudioView();
            InitializeFootageVerticalView();
            InitializeFootageLandscapeView();
            InitializeAudioView();
            InitializeGradingView();
            InitializeProjectsView();
            InitializeApiView();
            InitializeSettingsView();
        }

        public void SwitchSidebarView(string viewName)
        {
            _currentActiveView = viewName;

            // Update sidebar buttons active state
            if (_navStudio != null) _navStudio.IsActive = (viewName == "Studio");
            if (_navFootageVertical != null) _navFootageVertical.IsActive = (viewName == "FootageVertical");
            if (_navFootageLandscape != null) _navFootageLandscape.IsActive = (viewName == "FootageLandscape");
            if (_navAudio != null) _navAudio.IsActive = (viewName == "Audio");
            if (_navGrading != null) _navGrading.IsActive = (viewName == "Grading");
            if (_navProjects != null) _navProjects.IsActive = (viewName == "Projects");
            if (_navApi != null) _navApi.IsActive = (viewName == "Api");
            if (_navSettings != null) _navSettings.IsActive = (viewName == "Settings");

            if (_pnlSidebar != null) _pnlSidebar.Invalidate(true);

            // Hide all views first
            if (_viewStudio != null) _viewStudio.Visible = false;
            if (_viewFootageVertical != null) _viewFootageVertical.Visible = false;
            if (_viewFootageLandscape != null) _viewFootageLandscape.Visible = false;
            if (_viewAudio != null) _viewAudio.Visible = false;
            if (_viewGrading != null) _viewGrading.Visible = false;
            if (_viewProjects != null) _viewProjects.Visible = false;
            if (_viewApi != null) _viewApi.Visible = false;
            if (_viewSettings != null) _viewSettings.Visible = false;

            // Show selected view and update header title
            switch (viewName)
            {
                case "Studio":
                    if (_viewStudio != null) _viewStudio.Visible = true;
                    if (_lblTopNavTitle != null) _lblTopNavTitle.Text = "Studio Video Vertikal 9:16";
                    LayoutStudioComponents();
                    break;
                case "FootageVertical":
                    if (_viewFootageVertical != null)
                    {
                        _viewFootageVertical.Visible = true;
                        RefreshFootageVerticalViewData();
                    }
                    if (_lblTopNavTitle != null) _lblTopNavTitle.Text = "Footage Vertikal (9:16)";
                    break;
                case "FootageLandscape":
                    if (_viewFootageLandscape != null)
                    {
                        _viewFootageLandscape.Visible = true;
                        RefreshFootageLandscapeViewData();
                    }
                    if (_lblTopNavTitle != null) _lblTopNavTitle.Text = "Footage Landscape (16:9)";
                    break;
                case "Audio":
                    if (_viewAudio != null)
                    {
                        _viewAudio.Visible = true;
                        RefreshAudioViewData();
                    }
                    if (_lblTopNavTitle != null) _lblTopNavTitle.Text = "Koleksi Musik & Audio (BGM)";
                    break;
                case "Grading":
                    if (_viewGrading != null)
                    {
                        _viewGrading.Visible = true;
                    }
                    if (_lblTopNavTitle != null) _lblTopNavTitle.Text = "Color Grading (3D LUTs Gallery)";
                    break;
                case "Projects":
                    if (_viewProjects != null)
                    {
                        _viewProjects.Visible = true;
                        RefreshProjectsList();
                    }
                    if (_lblTopNavTitle != null) _lblTopNavTitle.Text = "Riwayat Proyek Video Selesai";
                    break;
                case "Api":
                    if (_viewApi != null)
                    {
                        _viewApi.Visible = true;
                        RefreshApiViewData();
                    }
                    if (_lblTopNavTitle != null) _lblTopNavTitle.Text = "Kunci Token & Sekoci API";
                    break;
                case "Settings":
                    if (_viewSettings != null)
                    {
                        _viewSettings.Visible = true;
                        RefreshSettingsViewData();
                    }
                    if (_lblTopNavTitle != null) _lblTopNavTitle.Text = "Pengaturan Sistem & Lisensi";
                    break;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                StopBgmPlayback();
                StopVoPlayback();
            }
            catch { }

            try
            {
                KillActivePipeline();
            }
            catch { }

            try
            {
                if (LiveConsoleForm.Instance != null && !LiveConsoleForm.Instance.IsDisposed)
                {
                    LiveConsoleForm.Instance.Close();
                }
            }
            catch { }

            base.OnFormClosing(e);
        }
    }
}
