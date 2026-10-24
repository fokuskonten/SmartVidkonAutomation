using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace SmartVidkonAutomation.Helpers
{
    public class StudioMediaFolderItem
    {
        public string id { get; set; }
        public string type { get; set; } // "Footage" | "Audio"
        public string name { get; set; }
        public string folder_path { get; set; }
        public bool is_active { get; set; }
        public int last_scan_files_count { get; set; }
        public long last_scan_size_bytes { get; set; }
        public string last_scanned_at { get; set; }

        public StudioMediaFolderItem()
        {
            id = Guid.NewGuid().ToString("N");
            is_active = true;
            last_scanned_at = "-";
        }
    }

    public class StudioMediaVaultData
    {
        public string version { get; set; }
        public string updated_at { get; set; }
        public List<StudioMediaFolderItem> folders { get; set; }

        public StudioMediaVaultData()
        {
            version = "1.0";
            folders = new List<StudioMediaFolderItem>();
        }
    }

    public static class SmartMediaVault
    {
        public static string VaultFilePath
        {
            get { return AppPaths.MediaVaultPath; }
        }

        private static List<StudioMediaFolderItem> _cache = null;

        public static List<StudioMediaFolderItem> GetDefaultItems()
        {
            // Koleksi media awal dikosongkan 100% agar pengguna dapat memilih dan mengkustomisasi direktori media sendiri
            return new List<StudioMediaFolderItem>();
        }

        public static List<StudioMediaFolderItem> LoadVault()
        {
            if (_cache != null) return _cache;

            string path = VaultFilePath;
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path, Encoding.UTF8);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    StudioMediaVaultData data = js.Deserialize<StudioMediaVaultData>(json);
                    if (data != null && data.folders != null)
                    {
                        List<StudioMediaFolderItem> normalized = new List<StudioMediaFolderItem>();
                        foreach (StudioMediaFolderItem item in data.folders)
                        {
                            if (item == null) continue;

                            // Normalisasi path relatif jika ada
                            if (!string.IsNullOrEmpty(item.folder_path))
                            {
                                if (item.folder_path.StartsWith("./") || item.folder_path.StartsWith(".\\"))
                                {
                                    item.folder_path = Path.GetFullPath(Path.Combine(AppPaths.AppDir, item.folder_path.Substring(2)));
                                }
                            }

                            // Hanya pertahankan folder kustom pengguna yang benar-benar ada di disk
                            if (!string.IsNullOrEmpty(item.folder_path) && Directory.Exists(item.folder_path))
                            {
                                normalized.Add(item);
                            }
                        }

                        _cache = normalized;
                        return _cache;
                    }
                }
                catch { }
            }

            _cache = GetDefaultItems();
            return _cache;
        }

        public static void SaveVault(List<StudioMediaFolderItem> items)
        {
            try
            {
                _cache = items;
                string path = VaultFilePath;
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                StudioMediaVaultData data = new StudioMediaVaultData();
                data.version = "1.0";
                data.updated_at = DateTime.UtcNow.ToString("o");
                data.folders = items;

                JavaScriptSerializer js = new JavaScriptSerializer();
                string json = js.Serialize(data);

                // Format JSON sederhana
                json = json.Replace("{\"", "{\n  \"")
                           .Replace(",\"", ",\n  \"")
                           .Replace(":[{", ":[\n    {")
                           .Replace("}]", "\n  }]");

                File.WriteAllText(path, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Gagal menyimpan database koleksi media: " + ex.Message);
            }
        }

        public static List<StudioMediaFolderItem> GetFolders(string type)
        {
            List<StudioMediaFolderItem> all = LoadVault();
            List<StudioMediaFolderItem> res = new List<StudioMediaFolderItem>();
            foreach (StudioMediaFolderItem item in all)
            {
                if (item == null) continue;
                if (string.Equals(item.type, type, StringComparison.OrdinalIgnoreCase))
                {
                    res.Add(item);
                }
                else if (type == "FootageVertical" && string.Equals(item.type, "Footage", StringComparison.OrdinalIgnoreCase))
                {
                    res.Add(item);
                }
            }
            return res;
        }

        public static StudioMediaFolderItem AddFolderDirect(string type, string folderPath, string customName = null)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) return null;

            List<StudioMediaFolderItem> items = LoadVault();

            foreach (StudioMediaFolderItem existing in items)
            {
                if (string.Equals(existing.folder_path, folderPath, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(existing.type, type, StringComparison.OrdinalIgnoreCase))
                {
                    ScanFolderItem(existing);
                    SaveVault(items);
                    return existing;
                }
            }

            string name = customName;
            if (string.IsNullOrEmpty(name))
            {
                name = Path.GetFileName(folderPath.TrimEnd('\\', '/'));
                if (string.IsNullOrEmpty(name)) name = folderPath;
            }

            StudioMediaFolderItem newItem = new StudioMediaFolderItem
            {
                name = name,
                folder_path = folderPath,
                type = type,
                is_active = true
            };

            ScanFolderItem(newItem);

            items.Add(newItem);
            SaveVault(items);
            return newItem;
        }

        public static bool RemoveFolder(string folderPath, string type)
        {
            List<StudioMediaFolderItem> items = LoadVault();
            int removed = items.RemoveAll(x =>
                string.Equals(x.folder_path, folderPath, StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrEmpty(type) || string.Equals(x.type, type, StringComparison.OrdinalIgnoreCase)));
            if (removed > 0)
            {
                SaveVault(items);
                return true;
            }
            return false;
        }

        public static void ScanFolderItem(StudioMediaFolderItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.folder_path) || !Directory.Exists(item.folder_path))
            {
                if (item != null) item.last_scanned_at = "Folder Tidak Ditemukan";
                return;
            }

            int count = 0;
            long bytes = 0;
            string[] exts = (item.type == "Audio")
                ? new string[] { ".mp3", ".wav", ".aac", ".m4a", ".flac", ".ogg" }
                : new string[] { ".mp4", ".mov", ".mkv", ".avi", ".webm", ".m4v" };

            try
            {
                string[] files = Directory.GetFiles(item.folder_path, "*.*", SearchOption.TopDirectoryOnly);
                foreach (string f in files)
                {
                    string ext = Path.GetExtension(f).ToLowerInvariant();
                    foreach (string validExt in exts)
                    {
                        if (ext == validExt)
                        {
                            count++;
                            try { bytes += new FileInfo(f).Length; } catch { }
                            break;
                        }
                    }
                }
            }
            catch { }

            item.last_scan_files_count = count;
            item.last_scan_size_bytes = bytes;
            item.last_scanned_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        public class MediaFileDetailItem
        {
            public string FileName { get; set; }
            public string Extension { get; set; }
            public long SizeBytes { get; set; }
            public DateTime LastModified { get; set; }
            public string FullPath { get; set; }
        }

        public static List<MediaFileDetailItem> GetFilesInFolder(string folderPath, string type)
        {
            List<MediaFileDetailItem> res = new List<MediaFileDetailItem>();
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) return res;

            string[] exts = (type == "Audio")
                ? new string[] { ".mp3", ".wav", ".aac", ".m4a", ".flac", ".ogg" }
                : new string[] { ".mp4", ".mov", ".mkv", ".avi", ".webm", ".m4v" };

            try
            {
                string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);
                Array.Sort(files);
                foreach (string f in files)
                {
                    string ext = Path.GetExtension(f).ToLowerInvariant();
                    bool isMatch = false;
                    foreach (string validExt in exts)
                    {
                        if (ext == validExt) { isMatch = true; break; }
                    }
                    if (isMatch)
                    {
                        FileInfo fi = new FileInfo(f);
                        res.Add(new MediaFileDetailItem
                        {
                            FileName = fi.Name,
                            Extension = fi.Extension.ToUpperInvariant(),
                            SizeBytes = fi.Length,
                            LastModified = fi.LastWriteTime,
                            FullPath = fi.FullName
                        });
                    }
                }
            }
            catch { }

            return res;
        }

        public static void ShowDialog(IWin32Window owner = null, int initialTab = 0)
        {
            using (SmartMediaVaultDialogForm form = new SmartMediaVaultDialogForm(initialTab))
            {
                form.ShowDialog(owner);
            }
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            double kb = bytes / 1024.0;
            if (kb < 1024) return kb.ToString("0.0") + " KB";
            double mb = kb / 1024.0;
            if (mb < 1024) return mb.ToString("0.0") + " MB";
            double gb = mb / 1024.0;
            return gb.ToString("0.00") + " GB";
        }
    }

    public class SmartMediaVaultDialogForm : Form
    {
        private ListView _lvFolders;
        private Button _btnAdd;
        private Button _btnScan;
        private Button _btnOpen;
        private Button _btnDelete;
        private Button _btnClose;
        private List<StudioMediaFolderItem> _items;

        public SmartMediaVaultDialogForm(int initialFilterIndex = 0)
        {
            this.Text = "Smart Media Vault — Koleksi Footage & Audio Video Studio";
            this.Size = new Size(880, 540);
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
            lblTitle.Text = "Smart Media Vault";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 10);
            lblTitle.AutoSize = true;
            pnlHeader.Controls.Add(lblTitle);

            Label lblDesc = new Label();
            lblDesc.Text = "Kelola folder koleksi video footage (MP4/MOV) & pustaka audio BGM resmi";
            lblDesc.Font = new Font("Segoe UI", 8.25f);
            lblDesc.ForeColor = Color.FromArgb(161, 161, 170);
            lblDesc.Location = new Point(20, 36);
            lblDesc.AutoSize = true;
            pnlHeader.Controls.Add(lblDesc);

            this.Controls.Add(pnlHeader);

            // Center Panel
            Panel pnlCenter = new Panel();
            pnlCenter.Dock = DockStyle.Fill;
            pnlCenter.Padding = new Padding(20);

            _lvFolders = new ListView();
            _lvFolders.Dock = DockStyle.Fill;
            _lvFolders.View = View.Details;
            _lvFolders.FullRowSelect = true;
            _lvFolders.GridLines = true;
            _lvFolders.CheckBoxes = true;
            _lvFolders.Font = new Font("Segoe UI", 9f);
            _lvFolders.BorderStyle = BorderStyle.FixedSingle;

            _lvFolders.Columns.Add("Tipe", 80);
            _lvFolders.Columns.Add("Nama Koleksi Media", 230);
            _lvFolders.Columns.Add("Lokasi Folder", 260);
            _lvFolders.Columns.Add("Jumlah Berkas", 100);
            _lvFolders.Columns.Add("Ukuran", 90);
            _lvFolders.Columns.Add("Terakhir Dipindai", 120);

            _lvFolders.ItemChecked += delegate(object s, ItemCheckedEventArgs e)
            {
                if (e.Item.Tag is StudioMediaFolderItem)
                {
                    StudioMediaFolderItem itm = (StudioMediaFolderItem)e.Item.Tag;
                    itm.is_active = e.Item.Checked;
                }
            };

            pnlCenter.Controls.Add(_lvFolders);
            this.Controls.Add(pnlCenter);
            pnlCenter.BringToFront();

            // Bottom Buttons Panel
            Panel pnlBottom = new Panel();
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Height = 60;
            pnlBottom.BackColor = Color.FromArgb(241, 245, 249);
            pnlBottom.Padding = new Padding(20, 10, 20, 10);

            _btnAdd = UIFactory.CreateButton("+ Tambah Folder", 20, 12, 140, 34, UIFactory.AccentPrimary);
            _btnAdd.Click += delegate(object s, EventArgs e) { AddNewFolder(); };
            pnlBottom.Controls.Add(_btnAdd);

            _btnScan = UIFactory.CreateButton("Pindai Media", 168, 12, 140, 34, UIFactory.AccentTeal);
            _btnScan.Click += async delegate(object s, EventArgs e) { await ScanAllFoldersAsync(); };
            pnlBottom.Controls.Add(_btnScan);

            _btnOpen = UIFactory.CreateButton("Buka Folder", 316, 12, 110, 34, UIFactory.AccentPrimaryLight);
            _btnOpen.Click += delegate(object s, EventArgs e) { OpenSelectedFolder(); };
            pnlBottom.Controls.Add(_btnOpen);

            _btnDelete = UIFactory.CreateButton("Hapus", 434, 12, 90, 34, Color.FromArgb(254, 242, 242), UIFactory.AccentRed);
            _btnDelete.Click += delegate(object s, EventArgs e) { DeleteSelectedFolder(); };
            pnlBottom.Controls.Add(_btnDelete);

            _btnClose = UIFactory.CreateButton("Tutup & Simpan", 710, 12, 140, 34, UIFactory.AccentPrimary);
            _btnClose.Click += delegate(object s, EventArgs e)
            {
                SmartMediaVault.SaveVault(_items);
                this.Close();
            };
            pnlBottom.Controls.Add(_btnClose);

            this.Controls.Add(pnlBottom);
        }

        private void LoadData()
        {
            _items = SmartMediaVault.LoadVault();
            RefreshList();
        }

        private void RefreshList()
        {
            _lvFolders.Items.Clear();
            foreach (StudioMediaFolderItem item in _items)
            {
                ListViewItem lvi = new ListViewItem(item.type ?? "Footage");
                lvi.SubItems.Add(item.name ?? "-");
                lvi.SubItems.Add(item.folder_path ?? "-");
                lvi.SubItems.Add(item.last_scan_files_count.ToString("#,##0"));
                lvi.SubItems.Add(SmartMediaVault.FormatBytes(item.last_scan_size_bytes));
                lvi.SubItems.Add(item.last_scanned_at ?? "-");
                lvi.Checked = item.is_active;
                lvi.Tag = item;
                _lvFolders.Items.Add(lvi);
            }
        }

        private void AddNewFolder()
        {
            ModernFolderPicker picker = new ModernFolderPicker();
            picker.Title = "Pilih Folder Koleksi Footage atau Pustaka Audio BGM";
            picker.OkButtonLabel = "Pilih Folder";

            if (picker.ShowDialog(this) == DialogResult.OK)
            {
                string path = picker.SelectedPath;
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    string name = Path.GetFileName(path);
                    if (string.IsNullOrEmpty(name)) name = path;

                    string type = "Footage";
                    if (MessageBox.Show("Apakah folder ini merupakan pustaka Audio BGM?\n(Pilih 'Yes' untuk Audio, 'No' untuk Footage Video)", "Tipe Koleksi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        type = "Audio";
                    }

                    StudioMediaFolderItem itm = new StudioMediaFolderItem();
                    itm.name = name;
                    itm.folder_path = path;
                    itm.type = type;
                    itm.is_active = true;
                    _items.Add(itm);
                    SmartMediaVault.SaveVault(_items);
                    RefreshList();
                }
            }
        }

        private async Task ScanAllFoldersAsync()
        {
            _btnScan.Enabled = false;
            _btnScan.Text = "Memindai...";

            try
            {
                await Task.Run(new Action(delegate
                {
                    foreach (StudioMediaFolderItem item in _items)
                    {
                        if (string.IsNullOrEmpty(item.folder_path) || !Directory.Exists(item.folder_path))
                        {
                            item.last_scanned_at = "Folder Tidak Ditemukan";
                            continue;
                        }

                        int count = 0;
                        long bytes = 0;

                        try
                        {
                            string[] exts = (item.type == "Audio") 
                                ? new string[] { ".mp3", ".wav", ".aac", ".m4a", ".flac", ".ogg" } 
                                : new string[] { ".mp4", ".mov", ".mkv", ".avi", ".webm" };

                            string[] files = Directory.GetFiles(item.folder_path, "*.*", SearchOption.AllDirectories);
                            foreach (string f in files)
                            {
                                string ext = Path.GetExtension(f).ToLowerInvariant();
                                foreach (string validExt in exts)
                                {
                                    if (ext == validExt)
                                    {
                                        count++;
                                        try { bytes += new FileInfo(f).Length; } catch { }
                                        break;
                                    }
                                }
                            }

                            item.last_scan_files_count = count;
                            item.last_scan_size_bytes = bytes;
                            item.last_scanned_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                        }
                        catch { }
                    }
                }));

                SmartMediaVault.SaveVault(_items);
                RefreshList();
                MessageBox.Show("Pemindaian seluruh koleksi media selesai!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                _btnScan.Enabled = true;
                _btnScan.Text = "Pindai Media";
            }
        }

        private void OpenSelectedFolder()
        {
            if (_lvFolders.SelectedItems.Count > 0)
            {
                StudioMediaFolderItem item = _lvFolders.SelectedItems[0].Tag as StudioMediaFolderItem;
                if (item != null && !string.IsNullOrEmpty(item.folder_path))
                {
                    if (Directory.Exists(item.folder_path))
                    {
                        Process.Start("explorer.exe", item.folder_path);
                    }
                    else
                    {
                        MessageBox.Show("Folder tidak ditemukan: " + item.folder_path, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void DeleteSelectedFolder()
        {
            if (_lvFolders.SelectedItems.Count > 0)
            {
                StudioMediaFolderItem item = _lvFolders.SelectedItems[0].Tag as StudioMediaFolderItem;
                if (item != null)
                {
                    if (MessageBox.Show("Apakah Anda yakin ingin menghapus koleksi '" + item.name + "' dari daftar vault?\n(File fisik di disk tidak akan terhapus)", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _items.Remove(item);
                        SmartMediaVault.SaveVault(_items);
                        RefreshList();
                    }
                }
            }
        }
    }
}
