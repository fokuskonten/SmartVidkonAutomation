using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using SmartVidkonAutomation.Helpers;

namespace SmartVidkonAutomation
{
    public partial class MainForm
    {
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

        private bool _isBgmPlaying = false;
        private bool _isVoPlaying = false;

        private void CreateLutBigPreviewComponents()
        {
            _pnlLutBigPreview = new Panel
            {
                Location = new Point(14, 260),
                Size = new Size(456, 420),
                BackColor = Color.FromArgb(250, 250, 252),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            _lblLutBigTitle = new Label
            {
                Text = "PRATINJAU 3D LUT",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = UIFactory.TextPrimary,
                Location = new Point(12, 10),
                Size = new Size(330, 22),
                AutoEllipsis = true
            };
            _pnlLutBigPreview.Controls.Add(_lblLutBigTitle);

            _btnLutBigClose = UIFactory.CreateButton("Tutup", 355, 7, 85, 26, UIFactory.AccentPrimaryLight, 6);
            _btnLutBigClose.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            _btnLutBigClose.Click += delegate(object s, EventArgs e) { SetLutBigPreviewVisible(false); };
            _pnlLutBigPreview.Controls.Add(_btnLutBigClose);

            _pbLutBigPreview = new PictureBox
            {
                Location = new Point(14, 40),
                Size = new Size(428, 330),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(15, 23, 42),
                BorderStyle = BorderStyle.FixedSingle
            };
            _pbLutBigPreview.Click += delegate(object s, EventArgs e) { SetLutBigPreviewVisible(false); };
            _pnlLutBigPreview.Controls.Add(_pbLutBigPreview);

            _lblLutBigDesc = new Label
            {
                Text = "Karakter color grading 3D LUT sinematik 9:16 vertikal.",
                Font = new Font("Segoe UI", 8.25f, FontStyle.Italic),
                ForeColor = UIFactory.TextSecondary,
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _pnlLutBigPreview.Controls.Add(_lblLutBigDesc);
        }

        private void CreateVideoBigPreviewComponents()
        {
            _pnlVideoBigPreview = new Panel
            {
                Location = new Point(14, 260),
                Size = new Size(456, 420),
                BackColor = Color.FromArgb(250, 250, 252),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            _lblVideoBigBadge = new Label
            {
                Text = "PRATINJAU VIDEO FINAL",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = UIFactory.TextPrimary,
                Location = new Point(12, 10),
                Size = new Size(180, 22),
                AutoEllipsis = true
            };
            _pnlVideoBigPreview.Controls.Add(_lblVideoBigBadge);

            _btnVideoCardPlay = UIFactory.CreateButton("Putar Video", 195, 7, 95, 26, UIFactory.AccentPrimary, 6);
            _btnVideoCardPlay.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            _btnVideoCardPlay.Click += delegate(object s, EventArgs e) { LaunchVideoInSystemPlayer(); };
            _pnlVideoBigPreview.Controls.Add(_btnVideoCardPlay);

            _btnVideoLaunchApp = UIFactory.CreateButton("Buka Lokasi", 296, 7, 90, 26, UIFactory.AccentPrimaryLight, 6);
            _btnVideoLaunchApp.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            _btnVideoLaunchApp.Click += delegate(object s, EventArgs e) { OpenStudioFolder(); };
            _pnlVideoBigPreview.Controls.Add(_btnVideoLaunchApp);

            _btnVideoBigClose = UIFactory.CreateButton("Tutup", 392, 7, 50, 26, UIFactory.AccentPrimaryLight, 6);
            _btnVideoBigClose.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            _btnVideoBigClose.Click += delegate(object s, EventArgs e) { SetVideoBigPreviewVisible(false); };
            _pnlVideoBigPreview.Controls.Add(_btnVideoBigClose);

            _pbVideoBigPreview = new PictureBox
            {
                Location = new Point(14, 40),
                Size = new Size(428, 360),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(15, 23, 42),
                BorderStyle = BorderStyle.FixedSingle
            };
            _pnlVideoBigPreview.Controls.Add(_pbVideoBigPreview);
        }

        public void SetLutBigPreviewVisible(bool visible)
        {
            _isLutBigPreviewVisible = visible;
            if (_pnlLutBigPreview != null) _pnlLutBigPreview.Visible = visible;
            if (visible && _pnlVideoBigPreview != null) _pnlVideoBigPreview.Visible = false;
        }

        public void SetVideoBigPreviewVisible(bool visible)
        {
            _isVideoBigPreviewVisible = visible;
            if (_pnlVideoBigPreview != null) _pnlVideoBigPreview.Visible = visible;
            if (visible && _pnlLutBigPreview != null) _pnlLutBigPreview.Visible = false;
        }

        private void ToggleLutBigPreview()
        {
            SetLutBigPreviewVisible(!_isLutBigPreviewVisible);
        }

        private void ToggleVideoBigPreview()
        {
            if (string.IsNullOrEmpty(_lastRenderedVideoPath) || !File.Exists(_lastRenderedVideoPath))
            {
                // Cek folder proyek lokal terakhir
                string outputDir = AppPaths.ProjectsDir;
                if (Directory.Exists(outputDir))
                {
                    string[] files = Directory.GetFiles(outputDir, "*.mp4", SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        Array.Sort(files);
                        _lastRenderedVideoPath = files[files.Length - 1];
                    }
                }
            }

            if (!string.IsNullOrEmpty(_lastRenderedVideoPath) && File.Exists(_lastRenderedVideoPath))
            {
                LaunchVideoInSystemPlayer();
            }
            else
            {
                MessageBox.Show("Belum ada video final yang selesai dirakit.\nSilakan tekan tombol [MULAI PERAKITAN VIDEO] terlebih dahulu.", "Putar Video", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LaunchVideoInSystemPlayer()
        {
            try
            {
                if (!string.IsNullOrEmpty(_lastRenderedVideoPath) && File.Exists(_lastRenderedVideoPath))
                {
                    Process.Start(_lastRenderedVideoPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memutar video: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenStudioFolder()
        {
            try
            {
                string targetDir = AppPaths.ProjectsDir;
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
                Process.Start("explorer.exe", targetDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membuka folder: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateLutPreview()
        {
            if (_cbStudioGrading == null) return;
            string sel = _cbStudioGrading.SelectedItem != null ? _cbStudioGrading.SelectedItem.ToString() : "";
            if (_lblLutBigTitle != null) _lblLutBigTitle.Text = "PRATINJAU 3D LUT: [" + sel + "]";
            if (_lblLutBigDesc != null)
            {
                if (sel.Contains("Nordic Cold")) _lblLutBigDesc.Text = "Karakter: Dingin stoik, shadow pekat, nuansa filosofis Eropa Utara.";
                else if (sel.Contains("Teal & Orange")) _lblLutBigDesc.Text = "Karakter: Kontras sinematik Hollywood modern, warna kulit cerah & punchy.";
                else if (sel.Contains("Golden Warm")) _lblLutBigDesc.Text = "Karakter: Hangat reflektif, golden hour, nuansa kebijaksanaan & memori.";
                else if (sel.Contains("Contrasty Cold")) _lblLutBigDesc.Text = "Karakter: Pekat, desaturasi dramatis, cocok untuk tema psikologi gelap.";
                else _lblLutBigDesc.Text = "Grading 3D LUT sinematik 9:16 untuk video vertikal berdaya hipnosis tinggi.";
            }
        }

        private void ToggleBgmPlayback()
        {
            if (_isBgmPlaying)
            {
                StopBgmPlayback();
            }
            else
            {
                PlaySelectedBgm();
            }
        }

        private void PlaySelectedBgm()
        {
            try
            {
                StopBgmPlayback();

                string bgmDir = StudioBgmLibrary;
                string sel = (_cbStudioBgm != null && _cbStudioBgm.SelectedItem != null) ? _cbStudioBgm.SelectedItem.ToString() : "";
                if (sel.Contains("Tanpa Musik")) return;

                string filePath = null;
                if (Directory.Exists(bgmDir))
                {
                    string[] candidates = Directory.GetFiles(bgmDir, "*.mp3", SearchOption.AllDirectories);
                    if (candidates.Length > 0)
                    {
                        filePath = candidates[0];
                        foreach (string c in candidates)
                        {
                            if (Path.GetFileName(c).IndexOf(sel, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                filePath = c;
                                break;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    mciSendString("close bgm_alias", null, 0, IntPtr.Zero);
                    string openCmd = string.Format("open \"{0}\" type mpegvideo alias bgm_alias", filePath);
                    mciSendString(openCmd, null, 0, IntPtr.Zero);
                    mciSendString("play bgm_alias", null, 0, IntPtr.Zero);
                    _isBgmPlaying = true;

                    if (_btnPreviewBgm != null)
                    {
                        _btnPreviewBgm.Text = "Stop";
                        _btnPreviewBgm.NormalBg = UIFactory.AccentPrimary;
                        _btnPreviewBgm.NormalText = Color.White;
                    }
                    AppendStudioLog("[BGM] Memutar pratinjau musik latar: " + Path.GetFileName(filePath));
                }
                else
                {
                    AppendStudioLog("[BGM] Berkas audio BGM tidak ditemukan di pustaka.");
                }
            }
            catch (Exception ex)
            {
                AppendStudioLog("[BGM ERROR] " + ex.Message);
            }
        }

        public void StopBgmPlayback()
        {
            try
            {
                mciSendString("stop bgm_alias", null, 0, IntPtr.Zero);
                mciSendString("close bgm_alias", null, 0, IntPtr.Zero);
            }
            catch { }
            _isBgmPlaying = false;
            if (_btnPreviewBgm != null)
            {
                _btnPreviewBgm.Text = "Dengar Audio";
                _btnPreviewBgm.NormalBg = UIFactory.AccentPrimaryLight;
                _btnPreviewBgm.NormalText = UIFactory.TextPrimary;
            }
        }

        private void ToggleVoPlayback()
        {
            if (_isVoPlaying)
            {
                StopVoPlayback();
            }
            else
            {
                PlaySelectedVo();
            }
        }

        private void PlaySelectedVo()
        {
            try
            {
                StopVoPlayback();
                string filePath = _uploadedVoicePath;

                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    mciSendString("close vo_alias", null, 0, IntPtr.Zero);
                    string openCmd = string.Format("open \"{0}\" type mpegvideo alias vo_alias", filePath);
                    mciSendString(openCmd, null, 0, IntPtr.Zero);
                    mciSendString("play vo_alias", null, 0, IntPtr.Zero);
                    _isVoPlaying = true;

                    if (_btnStudioPreviewVoice != null)
                    {
                        _btnStudioPreviewVoice.Text = "Stop";
                        _btnStudioPreviewVoice.NormalBg = UIFactory.AccentPrimary;
                        _btnStudioPreviewVoice.NormalText = Color.White;
                    }
                    AppendStudioLog("[VOKAL] Memutar audio vokal: " + Path.GetFileName(filePath));
                }
                else
                {
                    AppendStudioLog("[VOKAL] Belum ada berkas vokal eksternal yang diunggah.");
                }
            }
            catch (Exception ex)
            {
                AppendStudioLog("[VOKAL ERROR] " + ex.Message);
            }
        }

        public void StopVoPlayback()
        {
            try
            {
                mciSendString("stop vo_alias", null, 0, IntPtr.Zero);
                mciSendString("close vo_alias", null, 0, IntPtr.Zero);
            }
            catch { }
            _isVoPlaying = false;
            if (_btnStudioPreviewVoice != null)
            {
                _btnStudioPreviewVoice.Text = "Dengar Audio";
                _btnStudioPreviewVoice.NormalBg = UIFactory.AccentPrimaryLight;
                _btnStudioPreviewVoice.NormalText = UIFactory.TextPrimary;
            }
        }

        private void UploadLut()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Pilih Berkas 3D LUT (.cube)";
                ofd.Filter = "Berkas LUT (*.cube)|*.cube|Semua Berkas (*.*)|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        string destDir = StudioLutsVault;
                        if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                        string destPath = Path.Combine(destDir, Path.GetFileName(ofd.FileName));
                        File.Copy(ofd.FileName, destPath, true);
                        PopulateGradingList();
                        MessageBox.Show("Berkas 3D LUT berhasil diimpor: " + Path.GetFileName(destPath), "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal mengimpor LUT: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void UploadBgm()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Pilih Berkas Musik Latar (.mp3)";
                ofd.Filter = "Berkas Audio (*.mp3;*.wav)|*.mp3;*.wav|Semua Berkas (*.*)|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        string destDir = StudioBgmLibrary;
                        if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                        string destPath = Path.Combine(destDir, Path.GetFileName(ofd.FileName));
                        File.Copy(ofd.FileName, destPath, true);
                        PopulateBgmList();
                        MessageBox.Show("Berkas BGM berhasil diimpor ke pustaka audio: " + Path.GetFileName(destPath), "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal mengimpor BGM: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
