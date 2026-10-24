using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartVidkonAutomation.Helpers;

namespace SmartVidkonAutomation
{
    public partial class MainForm
    {
        // Direktori Master Sistem Video Edukasi (Portabel Mandiri)
        private static string StudioEducationRoot { get { return AppPaths.AppDir; } }
        private static string StudioMasterRunner { get { return AppPaths.PipelineScript; } }
        private static string StudioScanScript { get { return AppPaths.ScanScript; } }
        private static string StudioBgmLibrary { get { return AppPaths.BgmDir; } }
        private static string StudioLutsVault { get { return AppPaths.LutsDir; } }

        // Container Workspace Kiri & Kanan
        private RoundedCardPanel _pnlStudioLeft;
        private RoundedCardPanel _pnlStudioRight;

        // Input & Naskah Controls (Left Panel)
        private Label _lblTopicHeader;
        private TextBox _txtStudioTopic;
        private Label _lblThemeHeader;
        private ComboBox _cbStudioTheme;
        private Label _lblAudienceHeader;
        private ComboBox _cbStudioAudience;
        private Label _lblDurationHeader;
        private ComboBox _cbStudioDuration;
        private RoundedButton _btnStudioGenAi;
        private RoundedButton _btnStudioCheckAiQuota;
        private RoundedButton _btnStudioPolishScript;
        private RoundedButton _btnStudioUploadVoice;
        private RoundedButton _btnStudioPreviewVoice;
        private RoundedButton _btnStudioTranscribeVoice;
        private RoundedButton _btnStudioClearVoice;
        public static string _uploadedVoicePath = null;

        // Checkbox Koleksi Footage Karakter
        private Label _lblFootageHeader;
        private FlowLayoutPanel _flpFootageSkus;
        private CheckBox _chkFootageAuto;
        private CheckBox _chkFootageShelby;
        private CheckBox _chkFootageJoker;
        private CheckBox _chkFootageSenja;
        private CheckBox _chkFootageDark;
        private CheckBox _chkFootageAnime;
        private CheckBox _chkFootageHealing;
        private CheckBox _chkFootageHardWork;
        private bool _isUpdatingFootageCheckboxes = false;

        private Label _lblSentenceCount;
        private TextBox _txtStudioScript;

        // Parameter Vokal TTS & Intonasi
        private Label _lblVoice;
        private ComboBox _cbStudioVoice;
        private Label _lblIntonation;
        private ComboBox _cbStudioIntonation;

        // Parameter Post-Production (Grading & BGM)
        private Label _lblGrading;
        private ComboBox _cbStudioGrading;
        private RoundedButton _btnPreviewLut;
        private RoundedButton _btnUploadLut;
        private Label _lblBgm;
        private ComboBox _cbStudioBgm;
        private RoundedButton _btnPreviewBgm;
        private RoundedButton _btnUploadBgm;

        // Pengatur Volume Manual
        private Label _lblStudioVoVolume;
        private TrackBar _tbStudioVoVolume;
        private Label _lblStudioBgmVolume;
        private TrackBar _tbStudioBgmVolume;

        // Pengatur Kecepatan Vokal & Auto-Balance
        private Label _lblStudioVoSpeed;
        private TrackBar _tbStudioVoSpeed;
        private Label _lblStudioVoSpeedVal;
        private CheckBox _chkAutoSpeed;

        // Tombol Master Build & CTA Scan Footage
        private RoundedButton _btnStudioBuildVideo;
        private RoundedButton _btnStudioScanFootage;

        // Console Controls (Right Panel)
        private Label _lblConsoleHeader;
        private TextBox _txtStudioLog;

        // Panel In-Place Big Preview
        private Panel _pnlLutBigPreview;
        private PictureBox _pbLutBigPreview;
        private Label _lblLutBigTitle;
        private Label _lblLutBigDesc;
        private RoundedButton _btnLutBigClose;
        private bool _isLutBigPreviewVisible = false;

        private Panel _pnlVideoBigPreview;
        private PictureBox _pbVideoBigPreview;
        private Label _lblVideoBigBadge;
        private RoundedButton _btnVideoCardPlay;
        private RoundedButton _btnVideoLaunchApp;
        private RoundedButton _btnVideoBigClose;
        private bool _isVideoBigPreviewVisible = false;
        private string _lastRenderedVideoPath = null;

        private void InitializeStudioView()
        {
            _viewStudio = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = false
            };
            _pnlViewContainer.Controls.Add(_viewStudio);

            // Left Card & Right Card
            _pnlStudioLeft = UIFactory.CreateCard(0, 0, 520, 720, 12);
            _pnlStudioRight = UIFactory.CreateCard(540, 0, 480, 720, 12);
            _viewStudio.Controls.Add(_pnlStudioLeft);
            _viewStudio.Controls.Add(_pnlStudioRight);

