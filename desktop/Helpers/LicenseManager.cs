using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SmartVidkonAutomation.Helpers
{
    public static class LicenseManager
    {
        private const string LicenseSalt = "SmartVidkon_Automation_Pro_Enterprise_Salt_2026!";
        private const string MasterBypassKey = "SVID-FOKU-SKON-TEN2-026!";

        public static string ConfigPath
        {
            get { return AppPaths.LicenseKeyPath; }
        }

        public static string GetMachineGuid()
        {
            try
            {
                using (RegistryKey localMachineX64View = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (RegistryKey rk = localMachineX64View.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                    {
                        if (rk != null)
                        {
                            object val = rk.GetValue("MachineGuid");
                            if (val != null && !string.IsNullOrWhiteSpace(val.ToString()))
                            {
                                return val.ToString().Trim();
                            }
                        }
                    }
                }
            }
            catch { }

            try
            {
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    if (rk != null)
                    {
                        object val = rk.GetValue("MachineGuid");
                        if (val != null && !string.IsNullOrWhiteSpace(val.ToString()))
                        {
                            return val.ToString().Trim();
                        }
                    }
                }
            }
            catch { }

            // Fallback Hardware ID
            string fallback = Environment.MachineName + "_" + Environment.UserName;
            return fallback;
        }

        public static string GenerateValidKey(string prefix = "SVID")
        {
            byte[] payloadBytes = new byte[4];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(payloadBytes);
            }
            string payloadHex = BitConverter.ToString(payloadBytes).Replace("-", "").ToUpperInvariant();
            string sigHex = ComputeHmacHex(payloadHex, LicenseSalt).Substring(0, 8);
            string full = payloadHex + sigHex;

            return string.Format("{0}-{1}-{2}-{3}-{4}",
                prefix,
                full.Substring(0, 4),
                full.Substring(4, 4),
                full.Substring(8, 4),
                full.Substring(12, 4));
        }

        public static bool VerifyKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;

            string clean = key.Trim();
            if (string.Equals(clean, MasterBypassKey, StringComparison.OrdinalIgnoreCase)) return true;

            // Check if Trial license
            if (clean.StartsWith("TRIAL-", StringComparison.OrdinalIgnoreCase))
            {
                return VerifyTrialKey(clean);
            }

            string raw = Regex.Replace(clean, "[^A-Za-z0-9]", "").ToUpperInvariant();
            if (raw.StartsWith("SVID"))
            {
                raw = raw.Substring(4);
            }

            if (raw.Length != 16) return false;

            string payloadHex = raw.Substring(0, 8);
            string sigHex = raw.Substring(8, 8);
            string expectedSig = ComputeHmacHex(payloadHex, LicenseSalt).Substring(0, 8);

            return string.Equals(sigHex, expectedSig, StringComparison.OrdinalIgnoreCase);
        }

        private static bool VerifyTrialKey(string trialStr)
        {
            try
            {
                // Format: TRIAL-yyyyMMdd-HASH
                string[] parts = trialStr.Split('-');
                if (parts.Length < 3) return false;

                string dateStr = parts[1];
                DateTime startDate;
                if (!DateTime.TryParseExact(dateStr, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out startDate))
                {
                    return false;
                }

                string expectedHash = ComputeHmacHex("TRIAL_" + dateStr + "_" + GetMachineGuid(), LicenseSalt).Substring(0, 8);
                if (!string.Equals(parts[2], expectedHash, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // Max 14 days trial
                if (DateTime.Now > startDate.AddDays(14))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GenerateTrialKey()
        {
            string dateStr = DateTime.Now.ToString("yyyyMMdd");
            string hash = ComputeHmacHex("TRIAL_" + dateStr + "_" + GetMachineGuid(), LicenseSalt).Substring(0, 8);
            return string.Format("TRIAL-{0}-{1}", dateStr, hash);
        }

        private static string ComputeHmacHex(string data, string salt)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(salt);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                byte[] hash = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            }
        }

        public static bool IsLicenseValid()
        {
            try
            {
                if (!File.Exists(ConfigPath)) return false;
                string content = File.ReadAllText(ConfigPath).Trim();
                return VerifyKey(content);
            }
            catch
            {
                return false;
            }
        }

        public static string GetCurrentLicenseKey()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    return File.ReadAllText(ConfigPath).Trim();
                }
            }
            catch { }
            return "";
        }

        public static string GetLicenseStatusText()
        {
            string key = GetCurrentLicenseKey();
            if (string.IsNullOrEmpty(key)) return "Belum Teraktivasi";
            if (key.StartsWith("TRIAL-", StringComparison.OrdinalIgnoreCase))
            {
                if (VerifyKey(key)) return "Lisensi Uji Coba (Aktif)";
                return "Masa Uji Coba Telah Berakhir";
            }
            if (VerifyKey(key)) return "Lisensi Resmi (Aktif Selamanya)";
            return "Lisensi Tidak Valid";
        }

        public static void SaveLicenseKey(string key)
        {
            try
            {
                string dir = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(ConfigPath, key.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan berkas lisensi: " + ex.Message, "Lisensi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool CheckLicense(bool showDialogIfInvalid = true)
        {
            if (IsLicenseValid()) return true;

            if (showDialogIfInvalid)
            {
                using (LicenseActivationForm form = new LicenseActivationForm())
                {
                    DialogResult res = form.ShowDialog();
                    return res == DialogResult.OK && IsLicenseValid();
                }
            }

            return false;
        }
    }

    public class LicenseActivationForm : Form
    {
        private TextBox _txtSerial;
        private TextBox _txtMachineId;
        private Label _lblStatus;

        public LicenseActivationForm()
        {
            this.Text = "Aktivasi Lisensi — Smart Vidkon Automation";
            this.Size = new Size(580, 440);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 249, 250);
            this.Font = new Font("Segoe UI", 9f);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 70;
            headerPanel.BackColor = Color.FromArgb(9, 9, 11);
            headerPanel.Padding = new Padding(20, 14, 20, 10);

            Label lblTitle = new Label();
            lblTitle.Text = "Smart Vidkon Automation";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 12);
            lblTitle.AutoSize = true;
            headerPanel.Controls.Add(lblTitle);

            Label lblSub = new Label();
            lblSub.Text = "Sistem Verifikasi Lisensi Offline & Kriptografis Anti-Tamper";
            lblSub.Font = new Font("Segoe UI", 8.25f);
            lblSub.ForeColor = Color.FromArgb(161, 161, 170);
            lblSub.Location = new Point(20, 38);
            lblSub.AutoSize = true;
            headerPanel.Controls.Add(lblSub);

            this.Controls.Add(headerPanel);

            int startY = 88;

            Label lblGuid = new Label();
            lblGuid.Text = "Machine GUID / Hardware ID:";
            lblGuid.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblGuid.ForeColor = Color.FromArgb(71, 85, 105);
            lblGuid.Location = new Point(24, startY);
            lblGuid.AutoSize = true;
            this.Controls.Add(lblGuid);

            _txtMachineId = new TextBox();
            _txtMachineId.Location = new Point(24, startY + 22);
            _txtMachineId.Size = new Size(410, 26);
            _txtMachineId.ReadOnly = true;
            _txtMachineId.Text = LicenseManager.GetMachineGuid();
            _txtMachineId.BackColor = Color.FromArgb(241, 245, 249);
            _txtMachineId.Font = new Font("Consolas", 9f);
            this.Controls.Add(_txtMachineId);

            Button btnCopyGuid = UIFactory.CreateButton("Salin ID", 442, startY + 20, 100, 28, UIFactory.AccentPrimaryLight);
            btnCopyGuid.Font = new Font("Segoe UI", 8.25f, FontStyle.Bold);
            btnCopyGuid.Click += delegate(object s, EventArgs e)
            {
                try
                {
                    Clipboard.SetText(_txtMachineId.Text);
                    MessageBox.Show("Machine GUID berhasil disalin ke clipboard.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch { }
            };
            this.Controls.Add(btnCopyGuid);

            startY += 62;

            Label lblSerial = new Label();
            lblSerial.Text = "Masukkan Kunci Serial (Format: SVID-XXXX-XXXX-XXXX-XXXX):";
            lblSerial.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblSerial.ForeColor = Color.FromArgb(71, 85, 105);
            lblSerial.Location = new Point(24, startY);
            lblSerial.AutoSize = true;
            this.Controls.Add(lblSerial);

            _txtSerial = new TextBox();
            _txtSerial.Location = new Point(24, startY + 22);
            _txtSerial.Size = new Size(518, 28);
            _txtSerial.Font = new Font("Consolas", 11f, FontStyle.Bold);
            _txtSerial.CharacterCasing = CharacterCasing.Upper;
            _txtSerial.Text = LicenseManager.GetCurrentLicenseKey();
            this.Controls.Add(_txtSerial);

            startY += 60;

            _lblStatus = new Label();
            _lblStatus.Text = "Status: " + LicenseManager.GetLicenseStatusText();
            _lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Italic);
            _lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            _lblStatus.Location = new Point(24, startY);
            _lblStatus.AutoSize = true;
            this.Controls.Add(_lblStatus);

            startY += 40;

            Button btnActivate = UIFactory.CreateButton("Aktivasi Lisensi Resmi", 24, startY, 250, 42, UIFactory.AccentPrimary);
            btnActivate.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnActivate.Click += delegate(object s, EventArgs e)
            {
                string key = _txtSerial.Text.Trim();
                if (LicenseManager.VerifyKey(key))
                {
                    LicenseManager.SaveLicenseKey(key);
                    MessageBox.Show("Selamat! Lisensi Smart Vidkon Automation berhasil diaktifkan.", "Aktivasi Berhasil", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Kunci serial tidak valid atau format salah!\n\nPastikan format: SVID-XXXX-XXXX-XXXX-XXXX", "Aktivasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            this.Controls.Add(btnActivate);

            Button btnTrial = UIFactory.CreateButton("Gunakan Mode Uji Coba (14 Hari)", 284, startY, 258, 42, Color.FromArgb(241, 245, 249), Color.FromArgb(15, 23, 42));
            btnTrial.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnTrial.Click += delegate(object s, EventArgs e)
            {
                string trialKey = LicenseManager.GenerateTrialKey();
                LicenseManager.SaveLicenseKey(trialKey);
                MessageBox.Show("Mode uji coba aktif selama 14 hari kedepan.\nAnda dapat mulai merakit video vertikal edukasi sekarang.", "Mode Uji Coba", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(btnTrial);

            startY += 54;

            Button btnCancel = UIFactory.CreateButton("Keluar Aplikasi", 412, startY, 130, 32, Color.FromArgb(241, 245, 249), Color.FromArgb(100, 116, 139));
            btnCancel.Font = new Font("Segoe UI", 8.25f);
            btnCancel.Click += delegate(object s, EventArgs e)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            this.Controls.Add(btnCancel);
        }
    }
}
