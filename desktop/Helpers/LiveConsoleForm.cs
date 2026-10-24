using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SmartVidkonAutomation.Helpers
{
    public class LiveConsoleForm : Form
    {
        private static LiveConsoleForm _instance = null;
        private RichTextBox _rtxtConsole;
        private TextBox _txtSearch;
        private CheckBox _chkAutoScroll;
        private CheckBox _chkAlwaysOnTop;
        private Button _btnPauseResume;
        private Button _btnClear;
        private Button _btnCopy;
        private Label _lblStatus;
        private bool _isPaused = false;
        private StringBuilder _fullLog = new StringBuilder();

        public static LiveConsoleForm Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                {
                    _instance = new LiveConsoleForm();
                }
                return _instance;
            }
        }

        public LiveConsoleForm()
        {
            this.Text = "Smart Vidkon Automation — Live Telemetry Console";
            this.Size = new Size(880, 560);
            this.MinimumSize = new Size(650, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 23, 42); // Dark slate
            this.ForeColor = Color.FromArgb(226, 232, 240);
            this.Font = new Font("Segoe UI", 9f);

            InitializeControls();
        }

        private void InitializeControls()
        {
            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 48;
            pnlTop.BackColor = Color.FromArgb(30, 41, 59);
            pnlTop.Padding = new Padding(12, 8, 12, 8);

            Label lblFilter = new Label();
            lblFilter.Text = "Cari / Filter:";
            lblFilter.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblFilter.ForeColor = Color.FromArgb(148, 163, 184);
            lblFilter.Location = new Point(12, 14);
            lblFilter.AutoSize = true;
            pnlTop.Controls.Add(lblFilter);

            _txtSearch = new TextBox();
            _txtSearch.Location = new Point(95, 11);
            _txtSearch.Size = new Size(220, 26);
            _txtSearch.BackColor = Color.FromArgb(15, 23, 42);
            _txtSearch.ForeColor = Color.White;
            _txtSearch.Font = new Font("Segoe UI", 9.5f);
            _txtSearch.BorderStyle = BorderStyle.FixedSingle;
            _txtSearch.TextChanged += delegate(object s, EventArgs e) { ApplyFilter(); };
            pnlTop.Controls.Add(_txtSearch);

            _btnPauseResume = new Button();
            _btnPauseResume.Text = "Jeda";
            _btnPauseResume.Location = new Point(325, 9);
            _btnPauseResume.Size = new Size(76, 28);
            _btnPauseResume.BackColor = Color.FromArgb(51, 65, 85);
            _btnPauseResume.ForeColor = Color.White;
            _btnPauseResume.FlatStyle = FlatStyle.Flat;
            _btnPauseResume.FlatAppearance.BorderSize = 0;
            _btnPauseResume.Click += delegate(object s, EventArgs e)
            {
                _isPaused = !_isPaused;
                _btnPauseResume.Text = _isPaused ? "Lanjutkan" : "Jeda";
                _btnPauseResume.BackColor = _isPaused ? Color.FromArgb(180, 83, 9) : Color.FromArgb(51, 65, 85);
            };
            pnlTop.Controls.Add(_btnPauseResume);

            _btnClear = new Button();
            _btnClear.Text = "Bersihkan";
            _btnClear.Location = new Point(408, 9);
            _btnClear.Size = new Size(84, 28);
            _btnClear.BackColor = Color.FromArgb(51, 65, 85);
            _btnClear.ForeColor = Color.White;
            _btnClear.FlatStyle = FlatStyle.Flat;
            _btnClear.FlatAppearance.BorderSize = 0;
            _btnClear.Click += delegate(object s, EventArgs e)
            {
                _fullLog.Clear();
                _rtxtConsole.Clear();
                UpdateStatus();
            };
            pnlTop.Controls.Add(_btnClear);

            _btnCopy = new Button();
            _btnCopy.Text = "Salin Semua";
            _btnCopy.Location = new Point(499, 9);
            _btnCopy.Size = new Size(95, 28);
            _btnCopy.BackColor = Color.FromArgb(51, 65, 85);
            _btnCopy.ForeColor = Color.White;
            _btnCopy.FlatStyle = FlatStyle.Flat;
            _btnCopy.FlatAppearance.BorderSize = 0;
            _btnCopy.Click += delegate(object s, EventArgs e)
            {
                try
                {
                    if (!string.IsNullOrEmpty(_rtxtConsole.Text))
                    {
                        Clipboard.SetText(_rtxtConsole.Text);
                        MessageBox.Show("Seluruh log telemetri berhasil disalin ke clipboard.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch { }
            };
            pnlTop.Controls.Add(_btnCopy);

            _chkAutoScroll = new CheckBox();
            _chkAutoScroll.Text = "Auto Scroll";
            _chkAutoScroll.Checked = true;
            _chkAutoScroll.ForeColor = Color.FromArgb(203, 213, 225);
            _chkAutoScroll.Location = new Point(610, 14);
            _chkAutoScroll.AutoSize = true;
            pnlTop.Controls.Add(_chkAutoScroll);

            _chkAlwaysOnTop = new CheckBox();
            _chkAlwaysOnTop.Text = "Always On Top";
            _chkAlwaysOnTop.Checked = false;
            _chkAlwaysOnTop.ForeColor = Color.FromArgb(203, 213, 225);
            _chkAlwaysOnTop.Location = new Point(715, 14);
            _chkAlwaysOnTop.AutoSize = true;
            _chkAlwaysOnTop.CheckedChanged += delegate(object s, EventArgs e)
            {
                this.TopMost = _chkAlwaysOnTop.Checked;
            };
            pnlTop.Controls.Add(_chkAlwaysOnTop);

            this.Controls.Add(pnlTop);

            // Bottom Status Bar
            Panel pnlBottom = new Panel();
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Height = 26;
            pnlBottom.BackColor = Color.FromArgb(30, 41, 59);
            pnlBottom.Padding = new Padding(12, 4, 12, 4);

            _lblStatus = new Label();
            _lblStatus.Text = "Siap memantau telemetri pipeline.";
            _lblStatus.Font = new Font("Segoe UI", 8.25f);
            _lblStatus.ForeColor = Color.FromArgb(148, 163, 184);
            _lblStatus.Dock = DockStyle.Fill;
            pnlBottom.Controls.Add(_lblStatus);

            this.Controls.Add(pnlBottom);

            // Center RichTextBox
            _rtxtConsole = new RichTextBox();
            _rtxtConsole.Dock = DockStyle.Fill;
            _rtxtConsole.BackColor = Color.FromArgb(15, 23, 42);
            _rtxtConsole.ForeColor = Color.FromArgb(241, 245, 249);
            _rtxtConsole.Font = new Font("Consolas", 9f);
            _rtxtConsole.ReadOnly = true;
            _rtxtConsole.BorderStyle = BorderStyle.None;
            _rtxtConsole.WordWrap = false;
            _rtxtConsole.ScrollBars = RichTextBoxScrollBars.Both;

            this.Controls.Add(_rtxtConsole);
            _rtxtConsole.BringToFront();
        }

        public void AppendLog(string message)
        {
            if (_isPaused) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendLog), message);
                return;
            }

            _fullLog.AppendLine(message);

            string filter = _txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(filter) || message.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Color clr = Color.FromArgb(241, 245, 249);
                if (message.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("FAIL", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    clr = Color.FromArgb(248, 113, 113); // red
                }
                else if (message.IndexOf("WARN", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    clr = Color.FromArgb(251, 191, 36); // amber
                }
                else if (message.IndexOf("SUCCESS", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("OK", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("SELESAI", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    clr = Color.FromArgb(74, 222, 128); // green
                }

                _rtxtConsole.SelectionStart = _rtxtConsole.TextLength;
                _rtxtConsole.SelectionLength = 0;
                _rtxtConsole.SelectionColor = clr;
                _rtxtConsole.AppendText(message + Environment.NewLine);
                _rtxtConsole.SelectionColor = _rtxtConsole.ForeColor;

                if (_chkAutoScroll.Checked)
                {
                    _rtxtConsole.ScrollToCaret();
                }
            }

            UpdateStatus();
        }

        private void ApplyFilter()
        {
            string filter = _txtSearch.Text.Trim();
            _rtxtConsole.Clear();
            string[] lines = _fullLog.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                if (string.IsNullOrEmpty(filter) || line.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _rtxtConsole.AppendText(line + Environment.NewLine);
                }
            }
            if (_chkAutoScroll.Checked)
            {
                _rtxtConsole.ScrollToCaret();
            }
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            _lblStatus.Text = string.Format("Total baris log: {0} | Ditampilkan: {1}", 
                _fullLog.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length,
                _rtxtConsole.Lines.Length);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                base.OnFormClosing(e);
            }
        }
    }
}