            // ── KIRI: Input, Editor & Parameter Produksi ───────────────────────
            _lblTopicHeader = new Label { Text = "Topik Konten:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = UIFactory.TextPrimary, Location = new Point(14, 12), AutoSize = true };
            _pnlStudioLeft.Controls.Add(_lblTopicHeader);

            _txtStudioTopic = new TextBox
            {
                Location = new Point(14, 32),
                Size = new Size(492, 26),
                Font = new Font("Segoe UI", 9.5f),
                Text = ""
            };
            _pnlStudioLeft.Controls.Add(_txtStudioTopic);

            // 3 Kolom Parameter: Tema Klaster, Target Audiens, Durasi Target
            _lblThemeHeader = new Label { Text = "Tema Klaster:", Font = new Font("Segoe UI", 8.25f, FontStyle.Bold), ForeColor = UIFactory.TextSecondary, Location = new Point(14, 64), AutoSize = true };
            _pnlStudioLeft.Controls.Add(_lblThemeHeader);

            _cbStudioTheme = new ComboBox
            {
                Location = new Point(14, 84),
                Size = new Size(160, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.75f)
            };
            _cbStudioTheme.Items.AddRange(new object[] {
                "[Otomatis] Sesuai Topik (Multi-Klaster)",
                "Stoikisme & Filsafat Kehidupan",
                "Psikologi Gelap & Karakter Manusia",
                "Realitas Keras & Perjuangan Hidup",
                "Senja, Kesendirian & Refleksi Batin",
                "Ketenangan Jiwa & Self Healing",
                "Industri Modern & Mesin Manufaktur",
                "Sejarah Tokoh & Peradaban Besar",
                "Kreatifitas Seni & Inspirasi Craft",
                "Dakwah Hikmah & Renungan Hakiki",
                "Perang & Kepahlawanan Bangsa",
                "Visual Cyberpunk & Masa Depan AI",
                "Jualan Online & Hustle Kewirausahaan",
                "Santai, Relaksasi & Visual Loop",
                "Dongeng, Fabel & Cerita Kisah",
                "Berita & Investigasi Media TV",
                "Studi Kasus Bisnis & Finansial",
                "Dunia Kerja & Karir Modern",
                "Edukasi Sains & Fakta Unik",
                "Hukum, Korupsi & Keadilan Sosial",
                "Pendidikan, Kampus & Dunia Belajar"
            });
            _cbStudioTheme.SelectedIndex = 0;
            _pnlStudioLeft.Controls.Add(_cbStudioTheme);

            _lblAudienceHeader = new Label { Text = "Target Audiens:", Font = new Font("Segoe UI", 8.25f, FontStyle.Bold), ForeColor = UIFactory.TextSecondary, Location = new Point(180, 64), AutoSize = true };
            _pnlStudioLeft.Controls.Add(_lblAudienceHeader);

            _cbStudioAudience = new ComboBox
            {
                Location = new Point(180, 84),
                Size = new Size(160, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.75f)
            };
            _cbStudioAudience.Items.AddRange(new object[] {
                "[Otomatis] Universal (Semua)",
                "Karyawan Swasta & Buruh Pabrik",
                "Pegawai Negeri & Instansi Pemerintah",
                "Pejabat, Pemerintah & Pemegang Kuasa",
                "Koruptor & Pelaku Kecurangan",
                "Pekerja Keras & Pejuang Nafkah",
                "Pengangguran & Pencari Kerja",
                "Pedagang, UMKM & Pemilik Warung",
                "Pebisnis, Startup & Pengusaha",
                "Praktisi & Profesional Ahli",
                "Mahasiswa & Fresh Graduate",
                "Pelajar SMA/SMK & Anak Muda",
                "Freelancer & Kreator Mandiri",
                "Orang Tua & Ibu Rumah Tangga",
                "Pencari Arah (Quarter-Life)",
                "Penonton Berita & Media TV",
                "Pendengar Cerita & Dongeng"
            });
            _cbStudioAudience.SelectedIndex = 0;
            _pnlStudioLeft.Controls.Add(_cbStudioAudience);

            _lblDurationHeader = new Label { Text = "Durasi Target:", Font = new Font("Segoe UI", 8.25f, FontStyle.Bold), ForeColor = UIFactory.TextSecondary, Location = new Point(346, 64), AutoSize = true };
            _pnlStudioLeft.Controls.Add(_lblDurationHeader);

            _cbStudioDuration = new ComboBox
            {
                Location = new Point(346, 84),
                Size = new Size(160, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.75f)
            };
            _cbStudioDuration.Items.AddRange(new object[] {
                "1 Menit (~12-14 Adegan / ~160 Kata)",
                "2 Menit (~18-20 Adegan / ~260 Kata)",
                "3 Menit (~24-26 Adegan / ~380 Kata)",
                "5 Menit (~38-42 Adegan / ~600 Kata)",
                "10 Menit (~75-85 Adegan / ~1.300 Kata)",
                "20 Menit (~145-160 Adegan / ~2.600 Kata)",
                "30 Menit (~215-235 Adegan / ~3.900 Kata)"
            });
            _cbStudioDuration.SelectedIndex = 2; // Default: 3 Menit
            _cbStudioDuration.SelectedIndexChanged += delegate(object s, EventArgs e) { UpdateSentenceCounter(); };
            _pnlStudioLeft.Controls.Add(_cbStudioDuration);

            // Baris Tombol Pembuat Naskah di Y=118
            _btnStudioGenAi = UIFactory.CreateButton("Buat Naskah (AI)", 14, 118, 240, 30, UIFactory.AccentPrimary, 6);
            _btnStudioGenAi.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _btnStudioGenAi.Click += async delegate(object s, EventArgs e) { await GenerateScriptFromGeminiAsync(); };
            _pnlStudioLeft.Controls.Add(_btnStudioGenAi);

            _btnStudioCheckAiQuota = UIFactory.CreateButton("Cek Model dan Kuota", 266, 118, 240, 30, UIFactory.AccentPrimaryLight, 6);
            _btnStudioCheckAiQuota.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            _btnStudioCheckAiQuota.Click += async delegate(object s, EventArgs e) { await CheckElevenLabsQuotaAsync(); };
            _pnlStudioLeft.Controls.Add(_btnStudioCheckAiQuota);

            // Baris 2: Poles Prosody / Upload Vokal / Dengar Vokal di Y=154
            _btnStudioPolishScript = UIFactory.CreateButton("Poles Prosody Naskah", 14, 154, 240, 30, UIFactory.AccentPrimaryLight, 6);
            _btnStudioPolishScript.Font = new Font("Segoe UI", 8.25f, FontStyle.Bold);
            _btnStudioPolishScript.Click += delegate(object s, EventArgs e)
            {
                string currentScript = _txtStudioScript.Text.Trim();
                if (!string.IsNullOrEmpty(currentScript))
                {
                    SplitAndFormatScriptScenes();
                }
                else
                {
                    PolishScriptProsody();
                }
            };
            _pnlStudioLeft.Controls.Add(_btnStudioPolishScript);

            _btnStudioUploadVoice = UIFactory.CreateButton("+ Upload Vokal", 266, 154, 154, 30, UIFactory.AccentPrimaryLight, 6);
            _btnStudioUploadVoice.Font = new Font("Segoe UI", 8.25f, FontStyle.Bold);
            _btnStudioUploadVoice.Click += delegate(object s, EventArgs e) { SelectAndUploadCustomVoice(); };
            _pnlStudioLeft.Controls.Add(_btnStudioUploadVoice);

            _btnStudioClearVoice = UIFactory.CreateButton("X", 420, 154, 28, 30, UIFactory.AccentRed, 6);
            _btnStudioClearVoice.Font = new Font("Segoe UI", 8.25f, FontStyle.Bold);
            _btnStudioClearVoice.NormalText = Color.White;
            _btnStudioClearVoice.Visible = false;
            _btnStudioClearVoice.Click += delegate(object s, EventArgs e) { ResetUploadedCustomVoice(); };
            _pnlStudioLeft.Controls.Add(_btnStudioClearVoice);

            _btnStudioPreviewVoice = UIFactory.CreateButton("Dengar", 452, 154, 80, 30, UIFactory.AccentPrimaryLight, 6);
            _btnStudioPreviewVoice.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            _btnStudioPreviewVoice.Click += delegate(object s, EventArgs e) { ToggleVoPlayback(); };
            _pnlStudioLeft.Controls.Add(_btnStudioPreviewVoice);

            // Baris Pilihan Koleksi Footage Karakter (Y=188 & Y=208)
            _lblFootageHeader = new Label
            {
                Text = "Karakter / Vibe Footage:",
                Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
                ForeColor = UIFactory.TextSecondary,
                Location = new Point(14, 188),
                AutoSize = true
            };
            _pnlStudioLeft.Controls.Add(_lblFootageHeader);

            _flpFootageSkus = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false,
                Location = new Point(14, 208),
                Size = new Size(492, 28),
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            Font chkFootageFont = new Font("Segoe UI", 8.25f, FontStyle.Bold);
            Padding chkFootageMargin = new Padding(0, 3, 6, 0);

            _chkFootageAuto = new CheckBox
            {
                Text = "Otomatis",
                Font = chkFootageFont,
                ForeColor = UIFactory.TextPrimary,
                AutoSize = true,
                Checked = true,
                Margin = chkFootageMargin,
                Cursor = Cursors.Hand
            };

            _chkFootageShelby = new CheckBox
            {
                Text = "Shelby (ID9919)",
                Font = chkFootageFont,
                ForeColor = UIFactory.TextPrimary,
                AutoSize = true,
                Checked = false,
                Margin = chkFootageMargin,
                Cursor = Cursors.Hand
            };

            _chkFootageJoker = new CheckBox
            {
                Text = "Joker (IDB7CA)",
                Font = chkFootageFont,
                ForeColor = UIFactory.TextPrimary,
                AutoSize = true,
                Checked = false,
                Margin = chkFootageMargin,
                Cursor = Cursors.Hand
            };

            _chkFootageSenja = new CheckBox
            {
                Text = "Senja Galau (IDFCC3)",
                Font = chkFootageFont,
                ForeColor = UIFactory.TextPrimary,
                AutoSize = true,
                Checked = false,
                Margin = chkFootageMargin,
                Cursor = Cursors.Hand
            };

            _chkFootageDark = new CheckBox
            {
                Text = "Dark Psych (IDFFA0)",
                Font = chkFootageFont,
                ForeColor = UIFactory.TextPrimary,
                AutoSize = true,
                Checked = false,
                Margin = chkFootageMargin,
                Cursor = Cursors.Hand
            };

            _chkFootageAnime = new CheckBox
            {
                Text = "Anime Galau (ID7792)",
                Font = chkFootageFont,
                ForeColor = UIFactory.TextPrimary,
                AutoSize = true,
                Checked = false,
                Margin = chkFootageMargin,
                Cursor = Cursors.Hand
            };

            _chkFootageHealing = new CheckBox
            {
                Text = "Alam Healing (ID7667)",
                Font = chkFootageFont,
                ForeColor = UIFactory.TextPrimary,
                AutoSize = true,
                Checked = false,
                Margin = chkFootageMargin,
                Cursor = Cursors.Hand
            };

            _chkFootageHardWork = new CheckBox
            {
                Text = "Kerja Keras (IDE5EF/557A)",
                Font = chkFootageFont,
                ForeColor = UIFactory.TextPrimary,
                AutoSize = true,
                Checked = false,
                Margin = chkFootageMargin,
                Cursor = Cursors.Hand
            };

            // Event Saling Kunci (Mutual Exclusive dengan Otomatis)
            _chkFootageAuto.CheckedChanged += delegate(object s, EventArgs e)
            {
                if (_isUpdatingFootageCheckboxes) return;
                _isUpdatingFootageCheckboxes = true;
                try
                {
                    if (_chkFootageAuto.Checked)
                    {
                        if (_chkFootageShelby != null) _chkFootageShelby.Checked = false;
                        if (_chkFootageJoker != null) _chkFootageJoker.Checked = false;
                        if (_chkFootageSenja != null) _chkFootageSenja.Checked = false;
                        if (_chkFootageDark != null) _chkFootageDark.Checked = false;
                        if (_chkFootageAnime != null) _chkFootageAnime.Checked = false;
                        if (_chkFootageHealing != null) _chkFootageHealing.Checked = false;
                        if (_chkFootageHardWork != null) _chkFootageHardWork.Checked = false;
                    }
                    else
                    {
                        bool anySpecificChecked = (_chkFootageShelby != null && _chkFootageShelby.Checked) ||
                                                  (_chkFootageJoker != null && _chkFootageJoker.Checked) ||
                                                  (_chkFootageSenja != null && _chkFootageSenja.Checked) ||
                                                  (_chkFootageDark != null && _chkFootageDark.Checked) ||
                                                  (_chkFootageAnime != null && _chkFootageAnime.Checked) ||
                                                  (_chkFootageHealing != null && _chkFootageHealing.Checked) ||
                                                  (_chkFootageHardWork != null && _chkFootageHardWork.Checked);
                        if (!anySpecificChecked)
                        {
                            _chkFootageAuto.Checked = true;
                        }
                    }
                }
                finally
                {
                    _isUpdatingFootageCheckboxes = false;
                }
            };

            EventHandler onSpecificFootageChanged = delegate(object s, EventArgs e)
            {
                if (_isUpdatingFootageCheckboxes) return;
                _isUpdatingFootageCheckboxes = true;
                try
                {
                    bool anySpecificChecked = (_chkFootageShelby != null && _chkFootageShelby.Checked) ||
                                              (_chkFootageJoker != null && _chkFootageJoker.Checked) ||
                                              (_chkFootageSenja != null && _chkFootageSenja.Checked) ||
                                              (_chkFootageDark != null && _chkFootageDark.Checked) ||
                                              (_chkFootageAnime != null && _chkFootageAnime.Checked) ||
                                              (_chkFootageHealing != null && _chkFootageHealing.Checked) ||
                                              (_chkFootageHardWork != null && _chkFootageHardWork.Checked);

                    if (anySpecificChecked)
                    {
                        if (_chkFootageAuto != null) _chkFootageAuto.Checked = false;
                    }
                    else
                    {
                        if (_chkFootageAuto != null) _chkFootageAuto.Checked = true;
                    }
                }
                finally
                {
                    _isUpdatingFootageCheckboxes = false;
                }
            };

            _chkFootageShelby.CheckedChanged += onSpecificFootageChanged;
            _chkFootageJoker.CheckedChanged += onSpecificFootageChanged;
            _chkFootageSenja.CheckedChanged += onSpecificFootageChanged;
            _chkFootageDark.CheckedChanged += onSpecificFootageChanged;
            _chkFootageAnime.CheckedChanged += onSpecificFootageChanged;
            _chkFootageHealing.CheckedChanged += onSpecificFootageChanged;
            _chkFootageHardWork.CheckedChanged += onSpecificFootageChanged;

            _flpFootageSkus.Controls.Add(_chkFootageAuto);
            _flpFootageSkus.Controls.Add(_chkFootageShelby);
            _flpFootageSkus.Controls.Add(_chkFootageJoker);
            _flpFootageSkus.Controls.Add(_chkFootageSenja);
            _flpFootageSkus.Controls.Add(_chkFootageDark);
            _flpFootageSkus.Controls.Add(_chkFootageAnime);
            _flpFootageSkus.Controls.Add(_chkFootageHealing);
            _flpFootageSkus.Controls.Add(_chkFootageHardWork);
            _pnlStudioLeft.Controls.Add(_flpFootageSkus);

            // Label Indikator Jumlah Kalimat di Y=240
            _lblSentenceCount = new Label
            {
                Text = "Jumlah Adegan: 0 | Target: 22-28 Adegan (~3 Menit)",
                Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
                ForeColor = UIFactory.AccentAmber,
                Location = new Point(14, 240),
                AutoSize = true
            };
            _pnlStudioLeft.Controls.Add(_lblSentenceCount);

            // Kotak Teks Naskah (Y=260)
            _txtStudioScript = new TextBox
            {
                Location = new Point(14, 260),
                Size = new Size(492, 240),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle,
                Text = ""
            };
            _txtStudioScript.TextChanged += delegate(object s, EventArgs e) { UpdateSentenceCounter(); };

            ContextMenuStrip scriptMenu = new ContextMenuStrip();
            scriptMenu.Items.Add("Poles Prosody (Auto-Jeda & Ejaan)", null, delegate(object s, EventArgs e) { PolishScriptProsody(); });
            scriptMenu.Items.Add("Rapikan Naskah (1 Adegan Per Baris)", null, delegate(object s, EventArgs e) { FormatScript(); });
            scriptMenu.Items.Add(new ToolStripSeparator());
            scriptMenu.Items.Add("Tempel dari Clipboard", null, delegate(object s, EventArgs e) { PasteFromClipboard(); });
            scriptMenu.Items.Add("Muat Contoh Naskah 19 Baris", null, delegate(object s, EventArgs e) { LoadSampleScript(); });
            _txtStudioScript.ContextMenuStrip = scriptMenu;
            _pnlStudioLeft.Controls.Add(_txtStudioScript);

            // Parameter Produksi Vokal TTS & Intonasi
            _lblVoice = new Label { Text = "Karakter Vokal (TTS):", Font = new Font("Segoe UI", 8.25f, FontStyle.Bold), ForeColor = UIFactory.TextSecondary, Location = new Point(14, 510), AutoSize = true };
            _pnlStudioLeft.Controls.Add(_lblVoice);

            _cbStudioVoice = new ComboBox
            {
                Location = new Point(14, 532),
                Size = new Size(240, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            PopulateVoiceList();
            _cbStudioVoice.SelectedIndexChanged += delegate(object s, EventArgs e)
            {
                if (_cbStudioVoice.SelectedIndex > 0 && !string.IsNullOrEmpty(_uploadedVoicePath))
                {
                    ResetUploadedCustomVoice();
                    AppendStudioLog("  • Karakter vokal TTS dipilih dari dropdown. Vokal eksternal otomatis dilepas.");
                }
            };
            _pnlStudioLeft.Controls.Add(_cbStudioVoice);

            _lblIntonation = new Label { Text = "Gaya Intonasi:", Font = new Font("Segoe UI", 8.25f, FontStyle.Bold), ForeColor = UIFactory.TextSecondary, Location = new Point(266, 510), AutoSize = true };
            _pnlStudioLeft.Controls.Add(_lblIntonation);

            _cbStudioIntonation = new ComboBox
            {
                Location = new Point(266, 532),
                Size = new Size(240, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            PopulateIntonationList();
            _pnlStudioLeft.Controls.Add(_cbStudioIntonation);

            // Tombol Utama Perakitan Video
            _btnStudioBuildVideo = UIFactory.CreateButton("MULAI PERAKITAN VIDEO", 14, 574, 492, 48, UIFactory.AccentPrimary, 8);
            _btnStudioBuildVideo.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            _btnStudioBuildVideo.Click += async delegate(object s, EventArgs e) { await ExecuteVideoPipelineAsync(); };
            _pnlStudioLeft.Controls.Add(_btnStudioBuildVideo);

            _btnStudioTranscribeVoice = UIFactory.CreateButton("Transkrip & Split", 14, 574, 180, 48, Color.FromArgb(39, 39, 42), 8);
            _btnStudioTranscribeVoice.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _btnStudioTranscribeVoice.NormalText = Color.White;
            _btnStudioTranscribeVoice.Visible = false;
            _btnStudioTranscribeVoice.Click += delegate(object s, EventArgs e) { SplitAndFormatScriptScenes(); };
            _pnlStudioLeft.Controls.Add(_btnStudioTranscribeVoice);

            // ── KANAN: POST-PRODUCTION & LOG PRODUKSI ──────────────────────────
            // Row 1: Color Grading (3D LUT)
            _lblGrading = new Label { Text = "Color Grading (3D LUT):", Font = new Font("Segoe UI", 8.25f, FontStyle.Bold), ForeColor = UIFactory.TextSecondary, Location = new Point(14, 10), AutoSize = true };
            _pnlStudioRight.Controls.Add(_lblGrading);

            _cbStudioGrading = new ComboBox
            {
                Location = new Point(14, 30),
                Size = new Size(264, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            PopulateGradingList();
            _cbStudioGrading.SelectedIndexChanged += delegate(object s, EventArgs e)
            {
                UpdateLutPreview();
                SetLutBigPreviewVisible(true);
            };
            _pnlStudioRight.Controls.Add(_cbStudioGrading);

            _btnPreviewLut = UIFactory.CreateButton("Pratinjau", 288, 29, 90, 28, UIFactory.AccentPrimaryLight, 6);
            _btnPreviewLut.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            _btnPreviewLut.Click += delegate(object s, EventArgs e) { ToggleLutBigPreview(); };
            _pnlStudioRight.Controls.Add(_btnPreviewLut);

            _btnUploadLut = UIFactory.CreateButton("Upload LUT", 386, 29, 100, 28, UIFactory.AccentPrimary, 6);
            _btnUploadLut.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            _btnUploadLut.Click += delegate(object s, EventArgs e) { UploadLut(); };
            _pnlStudioRight.Controls.Add(_btnUploadLut);

            // Row 2: Musik Latar (BGM)
            _lblBgm = new Label { Text = "Musik Latar (BGM):", Font = new Font("Segoe UI", 8.25f, FontStyle.Bold), ForeColor = UIFactory.TextSecondary, Location = new Point(14, 64), AutoSize = true };
            _pnlStudioRight.Controls.Add(_lblBgm);

            _cbStudioBgm = new ComboBox
            {
                Location = new Point(14, 84),
                Size = new Size(264, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            PopulateBgmList();
            _pnlStudioRight.Controls.Add(_cbStudioBgm);

            _btnPreviewBgm = UIFactory.CreateButton("Dengar Audio", 288, 83, 90, 28, UIFactory.AccentPrimaryLight, 6);
            _btnPreviewBgm.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            _btnPreviewBgm.Click += delegate(object s, EventArgs e) { ToggleBgmPlayback(); };
            _pnlStudioRight.Controls.Add(_btnPreviewBgm);

            _btnUploadBgm = UIFactory.CreateButton("+ Upload BGM", 386, 83, 100, 28, UIFactory.AccentPrimary, 6);
            _btnUploadBgm.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            _btnUploadBgm.Click += delegate(object s, EventArgs e) { UploadBgm(); };
            _pnlStudioRight.Controls.Add(_btnUploadBgm);

            // Row 2b: Pengatur Volume Vokal & BGM
            _lblStudioVoVolume = new Label { Text = "Volume Vokal: 100%", Font = new Font("Segoe UI", 8.25f, FontStyle.Bold), ForeColor = UIFactory.TextSecondary, Location = new Point(14, 118), AutoSize = true };
            _pnlStudioRight.Controls.Add(_lblStudioVoVolume);

            _tbStudioVoVolume = CreateStudioVolumeTrackBar(14, 136, 220);
            _tbStudioVoVolume.ValueChanged += delegate(object s, EventArgs e) { UpdateStudioVolumeLabels(); };
            _pnlStudioRight.Controls.Add(_tbStudioVoVolume);

            _lblStudioBgmVolume = new Label { Text = "Volume BGM: 100%", Font = new Font("Segoe UI", 8.25f, FontStyle.Bold), ForeColor = UIFactory.TextSecondary, Location = new Point(250, 118), AutoSize = true };
            _pnlStudioRight.Controls.Add(_lblStudioBgmVolume);

            _tbStudioBgmVolume = CreateStudioVolumeTrackBar(250, 136, 220);
            _tbStudioBgmVolume.ValueChanged += delegate(object s, EventArgs e) { UpdateStudioVolumeLabels(); };
            _pnlStudioRight.Controls.Add(_tbStudioBgmVolume);

            // Row 2c: Kecepatan Vokal & Auto-Balance
            _lblStudioVoSpeed = new Label { Text = "Kecepatan Vokal:", Font = new Font("Segoe UI", 8.25f, FontStyle.Bold), ForeColor = UIFactory.TextSecondary, Location = new Point(14, 174), AutoSize = true };
            _pnlStudioRight.Controls.Add(_lblStudioVoSpeed);

            _lblStudioVoSpeedVal = new Label { Text = "1.00x", Font = new Font("Consolas", 8.5f, FontStyle.Bold), ForeColor = UIFactory.AccentGreen, Location = new Point(136, 174), AutoSize = true };
            _pnlStudioRight.Controls.Add(_lblStudioVoSpeedVal);

            _tbStudioVoSpeed = new TrackBar
            {
                Location = new Point(14, 192),
                Size = new Size(220, 30),
                Minimum = 75,
                Maximum = 125,
                Value = 100,
                TickFrequency = 5,
                SmallChange = 1,
                LargeChange = 5,
                AutoSize = false
            };
            _tbStudioVoSpeed.ValueChanged += delegate(object s, EventArgs e)
            {
                if (_chkAutoSpeed != null && _chkAutoSpeed.Checked) _chkAutoSpeed.Checked = false;
                UpdateSpeedLabel();
            };
            _pnlStudioRight.Controls.Add(_tbStudioVoSpeed);

            _chkAutoSpeed = new CheckBox
            {
                Text = "Auto Seimbang",
                Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
                ForeColor = UIFactory.TextPrimary,
                Location = new Point(250, 196),
                AutoSize = true,
                Checked = false
            };
            _chkAutoSpeed.CheckedChanged += delegate(object s, EventArgs e)
            {
                if (_chkAutoSpeed.Checked) ApplyAutoSpeedBalance();
                else UpdateSpeedLabel();
            };
            _pnlStudioRight.Controls.Add(_chkAutoSpeed);

            // Row 3: Console Header & Scan Footage
            _lblConsoleHeader = new Label { Text = "Log Produksi & Telemetri:", Font = new Font("Segoe UI", 8.75f, FontStyle.Bold), ForeColor = UIFactory.TextPrimary, Location = new Point(14, 230), AutoSize = true };
            _pnlStudioRight.Controls.Add(_lblConsoleHeader);

            _btnStudioScanFootage = UIFactory.CreateButton("Scan Footage", 250, 226, 220, 28, UIFactory.AccentTeal, 6);
            _btnStudioScanFootage.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _btnStudioScanFootage.NormalText = Color.White;
            _btnStudioScanFootage.Click += async delegate(object s, EventArgs e) { await ScanAndCheckFootageAsync(); };
            _pnlStudioRight.Controls.Add(_btnStudioScanFootage);

            // Row 4: Log TextBox
            _txtStudioLog = new TextBox
            {
                Location = new Point(14, 260),
                Size = new Size(456, 420),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 8.5f),
                BackColor = Color.FromArgb(250, 250, 252),
                ForeColor = Color.FromArgb(24, 24, 27),
                BorderStyle = BorderStyle.FixedSingle
            };
            _pnlStudioRight.Controls.Add(_txtStudioLog);

            // Create Big Previews
            CreateLutBigPreviewComponents();
            _pnlStudioRight.Controls.Add(_pnlLutBigPreview);

            CreateVideoBigPreviewComponents();
            _pnlStudioRight.Controls.Add(_pnlVideoBigPreview);

            // Hook Resize Event
            _viewStudio.Resize += delegate(object s, EventArgs e) { LayoutStudioComponents(); };

            // Inisialisasi awal
            LayoutStudioComponents();
        }

        public string GetSelectedFootageSkus()
        {
            if (_chkFootageAuto != null && _chkFootageAuto.Checked) return "auto";
            List<string> skus = new List<string>();
            if (_chkFootageShelby != null && _chkFootageShelby.Checked) skus.Add("ID9919");
            if (_chkFootageJoker != null && _chkFootageJoker.Checked) skus.Add("IDB7CA");
            if (_chkFootageSenja != null && _chkFootageSenja.Checked) skus.Add("IDFCC3");
            if (_chkFootageDark != null && _chkFootageDark.Checked) skus.Add("IDFFA0");
            if (_chkFootageAnime != null && _chkFootageAnime.Checked) skus.Add("ID7792");
            if (_chkFootageHealing != null && _chkFootageHealing.Checked) skus.Add("ID7667");
            if (_chkFootageHardWork != null && _chkFootageHardWork.Checked) { skus.Add("IDE5EF"); skus.Add("ID557A"); }
            return skus.Count > 0 ? string.Join(",", skus.ToArray()) : "auto";
        }

        private void LayoutStudioComponents()
        {
            if (_viewStudio == null || _viewStudio.IsDisposed) return;
            int clientW = _viewStudio.ClientSize.Width;
            int clientH = _viewStudio.ClientSize.Height;
            if (clientW < 600) return;

            int gap = 16;
            int rightW = Math.Max(380, Math.Min(440, (int)(clientW * 0.38)));
            int leftW = clientW - rightW - gap;
            int panelH = Math.Max(500, clientH - 4);

            if (_pnlStudioLeft != null)
            {
                _pnlStudioLeft.SetBounds(0, 0, leftW, panelH);
                LayoutLeftPanelControls(leftW, panelH);
            }

            if (_pnlStudioRight != null)
            {
                _pnlStudioRight.SetBounds(leftW + gap, 0, rightW, panelH);
                LayoutRightPanelControls(rightW, panelH);
            }
        }

        private void LayoutLeftPanelControls(int pnlW, int pnlH)
        {
            int contentW = pnlW - 28;
            if (_txtStudioTopic != null) _txtStudioTopic.Width = contentW;

            int halfTopW = (contentW - 12) / 2;
            int col2TopX = 14 + halfTopW + 12;

            int thirdW = (contentW - 24) / 3;
            int colAudX = 14 + thirdW + 12;
            int colDurX = colAudX + thirdW + 12;

            if (_lblThemeHeader != null) _lblThemeHeader.SetBounds(14, 64, thirdW, 18);
            if (_cbStudioTheme != null) _cbStudioTheme.SetBounds(14, 84, thirdW, 26);
            if (_lblAudienceHeader != null) _lblAudienceHeader.SetBounds(colAudX, 64, thirdW, 18);
            if (_cbStudioAudience != null) _cbStudioAudience.SetBounds(colAudX, 84, thirdW, 26);
            if (_lblDurationHeader != null) _lblDurationHeader.SetBounds(colDurX, 64, thirdW, 18);
            if (_cbStudioDuration != null) _cbStudioDuration.SetBounds(colDurX, 84, thirdW, 26);

            if (_btnStudioGenAi != null && _btnStudioCheckAiQuota != null)
            {
                _btnStudioGenAi.SetBounds(14, 118, halfTopW, 30);
                _btnStudioCheckAiQuota.SetBounds(col2TopX, 118, halfTopW, 30);
            }

            if (_btnStudioPolishScript != null && _btnStudioUploadVoice != null)
            {
                bool hasUploadedVoice = !string.IsNullOrEmpty(_uploadedVoicePath) && File.Exists(_uploadedVoicePath);
                if (hasUploadedVoice)
                {
                    _btnStudioPolishScript.Text = "Split Naskah Audio";
                    int previewVoW = 74;
                    int clearVoW = 28;
                    int uploadVoW = Math.Max(80, halfTopW - previewVoW - clearVoW - 10);
                    int clearVoX = col2TopX + uploadVoW + 4;
                    int previewVoX = clearVoX + clearVoW + 4;

                    _btnStudioUploadVoice.SetBounds(col2TopX, 154, uploadVoW, 30);
                    if (_btnStudioClearVoice != null)
                    {
                        _btnStudioClearVoice.SetBounds(clearVoX, 154, clearVoW, 30);
                        _btnStudioClearVoice.Visible = true;
                    }
                    if (_btnStudioPreviewVoice != null)
                    {
                        _btnStudioPreviewVoice.SetBounds(previewVoX, 154, previewVoW, 30);
                    }
                }
                else
                {
                    _btnStudioPolishScript.Text = "Poles Prosody Naskah";
                    if (_btnStudioClearVoice != null) _btnStudioClearVoice.Visible = false;

                    int previewVoW = 80;
                    int uploadVoW = Math.Max(100, halfTopW - previewVoW - 6);
                    int previewVoX = col2TopX + uploadVoW + 6;
                    _btnStudioUploadVoice.SetBounds(col2TopX, 154, uploadVoW, 30);
                    if (_btnStudioPreviewVoice != null)
                    {
                        _btnStudioPreviewVoice.SetBounds(previewVoX, 154, previewVoW, 30);
                    }
                }

                _btnStudioPolishScript.SetBounds(14, 154, halfTopW, 30);
            }

            if (_lblFootageHeader != null) _lblFootageHeader.Location = new Point(14, 188);
            if (_flpFootageSkus != null) _flpFootageSkus.SetBounds(14, 208, contentW, 28);
            if (_lblSentenceCount != null) _lblSentenceCount.Location = new Point(14, 240);

            int btnBuildY = pnlH - 14 - 48;
            bool isExternalVoiceActive = !string.IsNullOrEmpty(_uploadedVoicePath) && File.Exists(_uploadedVoicePath);

            if (isExternalVoiceActive)
            {
                int gapBtn = 8;
                int splitBuildW = (int)((contentW - gapBtn) * 0.58);
                int splitTranscribeW = contentW - gapBtn - splitBuildW;
                int splitTranscribeX = 14 + splitBuildW + gapBtn;

                if (_btnStudioBuildVideo != null)
                {
                    _btnStudioBuildVideo.Text = "RAKIT DARI VOKAL EKSTERNAL";
                    _btnStudioBuildVideo.NormalBg = Color.FromArgb(20, 83, 45);
                    _btnStudioBuildVideo.SetBounds(14, btnBuildY, splitBuildW, 48);
                }

                if (_btnStudioTranscribeVoice != null)
                {
                    _btnStudioTranscribeVoice.Visible = true;
                    _btnStudioTranscribeVoice.SetBounds(splitTranscribeX, btnBuildY, splitTranscribeW, 48);
                }
            }
            else
            {
                if (_btnStudioBuildVideo != null)
                {
                    _btnStudioBuildVideo.Text = "MULAI PERAKITAN VIDEO";
                    _btnStudioBuildVideo.NormalBg = UIFactory.AccentPrimary;
                    _btnStudioBuildVideo.SetBounds(14, btnBuildY, contentW, 48);
                }

                if (_btnStudioTranscribeVoice != null)
                {
                    _btnStudioTranscribeVoice.Visible = false;
                }
            }

            int cbVoiceY = btnBuildY - 12 - 26;
            int voiceHeaderY = cbVoiceY - 6 - 18;
            int halfW = (contentW - 12) / 2;
            int col2X = 14 + halfW + 12;

            if (_lblVoice != null) _lblVoice.SetBounds(14, voiceHeaderY, halfW, 18);
            if (_cbStudioVoice != null) _cbStudioVoice.SetBounds(14, cbVoiceY, halfW, 26);
            if (_lblIntonation != null) _lblIntonation.SetBounds(col2X, voiceHeaderY, halfW, 18);
            if (_cbStudioIntonation != null) _cbStudioIntonation.SetBounds(col2X, cbVoiceY, halfW, 26);

            int scriptTop = 260;
            int scriptH = voiceHeaderY - scriptTop - 12;
            if (_txtStudioScript != null)
            {
                _txtStudioScript.SetBounds(14, scriptTop, contentW, Math.Max(120, scriptH));
            }
        }

        private void LayoutRightPanelControls(int pnlW, int pnlH)
        {
            int contentW = pnlW - 28;
            int uploadBtnW = 95;
            int previewBtnW = 85;
            int btnGap = 6;
            int totalBtnW = uploadBtnW + previewBtnW + btnGap;
            int comboW = Math.Max(120, contentW - totalBtnW - btnGap);
            int previewBtnX = 14 + comboW + btnGap;
            int uploadBtnX = previewBtnX + previewBtnW + btnGap;

            if (_lblGrading != null) _lblGrading.Location = new Point(14, 10);
            if (_cbStudioGrading != null) _cbStudioGrading.SetBounds(14, 30, comboW, 26);
            if (_btnPreviewLut != null) _btnPreviewLut.SetBounds(previewBtnX, 29, previewBtnW, 28);
            if (_btnUploadLut != null) _btnUploadLut.SetBounds(uploadBtnX, 29, uploadBtnW, 28);

            if (_lblBgm != null) _lblBgm.Location = new Point(14, 64);
            if (_cbStudioBgm != null) _cbStudioBgm.SetBounds(14, 84, comboW, 26);
            if (_btnPreviewBgm != null) _btnPreviewBgm.SetBounds(previewBtnX, 83, previewBtnW, 28);
            if (_btnUploadBgm != null) _btnUploadBgm.SetBounds(uploadBtnX, 83, uploadBtnW, 28);

            int halfVolW = (contentW - 12) / 2;
            int col2VolX = 14 + halfVolW + 12;

            if (_lblStudioVoVolume != null) _lblStudioVoVolume.Location = new Point(14, 118);
            if (_tbStudioVoVolume != null) _tbStudioVoVolume.SetBounds(14, 136, halfVolW, 30);
            if (_lblStudioBgmVolume != null) _lblStudioBgmVolume.Location = new Point(col2VolX, 118);
            if (_tbStudioBgmVolume != null) _tbStudioBgmVolume.SetBounds(col2VolX, 136, halfVolW, 30);

            if (_lblStudioVoSpeed != null) _lblStudioVoSpeed.Location = new Point(14, 174);
            if (_lblStudioVoSpeedVal != null) _lblStudioVoSpeedVal.Location = new Point(136, 174);
            if (_tbStudioVoSpeed != null) _tbStudioVoSpeed.SetBounds(14, 192, halfVolW, 30);
            if (_chkAutoSpeed != null) _chkAutoSpeed.Location = new Point(col2VolX, 196);

            int row3Y = 228;
            if (_lblConsoleHeader != null) _lblConsoleHeader.Location = new Point(14, row3Y + 4);
            if (_btnStudioScanFootage != null) _btnStudioScanFootage.SetBounds(col2VolX, row3Y, halfVolW, 28);

            int consoleY = 262;
            int consoleH = Math.Max(180, pnlH - consoleY - 14);
            if (_txtStudioLog != null)
            {
                _txtStudioLog.SetBounds(14, consoleY, contentW, consoleH);
            }

            if (_pnlLutBigPreview != null)
            {
                _pnlLutBigPreview.SetBounds(14, consoleY, contentW, consoleH);
                if (_pbLutBigPreview != null)
                {
                    _pbLutBigPreview.SetBounds(14, 40, contentW - 28, Math.Max(80, consoleH - 50));
                }
            }

            if (_pnlVideoBigPreview != null)
            {
                _pnlVideoBigPreview.SetBounds(14, consoleY, contentW, consoleH);
                if (_pbVideoBigPreview != null)
                {
                    _pbVideoBigPreview.SetBounds(14, 40, contentW - 28, Math.Max(80, consoleH - 50));
                }
            }
        }

        private TrackBar CreateStudioVolumeTrackBar(int x, int y, int w)
        {
            TrackBar tb = new TrackBar
            {
                Minimum = 0,
                Maximum = 200,
                Value = 100,
                TickFrequency = 25,
                SmallChange = 5,
                LargeChange = 10,
                TickStyle = TickStyle.BottomRight,
                AutoSize = false,
                Location = new Point(x, y),
                Size = new Size(w, 30)
            };
            return tb;
        }

        private void UpdateStudioVolumeLabels()
        {
            if (_lblStudioVoVolume != null && _tbStudioVoVolume != null)
                _lblStudioVoVolume.Text = string.Format("Volume Vokal: {0}%", _tbStudioVoVolume.Value);
            if (_lblStudioBgmVolume != null && _tbStudioBgmVolume != null)
                _lblStudioBgmVolume.Text = string.Format("Volume BGM: {0}%", _tbStudioBgmVolume.Value);
        }

        private void UpdateSpeedLabel()
        {
            if (_tbStudioVoSpeed == null || _lblStudioVoSpeedVal == null) return;
            double speedRatio = _tbStudioVoSpeed.Value / 100.0;
            _lblStudioVoSpeedVal.Text = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00}x", speedRatio);
            _lblStudioVoSpeedVal.ForeColor = (speedRatio >= 0.90 && speedRatio <= 1.10)
                ? UIFactory.AccentGreen
                : UIFactory.AccentAmber;
        }

        private void ApplyAutoSpeedBalance()
        {
            if (_txtStudioScript == null || _tbStudioVoSpeed == null) return;
            string text = _txtStudioScript.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            int wordCount = text.Split(new char[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount == 0) return;

            const double BASELINE_WPS = 2.50;
            double targetSeconds = 180.0;
            int durIdx = _cbStudioDuration != null ? _cbStudioDuration.SelectedIndex : 2;
            if      (durIdx == 0) targetSeconds = 60.0;
            else if (durIdx == 1) targetSeconds = 120.0;
            else if (durIdx == 2) targetSeconds = 180.0;
            else if (durIdx == 3) targetSeconds = 300.0;
            else if (durIdx == 4) targetSeconds = 600.0;
            else if (durIdx == 5) targetSeconds = 1200.0;
            else if (durIdx == 6) targetSeconds = 1800.0;

            double estimasiDetik = wordCount / BASELINE_WPS;
            double speedRatio = targetSeconds / estimasiDetik;
            speedRatio = Math.Max(0.78, Math.Min(1.20, speedRatio));
            int sliderVal = (int)Math.Round(speedRatio * 100.0);
            sliderVal = Math.Max(75, Math.Min(125, sliderVal));

            _tbStudioVoSpeed.Value = sliderVal;
            UpdateSpeedLabel();

            AppendStudioLog(string.Format("[AUTO-SPEED] {0} kata => Target {1}s => Kecepatan TTS: {2:0.00}x", wordCount, targetSeconds, speedRatio));
        }

        private void PopulateVoiceList()
        {
            _cbStudioVoice.Items.Clear();
            _cbStudioVoice.Items.Add("[Otomatis] Rotasi Suara Terjadwal (Fallback Cerdas)");
            _cbStudioVoice.Items.Add("Kore (Gemini HD - Wanita Tenang & Berkelas)");
            _cbStudioVoice.Items.Add("Aoede (Gemini HD - Wanita Hangat & Ekspresif)");
            _cbStudioVoice.Items.Add("Leda (Gemini HD - Wanita Muda & Santai)");
            _cbStudioVoice.Items.Add("Zephyr (Gemini HD - Wanita Lembut & Natural)");
            _cbStudioVoice.Items.Add("Charon (Gemini HD - Pria Berwibawa & Dokumenter)");
            _cbStudioVoice.Items.Add("Fenrir (Gemini HD - Pria Tegas & Teknis)");
            _cbStudioVoice.Items.Add("Puck (Gemini HD - Pria Ceria & Energik)");
            _cbStudioVoice.Items.Add("Sarah (ElevenLabs - Wanita Bijak)");
            _cbStudioVoice.Items.Add("Arunika (ElevenLabs - Audio Master Studio)");
            _cbStudioVoice.Items.Add("Roger (ElevenLabs - Pria Resonan)");
            _cbStudioVoice.SelectedIndex = 0;
        }

        private void PopulateIntonationList()
        {
            _cbStudioIntonation.Items.Clear();
            _cbStudioIntonation.Items.Add("[Otomatis] Sesuai Karakter & Tema");
            _cbStudioIntonation.Items.Add("Tenang, Dingin & Stoik (Reflektif & Jeda)");
            _cbStudioIntonation.Items.Add("Tegas, Wibawa & Heroik (Membakar Tekad)");
            _cbStudioIntonation.Items.Add("Misterius & Dark Mind (Psikologi Gelap)");
            _cbStudioIntonation.Items.Add("Hangat, Mengalir & Empatik (Storytelling)");
            _cbStudioIntonation.Items.Add("Dokumenter Lugas & Realitas Keras");
            _cbStudioIntonation.Items.Add("Spiritual & Khidmat (Menenangkan Jiwa)");
            _cbStudioIntonation.SelectedIndex = 0;
        }

        private void PopulateGradingList()
        {
            _cbStudioGrading.Items.Clear();
            _cbStudioGrading.Items.AddRange(new object[] {
                "[Otomatis] Sesuai Naskah",
                "Nordic Cold (Dingin Stoik)",
                "Teal & Orange (Sinematik)",
                "Golden Warm (Hangat Reflektif)",
                "Contrasty Cold (Pekat)",
                "Deep Teal (Modern)",
                "Warm Contrasty (Hangat Tegas)",
                "Neutral Fade (Film Lembut)",
                "Tanpa LUT (Alami)"
            });
            _cbStudioGrading.SelectedIndex = 0;
        }

        private void PopulateBgmList()
        {
            _cbStudioBgm.Items.Clear();
            _cbStudioBgm.Items.Add("[Otomatis] Rotasi Musik Tema");
            _cbStudioBgm.Items.Add("Tanpa Musik Latar");

            try
            {
                string bgmDir = Helpers.AppPaths.BgmDir;
                if (Directory.Exists(bgmDir))
                {
                    string[] files = Directory.GetFiles(bgmDir, "*.mp3", SearchOption.AllDirectories);
                    foreach (string f in files)
                    {
                        string name = Path.GetFileName(f);
                        if (!string.IsNullOrEmpty(name) && !_cbStudioBgm.Items.Contains(name))
                        {
                            _cbStudioBgm.Items.Add(name);
                        }
                    }
                }
            }
            catch { }

            _cbStudioBgm.SelectedIndex = 0;
        }

        private void UpdateSentenceCounter()
        {
            try
            {
                string text = _txtStudioScript.Text.Trim();
                int durIdx = (_cbStudioDuration != null) ? _cbStudioDuration.SelectedIndex : 2;
                int optMin = 22, optMax = 28;
                string targetDurStr = "3 Menit";

                if (durIdx == 0) { optMin = 11; optMax = 15; targetDurStr = "1 Menit"; }
                else if (durIdx == 1) { optMin = 17; optMax = 21; targetDurStr = "2 Menit"; }
                else if (durIdx == 2) { optMin = 22; optMax = 28; targetDurStr = "3 Menit"; }
                else if (durIdx == 3) { optMin = 36; optMax = 44; targetDurStr = "5 Menit"; }
                else if (durIdx == 4) { optMin = 75; optMax = 85; targetDurStr = "10 Menit"; }
                else if (durIdx == 5) { optMin = 145; optMax = 160; targetDurStr = "20 Menit"; }
                else if (durIdx == 6) { optMin = 215; optMax = 235; targetDurStr = "30 Menit"; }

                if (string.IsNullOrEmpty(text))
                {
                    _lblSentenceCount.Text = string.Format("Jumlah Adegan: 0 | Target: {0}-{1} Adegan (~{2})", optMin, optMax, targetDurStr);
                    _lblSentenceCount.ForeColor = UIFactory.AccentAmber;
                    return;
                }

                string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                int count = lines.Length;
                int wordCount = text.Split(new char[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
                int estSeconds = (int)Math.Round(wordCount / 2.50);
                string durasiStr = string.Format("{0}m {1:D2}s", estSeconds / 60, estSeconds % 60);

                if (count >= optMin && count <= optMax)
                {
                    _lblSentenceCount.Text = string.Format("Jumlah Adegan: {0} ({1} Kata ~{2}) — OPTIMAL ~{3}", count, wordCount, durasiStr, targetDurStr.ToUpper());
                    _lblSentenceCount.ForeColor = UIFactory.AccentGreen;
                }
                else
                {
                    _lblSentenceCount.Text = string.Format("Jumlah Adegan: {0} ({1} Kata ~{2}) — Target: {3}-{4} Adegan (~{5})", count, wordCount, durasiStr, optMin, optMax, targetDurStr);
                    _lblSentenceCount.ForeColor = UIFactory.AccentAmber;
                }
            }
            catch { }
        }

        private void SplitAndFormatScriptScenes()
        {
            try
            {
                string raw = _txtStudioScript.Text.Trim();
                if (string.IsNullOrEmpty(raw)) return;

                string spaced = System.Text.RegularExpressions.Regex.Replace(raw, @"([.!?])\s*([A-Z0-9])", "$1\r\n$2");
                string[] lines = spaced.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> cleanLines = new List<string>();
                foreach (string l in lines)
                {
                    string t = l.Trim();
                    if (!string.IsNullOrEmpty(t)) cleanLines.Add(t);
                }

                _txtStudioScript.Text = string.Join(Environment.NewLine, cleanLines.ToArray());
                UpdateSentenceCounter();
                AppendStudioLog(string.Format("[SPLIT NASKAH] Naskah dirapikan menjadi {0} baris adegan.", cleanLines.Count));
            }
            catch { }
        }

        private void FormatScript()
        {
            SplitAndFormatScriptScenes();
        }

        private void PasteFromClipboard()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    _txtStudioScript.Text = Clipboard.GetText().Trim();
                    SplitAndFormatScriptScenes();
                    AppendStudioLog("[CLIPBOARD] Teks berhasil ditempel dari clipboard.");
                }
            }
            catch { }
        }

        private void LoadSampleScript()
        {
            string sample = "Di balik setiap keheningan malam, selalu ada jiwa yang sedang bertarung melawan rasa lelah.\r\n"
                          + "Kita sering kali lupa bahwa bertahan bukanlah tentang tidak pernah jatuh.\r\n"
                          + "Melainkan tentang kesediaan untuk berdiri lagi, satu kali lebih banyak dari saat kita tersungkur.\r\n"
                          + "Dunia mungkin terlalu bising dengan pencapaian orang lain.\r\n"
                          + "Namun perjalananmu memiliki garis waktu yang telah dirancang sempurna untuk dirimu sendiri.\r\n"
                          + "Fokuslah pada langkah kecil hari ini, bukan pada puncak gunung yang masih tertutup kabut.\r\n"
                          + "Karena konsistensi yang tenang akan selalu mengalahkan antusiasme yang sesaat.\r\n"
                          + "Bernapaslah dalam-dalam, lepaskan kekhawatiran yang belum tentu menjadi nyata.\r\n"
                          + "Dan ketahuilah, setiap usahamu yang tulus sedang membentuk karaktermu menjadi lebih kokoh.";
            _txtStudioTopic.Text = "Seni Bertahan di Tengah Badai Hidup";
            _txtStudioScript.Text = sample;
            UpdateSentenceCounter();
            AppendStudioLog("[CONTOH] Berhasil memuat naskah contoh edukasi Stoikisme & Kehidupan.");
        }

        private void SelectAndUploadCustomVoice()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Pilih Berkas Audio Vokal Naskah";
                ofd.Filter = "Berkas Audio (*.mp3;*.wav;*.m4a;*.aac)|*.mp3;*.wav;*.m4a;*.aac|Semua Berkas (*.*)|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    string chosen = ofd.FileName;
                    if (File.Exists(chosen))
                    {
                        _uploadedVoicePath = chosen;
                        string fName = Path.GetFileName(chosen);
                        _btnStudioUploadVoice.Text = "Vokal: " + (fName.Length > 14 ? fName.Substring(0, 11) + "..." : fName);
                        _btnStudioUploadVoice.NormalBg = Color.FromArgb(20, 83, 45);
                        _btnStudioUploadVoice.NormalText = Color.White;
                        if (_btnStudioClearVoice != null) _btnStudioClearVoice.Visible = true;

                        if (string.IsNullOrEmpty(_txtStudioTopic.Text.Trim()))
                        {
                            _txtStudioTopic.Text = Path.GetFileNameWithoutExtension(chosen).Replace("_", " ");
                        }

                        LayoutLeftPanelControls(_pnlStudioLeft.Width, _pnlStudioLeft.Height);
                        AppendStudioLog(string.Format("[VOKAL UPLOAD] Audio vokal terhubung: {0}", fName));
                    }
                }
            }
        }

        public void ResetUploadedCustomVoice()
        {
            _uploadedVoicePath = null;
            if (_btnStudioUploadVoice != null)
            {
                _btnStudioUploadVoice.Text = "+ Upload Vokal";
                _btnStudioUploadVoice.NormalBg = UIFactory.AccentPrimaryLight;
                _btnStudioUploadVoice.NormalText = UIFactory.TextPrimary;
            }
            if (_btnStudioClearVoice != null) _btnStudioClearVoice.Visible = false;
            LayoutLeftPanelControls(_pnlStudioLeft.Width, _pnlStudioLeft.Height);
            AppendStudioLog("[VOKAL] Mode vokal eksternal dinonaktifkan. Kembali ke TTS AI bawaan.");
        }

        private void ResetStudioSettingsToDefault()
        {
            if (_txtStudioTopic != null) _txtStudioTopic.Text = "";
            if (_txtStudioScript != null) _txtStudioScript.Text = "";
            if (_cbStudioTheme != null) _cbStudioTheme.SelectedIndex = 0;
            if (_cbStudioAudience != null) _cbStudioAudience.SelectedIndex = 0;
            if (_cbStudioDuration != null) _cbStudioDuration.SelectedIndex = 2;
            if (_cbStudioVoice != null) _cbStudioVoice.SelectedIndex = 0;
            if (_cbStudioIntonation != null) _cbStudioIntonation.SelectedIndex = 0;
            if (_cbStudioGrading != null) _cbStudioGrading.SelectedIndex = 0;
            if (_cbStudioBgm != null) _cbStudioBgm.SelectedIndex = 0;
            if (_tbStudioVoVolume != null) _tbStudioVoVolume.Value = 100;
            if (_tbStudioBgmVolume != null) _tbStudioBgmVolume.Value = 100;
            if (_tbStudioVoSpeed != null) _tbStudioVoSpeed.Value = 100;
            if (_chkAutoSpeed != null) _chkAutoSpeed.Checked = false;
            if (_chkFootageAuto != null) _chkFootageAuto.Checked = true;
            UpdateStudioVolumeLabels();
            UpdateSpeedLabel();
            UpdateSentenceCounter();
            ResetUploadedCustomVoice();
            AppendStudioLog("[RESET] Pengaturan studio dikembalikan ke standar awal.");
        }

        public void AppendStudioLog(string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendStudioLog), msg);
                return;
            }

            string formatted = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), msg);
            if (_txtStudioLog != null)
            {
                _txtStudioLog.AppendText(formatted + Environment.NewLine);
            }
            LiveConsoleForm.Instance.AppendLog(formatted);
        }
    }
}
