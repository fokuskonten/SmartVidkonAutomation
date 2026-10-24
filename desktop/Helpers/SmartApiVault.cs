using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace SmartVidkonAutomation.Helpers
{
    public class StudioApiKeyItem
    {
        public string id { get; set; }
        public string provider { get; set; } // "Gemini" | "ElevenLabs"
        public string key { get; set; }
        public string label { get; set; }
        public bool is_active { get; set; }
        public string last_tested_status { get; set; }
        public string last_tested_at { get; set; }

        public StudioApiKeyItem()
        {
            id = Guid.NewGuid().ToString("N");
            is_active = true;
            last_tested_status = "Belum diuji";
            last_tested_at = "-";
        }
    }

    public static class SmartApiVault
    {
        public static string VaultFilePath
        {
            get { return AppPaths.ApiVaultPath; }
        }

        private static List<StudioApiKeyItem> _cache = null;

        public static List<StudioApiKeyItem> GetDefaultItems()
        {
            // Untuk pengguna baru: Mulai dengan daftar kosong.
            // Pengguna menambahkan Kunci API mereka sendiri (Gemini / ElevenLabs) via antarmuka Vault.
            return new List<StudioApiKeyItem>();
        }

        public static List<StudioApiKeyItem> LoadVault()
        {
            if (_cache != null) return _cache;

            string path = VaultFilePath;
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path, Encoding.UTF8);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(json);
                    if (dict != null)
                    {
                        List<StudioApiKeyItem> list = new List<StudioApiKeyItem>();
                        if (dict.ContainsKey("gemini_keys"))
                        {
                            ArrayList gArr = dict["gemini_keys"] as ArrayList;
                            if (gArr != null)
                            {
                                foreach (Dictionary<string, object> entry in gArr)
                                {
                                    list.Add(new StudioApiKeyItem
                                    {
                                        id = entry.ContainsKey("id") ? entry["id"].ToString() : Guid.NewGuid().ToString("N"),
                                        provider = "Gemini",
                                        key = entry.ContainsKey("key") ? entry["key"].ToString() : "",
                                        label = entry.ContainsKey("label") ? entry["label"].ToString() : "Gemini Sekoci",
                                        is_active = entry.ContainsKey("is_active") ? Convert.ToBoolean(entry["is_active"]) : true,
                                        last_tested_status = entry.ContainsKey("last_tested_status") ? entry["last_tested_status"].ToString() : "Belum diuji",
                                        last_tested_at = entry.ContainsKey("last_tested_at") ? entry["last_tested_at"].ToString() : "-"
                                    });
                                }
                            }
                        }

                        if (dict.ContainsKey("elevenlabs_keys"))
                        {
                            ArrayList eArr = dict["elevenlabs_keys"] as ArrayList;
                            if (eArr != null)
                            {
                                foreach (Dictionary<string, object> entry in eArr)
                                {
                                    list.Add(new StudioApiKeyItem
                                    {
                                        id = entry.ContainsKey("id") ? entry["id"].ToString() : Guid.NewGuid().ToString("N"),
                                        provider = "ElevenLabs",
                                        key = entry.ContainsKey("key") ? entry["key"].ToString() : "",
                                        label = entry.ContainsKey("label") ? entry["label"].ToString() : "ElevenLabs Sekoci",
                                        is_active = entry.ContainsKey("is_active") ? Convert.ToBoolean(entry["is_active"]) : true,
                                        last_tested_status = entry.ContainsKey("last_tested_status") ? entry["last_tested_status"].ToString() : "Belum diuji",
                                        last_tested_at = entry.ContainsKey("last_tested_at") ? entry["last_tested_at"].ToString() : "-"
                                    });
                                }
                            }
                        }

                        _cache = list;
                        return _cache;
                    }
                }
                catch { }
            }

            _cache = GetDefaultItems();
            SaveVault(_cache);
            return _cache;
        }

        public static void SaveVault(List<StudioApiKeyItem> items)
        {
            try
            {
                _cache = items;
                string path = VaultFilePath;
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                List<Dictionary<string, object>> gList = new List<Dictionary<string, object>>();
                List<Dictionary<string, object>> eList = new List<Dictionary<string, object>>();

                foreach (StudioApiKeyItem item in items)
                {
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d.Add("id", item.id);
                    d.Add("provider", item.provider);
                    d.Add("key", item.key);
                    d.Add("label", item.label);
                    d.Add("is_active", item.is_active);
                    d.Add("last_tested_status", item.last_tested_status ?? "Belum diuji");
                    d.Add("last_tested_at", item.last_tested_at ?? "-");

                    if (item.provider == "Gemini") gList.Add(d);
                    else if (item.provider == "ElevenLabs") eList.Add(d);
                }

                Dictionary<string, object> root = new Dictionary<string, object>();
                root.Add("version", "1.0");
                root.Add("updated_at", DateTime.UtcNow.ToString("o"));
                root.Add("gemini_keys", gList);
                root.Add("elevenlabs_keys", eList);

                JavaScriptSerializer js = new JavaScriptSerializer();
                string json = js.Serialize(root);
                json = json.Replace("{\"", "{\r\n  \"").Replace(",\"", ",\r\n  \"").Replace("}],", "}\r\n],\r\n");

                File.WriteAllText(path, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Gagal menyimpan kunci API: " + ex.Message);
            }
        }

        public static List<string> GetStudioGeminiKeys()
        {
            List<StudioApiKeyItem> list = LoadVault();
            List<string> keys = new List<string>();
            foreach (StudioApiKeyItem item in list)
            {
                if (item.provider == "Gemini" && item.is_active && !string.IsNullOrEmpty(item.key))
                {
                    keys.Add(item.key.Trim());
                }
            }
            if (keys.Count == 0)
            {
                string envKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
                if (!string.IsNullOrEmpty(envKey)) keys.Add(envKey.Trim());
            }
            return keys;
        }

        public static List<string> GetStudioElevenLabsKeys()
        {
            List<StudioApiKeyItem> list = LoadVault();
            List<StudioApiKeyItem> elevenItems = new List<StudioApiKeyItem>();
            foreach (StudioApiKeyItem item in list)
            {
                if (item.provider == "ElevenLabs" && item.is_active && !string.IsNullOrEmpty(item.key))
                {
                    elevenItems.Add(item);
                }
            }

            elevenItems.Sort(delegate(StudioApiKeyItem a, StudioApiKeyItem b)
            {
                int quotaA = ExtractRemainingQuota(a.last_tested_status);
                int quotaB = ExtractRemainingQuota(b.last_tested_status);
                return quotaB.CompareTo(quotaA);
            });

            List<string> keys = new List<string>();
            foreach (StudioApiKeyItem item in elevenItems)
            {
                keys.Add(item.key.Trim());
            }

            if (keys.Count == 0)
            {
                string envKey = Environment.GetEnvironmentVariable("ELEVENLABS_API_KEY");
                if (!string.IsNullOrEmpty(envKey)) keys.Add(envKey.Trim());
            }
            return keys;
        }

        private static int ExtractRemainingQuota(string status)
        {
            if (string.IsNullOrEmpty(status)) return 0;
            Match m = Regex.Match(status, @"Sisa\s+([\d\.]+)", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                string raw = m.Groups[1].Value.Replace(".", "").Trim();
                int q;
                if (int.TryParse(raw, out q)) return q;
            }
            return 0;
        }

        public static async Task<string> TestGeminiKeyAsync(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey)) return "Kunci Kosong";
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | (System.Net.SecurityProtocolType)3072;
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    string url = string.Format("https://generativelanguage.googleapis.com/v1beta/models?key={0}", apiKey.Trim());
                    Stopwatch sw = Stopwatch.StartNew();
                    HttpResponseMessage resp = await client.GetAsync(url);
                    sw.Stop();

                    if (resp.IsSuccessStatusCode)
                    {
                        return string.Format("200 OK — Valid (Siap Pakai, {0} ms)", sw.ElapsedMilliseconds);
                    }
                    else if ((int)resp.StatusCode == 400 || (int)resp.StatusCode == 403)
                    {
                        return string.Format("Gagal HTTP {0}: Kunci Ditolak / Tidak Valid ({1} ms)", (int)resp.StatusCode, sw.ElapsedMilliseconds);
                    }
                    else if ((int)resp.StatusCode == 429)
                    {
                        return string.Format("Limit HTTP 429: Rate Limit Terlampaui ({0} ms)", sw.ElapsedMilliseconds);
                    }
                    else
                    {
                        return string.Format("HTTP {0} ({1} ms)", (int)resp.StatusCode, sw.ElapsedMilliseconds);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException || ex is System.Net.WebException || ex.Message.IndexOf("offline", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("name resolution", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("host", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Terputus (Offline / Masalah Jaringan)";
                }
                if (ex is TaskCanceledException || ex.Message.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Batas Waktu Habis (Timeout)";
                }
                return string.Format("Kendala: {0}", ex.Message);
            }
        }

        public static async Task<string> TestElevenLabsKeyAsync(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey)) return "Kunci Kosong";
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | (System.Net.SecurityProtocolType)3072;
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, "https://api.elevenlabs.io/v1/user/subscription");
                    req.Headers.Add("xi-api-key", apiKey.Trim());

                    Stopwatch sw = Stopwatch.StartNew();
                    HttpResponseMessage resp = await client.SendAsync(req);
                    sw.Stop();

                    if (resp.IsSuccessStatusCode)
                    {
                        string body = await resp.Content.ReadAsStringAsync();
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        Dictionary<string, object> dict = serializer.Deserialize<Dictionary<string, object>>(body);

                        int charCount = Convert.ToInt32(dict["character_count"]);
                        int charLimit = Convert.ToInt32(dict["character_limit"]);
                        string tier = dict.ContainsKey("tier") ? dict["tier"].ToString() : "free";
                        int rem = Math.Max(0, charLimit - charCount);

                        if (rem == 0)
                        {
                            return string.Format("Habis: 0 / {0:N0} Karakter (Tier {1} — {2} ms)", charLimit, tier, sw.ElapsedMilliseconds);
                        }
                        return string.Format("Sisa {0:N0} / {1:N0} Karakter (Tier {2} — {3} ms)", rem, charLimit, tier, sw.ElapsedMilliseconds);
                    }
                    else
                    {
                        return string.Format("HTTP {0} ({1} ms)", (int)resp.StatusCode, sw.ElapsedMilliseconds);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException || ex is System.Net.WebException || ex.Message.IndexOf("offline", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("name resolution", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("host", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Terputus (Offline / Masalah Jaringan)";
                }
                if (ex is TaskCanceledException || ex.Message.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Batas Waktu Habis (Timeout)";
                }
                return string.Format("Kendala: {0}", ex.Message);
            }
        }

        public static void ShowDialog(IWin32Window owner = null)
        {
            using (SmartApiVaultDialogForm form = new SmartApiVaultDialogForm())
            {
                form.ShowDialog(owner);
            }
        }
    }

    public class SmartApiVaultDialogForm : Form
    {
        private ListView _lvKeys;
        private Button _btnAdd;
        private Button _btnTest;
        private Button _btnDelete;
        private Button _btnClose;
        private List<StudioApiKeyItem> _items;

        public SmartApiVaultDialogForm()
        {
            this.Text = "Smart API Vault — Pengelola Token & Sekoci AI Mandiri";
            this.Size = new Size(880, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);
            this.Font = new Font("Segoe UI", 9f);

            InitializeComponents();
            LoadData();
        }

        private void InitializeComponents()
        {
            Panel pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 65;
            pnlHeader.BackColor = Color.FromArgb(9, 9, 11);
            pnlHeader.Padding = new Padding(20, 12, 20, 10);

            Label lblTitle = new Label();
            lblTitle.Text = "Smart API Vault";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 10);
            lblTitle.AutoSize = true;
            pnlHeader.Controls.Add(lblTitle);

            Label lblDesc = new Label();
            lblDesc.Text = "Kelola sekoci kunci API Google Gemini (Naskah) & ElevenLabs TTS (Vokal) dengan rotasi otomatis";
            lblDesc.Font = new Font("Segoe UI", 8.25f);
            lblDesc.ForeColor = Color.FromArgb(161, 161, 170);
            lblDesc.Location = new Point(20, 36);
            lblDesc.AutoSize = true;
            pnlHeader.Controls.Add(lblDesc);

            this.Controls.Add(pnlHeader);

            Panel pnlCenter = new Panel();
            pnlCenter.Dock = DockStyle.Fill;
            pnlCenter.Padding = new Padding(20);

            _lvKeys = new ListView();
            _lvKeys.Dock = DockStyle.Fill;
            _lvKeys.View = View.Details;
            _lvKeys.FullRowSelect = true;
            _lvKeys.GridLines = true;
            _lvKeys.CheckBoxes = true;
            _lvKeys.Font = new Font("Segoe UI", 9f);
            _lvKeys.BorderStyle = BorderStyle.FixedSingle;

            _lvKeys.Columns.Add("Provider", 110);
            _lvKeys.Columns.Add("Label Sekoci", 220);
            _lvKeys.Columns.Add("Kunci (Key)", 190);
            _lvKeys.Columns.Add("Status Kuota / Koneksi", 210);
            _lvKeys.Columns.Add("Terakhir Diuji", 110);

            _lvKeys.ItemChecked += delegate(object s, ItemCheckedEventArgs e)
            {
                if (e.Item.Tag is StudioApiKeyItem)
                {
                    StudioApiKeyItem itm = (StudioApiKeyItem)e.Item.Tag;
                    itm.is_active = e.Item.Checked;
                }
            };

            pnlCenter.Controls.Add(_lvKeys);
            this.Controls.Add(pnlCenter);
            pnlCenter.BringToFront();

            Panel pnlBottom = new Panel();
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Height = 60;
            pnlBottom.BackColor = Color.FromArgb(241, 245, 249);
            pnlBottom.Padding = new Padding(20, 10, 20, 10);

            _btnAdd = UIFactory.CreateButton("+ Tambah Kunci", 20, 12, 140, 34, UIFactory.AccentPrimary);
            _btnAdd.Click += delegate(object s, EventArgs e) { AddNewKey(); };
            pnlBottom.Controls.Add(_btnAdd);

            _btnTest = UIFactory.CreateButton("Cek Kuota & Status", 168, 12, 160, 34, UIFactory.AccentTeal);
            _btnTest.Click += async delegate(object s, EventArgs e) { await TestAllKeysAsync(); };
            pnlBottom.Controls.Add(_btnTest);

            _btnDelete = UIFactory.CreateButton("Hapus", 336, 12, 90, 34, Color.FromArgb(254, 242, 242), UIFactory.AccentRed);
            _btnDelete.Click += delegate(object s, EventArgs e) { DeleteSelectedKey(); };
            pnlBottom.Controls.Add(_btnDelete);

            _btnClose = UIFactory.CreateButton("Tutup & Simpan", 710, 12, 140, 34, UIFactory.AccentPrimary);
            _btnClose.Click += delegate(object s, EventArgs e)
            {
                SmartApiVault.SaveVault(_items);
                this.Close();
            };
            pnlBottom.Controls.Add(_btnClose);

            this.Controls.Add(pnlBottom);
        }

        private void LoadData()
        {
            _items = SmartApiVault.LoadVault();
            RefreshList();
        }

        private void RefreshList()
        {
            _lvKeys.Items.Clear();
            foreach (StudioApiKeyItem item in _items)
            {
                ListViewItem lvi = new ListViewItem(item.provider ?? "-");
                lvi.SubItems.Add(item.label ?? "-");

                string masked = MaskKey(item.key);
                lvi.SubItems.Add(masked);
                lvi.SubItems.Add(item.last_tested_status ?? "Belum diuji");
                lvi.SubItems.Add(item.last_tested_at ?? "-");
                lvi.Checked = item.is_active;
                lvi.Tag = item;
                _lvKeys.Items.Add(lvi);
            }
        }

        private string MaskKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";
            if (key.Length <= 8) return "••••••••";
            return key.Substring(0, 4) + "••••••••" + key.Substring(key.Length - 4);
        }

        private void AddNewKey()
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "Tambah Kunci API";
                dlg.Size = new Size(480, 280);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;

                Label lblProv = new Label { Text = "Provider:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
                ComboBox cbProv = new ComboBox { Location = new Point(20, 42), Size = new Size(420, 26), DropDownStyle = ComboBoxStyle.DropDownList };
                cbProv.Items.AddRange(new object[] { "Gemini", "ElevenLabs" });
                cbProv.SelectedIndex = 0;

                Label lblLbl = new Label { Text = "Label Sekoci:", Location = new Point(20, 78), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
                TextBox txtLbl = new TextBox { Location = new Point(20, 100), Size = new Size(420, 26), Text = "Sekoci Cadangan Baru" };

                Label lblKey = new Label { Text = "Kunci API:", Location = new Point(20, 136), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
                TextBox txtKey = new TextBox { Location = new Point(20, 158), Size = new Size(420, 26) };

                Button btnOk = UIFactory.CreateButton("Simpan", 240, 196, 95, 32, UIFactory.AccentPrimary);
                btnOk.Click += delegate(object s, EventArgs e)
                {
                    if (string.IsNullOrWhiteSpace(txtKey.Text))
                    {
                        MessageBox.Show("Kunci API tidak boleh kosong.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    StudioApiKeyItem itm = new StudioApiKeyItem();
                    itm.provider = cbProv.SelectedItem.ToString();
                    itm.label = txtLbl.Text.Trim();
                    itm.key = txtKey.Text.Trim();
                    itm.is_active = true;
                    _items.Add(itm);
                    SmartApiVault.SaveVault(_items);
                    RefreshList();
                    dlg.DialogResult = DialogResult.OK;
                    dlg.Close();
                };

                Button btnCancel = UIFactory.CreateButton("Batal", 345, 196, 95, 32, UIFactory.AccentPrimaryLight);
                btnCancel.Click += delegate(object s, EventArgs e) { dlg.Close(); };

                dlg.Controls.Add(lblProv);
                dlg.Controls.Add(cbProv);
                dlg.Controls.Add(lblLbl);
                dlg.Controls.Add(txtLbl);
                dlg.Controls.Add(lblKey);
                dlg.Controls.Add(txtKey);
                dlg.Controls.Add(btnOk);
                dlg.Controls.Add(btnCancel);

                dlg.ShowDialog(this);
            }
        }

        private async Task TestAllKeysAsync()
        {
            _btnTest.Enabled = false;
            _btnTest.Text = "Menguji...";

            try
            {
                foreach (StudioApiKeyItem itm in _items)
                {
                    if (!itm.is_active || string.IsNullOrEmpty(itm.key)) continue;

                    string status = "";
                    if (itm.provider == "Gemini")
                    {
                        status = await SmartApiVault.TestGeminiKeyAsync(itm.key);
                    }
                    else if (itm.provider == "ElevenLabs")
                    {
                        status = await SmartApiVault.TestElevenLabsKeyAsync(itm.key);
                    }

                    itm.last_tested_status = status;
                    itm.last_tested_at = DateTime.Now.ToString("HH:mm:ss");
                }

                SmartApiVault.SaveVault(_items);
                RefreshList();
                MessageBox.Show("Pengujian kuota dan koneksi seluruh kunci API selesai!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                _btnTest.Enabled = true;
                _btnTest.Text = "Cek Kuota & Status";
            }
        }

        private void DeleteSelectedKey()
        {
            if (_lvKeys.SelectedItems.Count > 0)
            {
                StudioApiKeyItem item = _lvKeys.SelectedItems[0].Tag as StudioApiKeyItem;
                if (item != null)
                {
                    if (MessageBox.Show("Apakah Anda yakin ingin menghapus kunci '" + item.label + "'?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _items.Remove(item);
                        SmartApiVault.SaveVault(_items);
                        RefreshList();
                    }
                }
            }
        }
    }
}
