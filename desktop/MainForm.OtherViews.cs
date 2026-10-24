using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartVidkonAutomation.Helpers;

namespace SmartVidkonAutomation
{
    public partial class MainForm
    {
        // ═════════════════════════════════════════════════════════════════════
        // ═════════════════════════════════════════════════════════════════════
        // ═════════════════════════════════════════════════════════════════════
        // VIEW 2A: FOOTAGE VERTIKAL (9:16)
        // ═════════════════════════════════════════════════════════════════════
        private StudioMediaFolderItem _selectedVerticalFolder;
        private Panel _pnlVerticalFolderList;
        private Label _lblVerticalFolderCount;
        private Label _lblVerticalGalleryTitle;
        private Label _lblVerticalGallerySubtitle;
        private FlowLayoutPanel _flpVerticalMediaGrid;

        private void InitializeFootageVerticalView()
        {
            EnsureVideoComponents();

            _viewFootageVertical = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };
            _pnlViewContainer.Controls.Add(_viewFootageVertical);

            RoundedCardPanel card = UIFactory.CreateCard(0, 0, _pnlViewContainer.ClientSize.Width - 32, _pnlViewContainer.ClientSize.Height - 32, 12);
            card.Dock = DockStyle.Fill;
            _viewFootageVertical.Controls.Add(card);

            Label lblTitle = new Label { Text = "Footage Vertikal (9:16)", UseMnemonic = false, Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = UIFactory.TextPrimary, Location = new Point(16, 12), AutoSize = true };
            card.Controls.Add(lblTitle);

            Label lblSub = new Label { Text = "Pusat koleksi video footage vertikal (format potret 9:16) untuk TikTok, Shorts, dan Reels. Pilih folder di sebelah kiri untuk melihat klip.", Font = new Font("Segoe UI", 8.5f), ForeColor = UIFactory.TextSecondary, Location = new Point(16, 36), AutoSize = true };
            card.Controls.Add(lblSub);

            // Action Buttons
            RoundedButton btnAdd = UIFactory.CreateButton("+ Tambah Folder", 16, 62, 140, 32, UIFactory.AccentPrimary, 6);
            btnAdd.Click += delegate(object s, EventArgs e) { AddFolderForType("FootageVertical", RefreshFootageVerticalViewData); };
            card.Controls.Add(btnAdd);

            RoundedButton btnOpen = UIFactory.CreateButton("Buka di Explorer", 162, 62, 130, 32, UIFactory.AccentPrimaryLight, 6);
            btnOpen.Click += delegate(object s, EventArgs e) { OpenActiveFolder(_selectedVerticalFolder); };
            card.Controls.Add(btnOpen);

            RoundedButton btnRefresh = UIFactory.CreateButton("Segarkan", 298, 62, 90, 32, UIFactory.BgSubtle, 6);
            btnRefresh.Click += delegate(object s, EventArgs e) { RefreshFootageVerticalViewData(); };
            card.Controls.Add(btnRefresh);

            RoundedButton btnDelete = UIFactory.CreateButton("Hapus dari Daftar", 394, 62, 140, 32, Color.FromArgb(254, 242, 242), UIFactory.AccentRed, 6);
            btnDelete.Click += delegate(object s, EventArgs e) { DeleteActiveFolder(_selectedVerticalFolder, "FootageVertical", RefreshFootageVerticalViewData); };
            card.Controls.Add(btnDelete);

            // Split Layout Container
            Panel pnlSplit = new Panel
            {
                Location = new Point(16, 102),
                Size = new Size(card.Width - 32, Math.Max(200, card.Height - 118)),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            card.Controls.Add(pnlSplit);

            // Left Rail: Direktori Folder Bins
            Panel pnlLeftRail = new Panel
            {
                Width = 260,
                Dock = DockStyle.Left,
                BackColor = Color.White,
                Padding = new Padding(0, 0, 10, 0)
            };
            pnlSplit.Controls.Add(pnlLeftRail);

            Panel pnlRailHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.White
            };
            Label lblRailTitle = new Label
            {
                Text = "Direktori Folder Koleksi",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = UIFactory.TextPrimary,
                Location = new Point(0, 6),
                AutoSize = true
            };
            pnlRailHeader.Controls.Add(lblRailTitle);

            _lblVerticalFolderCount = new Label
            {
                Text = "(0 folder)",
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = UIFactory.TextMuted,
                Location = new Point(148, 7),
                AutoSize = true
            };
            pnlRailHeader.Controls.Add(_lblVerticalFolderCount);
            pnlLeftRail.Controls.Add(pnlRailHeader);

            _pnlVerticalFolderList = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(0, 4, 4, 4)
            };
            pnlLeftRail.Controls.Add(_pnlVerticalFolderList);
            _pnlVerticalFolderList.BringToFront();

            // Divider Line
            Panel pnlDivider = new Panel
            {
                Width = 1,
                Dock = DockStyle.Left,
                BackColor = UIFactory.BorderColor
            };
            pnlSplit.Controls.Add(pnlDivider);
            pnlDivider.BringToFront();

            // Right Gallery: Fluid Video Cards Grid
            Panel pnlRightGallery = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIFactory.BgCanvas,
                Padding = new Padding(12, 0, 0, 0)
            };
            pnlSplit.Controls.Add(pnlRightGallery);
            pnlRightGallery.BringToFront();

            Panel pnlGalleryHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.White,
                Padding = new Padding(8, 4, 8, 4)
            };
            pnlGalleryHeader.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen p = new Pen(UIFactory.BorderColor, 1f))
                {
                    e.Graphics.DrawLine(p, 0, pnlGalleryHeader.Height - 1, pnlGalleryHeader.Width, pnlGalleryHeader.Height - 1);
                }
            };

            _lblVerticalGalleryTitle = new Label
            {
                Text = "Pilih folder di sebelah kiri untuk melihat isi klip",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = UIFactory.TextPrimary,
                Location = new Point(8, 4),
                AutoSize = true
            };
            pnlGalleryHeader.Controls.Add(_lblVerticalGalleryTitle);

            _lblVerticalGallerySubtitle = new Label
            {
                Text = "-",
                Font = new Font("Segoe UI", 8f),
                ForeColor = UIFactory.TextMuted,
                Location = new Point(8, 24),
                Size = new Size(500, 16),
                AutoEllipsis = true
            };
            pnlGalleryHeader.Controls.Add(_lblVerticalGallerySubtitle);
            pnlRightGallery.Controls.Add(pnlGalleryHeader);

            _flpVerticalMediaGrid = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                BackColor = UIFactory.BgCanvas,
                Padding = new Padding(8)
            };
            pnlRightGallery.Controls.Add(_flpVerticalMediaGrid);
            _flpVerticalMediaGrid.BringToFront();
        }

        private void RefreshFootageVerticalViewData()
        {
            List<StudioMediaFolderItem> folders = SmartMediaVault.GetFolders("FootageVertical");

            if (_selectedVerticalFolder != null)
            {
                bool stillExists = false;
                foreach (StudioMediaFolderItem f in folders)
                {
                    if (string.Equals(f.folder_path, _selectedVerticalFolder.folder_path, StringComparison.OrdinalIgnoreCase))
                    {
                        _selectedVerticalFolder = f;
                        stillExists = true;
                        break;
                    }
                }
                if (!stillExists) _selectedVerticalFolder = (folders.Count > 0) ? folders[0] : null;
            }
            else if (folders.Count > 0)
            {
                _selectedVerticalFolder = folders[0];
            }

            RenderFolderList(_pnlVerticalFolderList, _lblVerticalFolderCount, "FootageVertical", _selectedVerticalFolder, delegate(StudioMediaFolderItem clicked)
            {
                _selectedVerticalFolder = clicked;
                LoadFootageVerticalGrid();
                RenderFolderList(_pnlVerticalFolderList, _lblVerticalFolderCount, "FootageVertical", _selectedVerticalFolder, null);
            });

            LoadFootageVerticalGrid();
        }

        private void LoadFootageVerticalGrid()
        {
            if (_selectedVerticalFolder == null || string.IsNullOrEmpty(_selectedVerticalFolder.folder_path) || !Directory.Exists(_selectedVerticalFolder.folder_path))
            {
                if (_lblVerticalGalleryTitle != null) _lblVerticalGalleryTitle.Text = "Belum Ada Folder Terpilih";
                if (_lblVerticalGallerySubtitle != null) _lblVerticalGallerySubtitle.Text = "Tambahkan atau pilih folder di sebelah kiri untuk melihat klip video";
                if (_flpVerticalMediaGrid != null) RenderMediaCards(_flpVerticalMediaGrid, new List<SmartMediaVault.MediaFileDetailItem>(), true);
                return;
            }

            List<SmartMediaVault.MediaFileDetailItem> files = SmartMediaVault.GetFilesInFolder(_selectedVerticalFolder.folder_path, "FootageVertical");
            if (_lblVerticalGalleryTitle != null)
            {
                _lblVerticalGalleryTitle.Text = string.Format("Folder: {0} ({1} Klip Video, {2})", _selectedVerticalFolder.name, files.Count, SmartMediaVault.FormatBytes(_selectedVerticalFolder.last_scan_size_bytes));
            }
            if (_lblVerticalGallerySubtitle != null)
            {
                _lblVerticalGallerySubtitle.Text = _selectedVerticalFolder.folder_path;
            }
            if (_flpVerticalMediaGrid != null)
            {
                RenderMediaCards(_flpVerticalMediaGrid, files, true);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // VIEW 2B: FOOTAGE LANDSCAPE (16:9)
        // ═════════════════════════════════════════════════════════════════════
        private StudioMediaFolderItem _selectedLandscapeFolder;
        private Panel _pnlLandscapeFolderList;
        private Label _lblLandscapeFolderCount;
        private Label _lblLandscapeGalleryTitle;
        private Label _lblLandscapeGallerySubtitle;
        private FlowLayoutPanel _flpLandscapeMediaGrid;

        private void InitializeFootageLandscapeView()
        {
            EnsureVideoComponents();

            _viewFootageLandscape = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };
            _pnlViewContainer.Controls.Add(_viewFootageLandscape);

            RoundedCardPanel card = UIFactory.CreateCard(0, 0, _pnlViewContainer.ClientSize.Width - 32, _pnlViewContainer.ClientSize.Height - 32, 12);
            card.Dock = DockStyle.Fill;
            _viewFootageLandscape.Controls.Add(card);

            Label lblTitle = new Label { Text = "Footage Landscape (16:9)", UseMnemonic = false, Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = UIFactory.TextPrimary, Location = new Point(16, 12), AutoSize = true };
            card.Controls.Add(lblTitle);

            Label lblSub = new Label { Text = "Pusat koleksi video footage landscape (format horizontal 16:9) untuk video layar lebar dan YouTube. Pilih folder di sebelah kiri untuk melihat klip.", Font = new Font("Segoe UI", 8.5f), ForeColor = UIFactory.TextSecondary, Location = new Point(16, 36), AutoSize = true };
            card.Controls.Add(lblSub);

            // Action Buttons
            RoundedButton btnAdd = UIFactory.CreateButton("+ Tambah Folder", 16, 62, 140, 32, UIFactory.AccentPrimary, 6);
            btnAdd.Click += delegate(object s, EventArgs e) { AddFolderForType("FootageLandscape", RefreshFootageLandscapeViewData); };
            card.Controls.Add(btnAdd);

            RoundedButton btnOpen = UIFactory.CreateButton("Buka di Explorer", 162, 62, 130, 32, UIFactory.AccentPrimaryLight, 6);
            btnOpen.Click += delegate(object s, EventArgs e) { OpenActiveFolder(_selectedLandscapeFolder); };
            card.Controls.Add(btnOpen);

            RoundedButton btnRefresh = UIFactory.CreateButton("Segarkan", 298, 62, 90, 32, UIFactory.BgSubtle, 6);
            btnRefresh.Click += delegate(object s, EventArgs e) { RefreshFootageLandscapeViewData(); };
            card.Controls.Add(btnRefresh);

            RoundedButton btnDelete = UIFactory.CreateButton("Hapus dari Daftar", 394, 62, 140, 32, Color.FromArgb(254, 242, 242), UIFactory.AccentRed, 6);
            btnDelete.Click += delegate(object s, EventArgs e) { DeleteActiveFolder(_selectedLandscapeFolder, "FootageLandscape", RefreshFootageLandscapeViewData); };
            card.Controls.Add(btnDelete);

            // Split Layout Container
            Panel pnlSplit = new Panel
            {
                Location = new Point(16, 102),
                Size = new Size(card.Width - 32, Math.Max(200, card.Height - 118)),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            card.Controls.Add(pnlSplit);

            // Left Rail: Direktori Folder Bins
            Panel pnlLeftRail = new Panel
            {
                Width = 260,
                Dock = DockStyle.Left,
                BackColor = Color.White,
                Padding = new Padding(0, 0, 10, 0)
            };
            pnlSplit.Controls.Add(pnlLeftRail);

            Panel pnlRailHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.White
            };
            Label lblRailTitle = new Label
            {
                Text = "Direktori Folder Koleksi",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = UIFactory.TextPrimary,
                Location = new Point(0, 6),
                AutoSize = true
            };
            pnlRailHeader.Controls.Add(lblRailTitle);

            _lblLandscapeFolderCount = new Label
            {
                Text = "(0 folder)",
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = UIFactory.TextMuted,
                Location = new Point(148, 7),
                AutoSize = true
            };
            pnlRailHeader.Controls.Add(_lblLandscapeFolderCount);
            pnlLeftRail.Controls.Add(pnlRailHeader);

            _pnlLandscapeFolderList = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(0, 4, 4, 4)
            };
            pnlLeftRail.Controls.Add(_pnlLandscapeFolderList);
            _pnlLandscapeFolderList.BringToFront();

            // Divider Line
            Panel pnlDivider = new Panel
            {
                Width = 1,
                Dock = DockStyle.Left,
                BackColor = UIFactory.BorderColor
            };
            pnlSplit.Controls.Add(pnlDivider);
            pnlDivider.BringToFront();

            // Right Gallery: Fluid Video Cards Grid
            Panel pnlRightGallery = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIFactory.BgCanvas,
                Padding = new Padding(12, 0, 0, 0)
            };
            pnlSplit.Controls.Add(pnlRightGallery);
            pnlRightGallery.BringToFront();

            Panel pnlGalleryHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.White,
                Padding = new Padding(8, 4, 8, 4)
            };
            pnlGalleryHeader.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen p = new Pen(UIFactory.BorderColor, 1f))
                {
                    e.Graphics.DrawLine(p, 0, pnlGalleryHeader.Height - 1, pnlGalleryHeader.Width, pnlGalleryHeader.Height - 1);
                }
            };

            _lblLandscapeGalleryTitle = new Label
            {
                Text = "Pilih folder di sebelah kiri untuk melihat isi klip",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = UIFactory.TextPrimary,
                Location = new Point(8, 4),
                AutoSize = true
            };
            pnlGalleryHeader.Controls.Add(_lblLandscapeGalleryTitle);

            _lblLandscapeGallerySubtitle = new Label
            {
                Text = "-",
                Font = new Font("Segoe UI", 8f),
                ForeColor = UIFactory.TextMuted,
                Location = new Point(8, 24),
                Size = new Size(500, 16),
                AutoEllipsis = true
            };
            pnlGalleryHeader.Controls.Add(_lblLandscapeGallerySubtitle);
            pnlRightGallery.Controls.Add(pnlGalleryHeader);

            _flpLandscapeMediaGrid = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                BackColor = UIFactory.BgCanvas,
                Padding = new Padding(8)
            };
            pnlRightGallery.Controls.Add(_flpLandscapeMediaGrid);
            _flpLandscapeMediaGrid.BringToFront();
        }

        private void RefreshFootageLandscapeViewData()
        {
            List<StudioMediaFolderItem> folders = SmartMediaVault.GetFolders("FootageLandscape");

            if (_selectedLandscapeFolder != null)
            {
                bool stillExists = false;
                foreach (StudioMediaFolderItem f in folders)
                {
                    if (string.Equals(f.folder_path, _selectedLandscapeFolder.folder_path, StringComparison.OrdinalIgnoreCase))
                    {
                        _selectedLandscapeFolder = f;
                        stillExists = true;
                        break;
                    }
                }
                if (!stillExists) _selectedLandscapeFolder = (folders.Count > 0) ? folders[0] : null;
            }
            else if (folders.Count > 0)
            {
                _selectedLandscapeFolder = folders[0];
            }

            RenderFolderList(_pnlLandscapeFolderList, _lblLandscapeFolderCount, "FootageLandscape", _selectedLandscapeFolder, delegate(StudioMediaFolderItem clicked)
            {
                _selectedLandscapeFolder = clicked;
                LoadFootageLandscapeGrid();
                RenderFolderList(_pnlLandscapeFolderList, _lblLandscapeFolderCount, "FootageLandscape", _selectedLandscapeFolder, null);
            });

            LoadFootageLandscapeGrid();
        }

        private void LoadFootageLandscapeGrid()
        {
            if (_selectedLandscapeFolder == null || string.IsNullOrEmpty(_selectedLandscapeFolder.folder_path) || !Directory.Exists(_selectedLandscapeFolder.folder_path))
            {
                if (_lblLandscapeGalleryTitle != null) _lblLandscapeGalleryTitle.Text = "Belum Ada Folder Terpilih";
                if (_lblLandscapeGallerySubtitle != null) _lblLandscapeGallerySubtitle.Text = "Tambahkan atau pilih folder di sebelah kiri untuk melihat klip video";
                if (_flpLandscapeMediaGrid != null) RenderMediaCards(_flpLandscapeMediaGrid, new List<SmartMediaVault.MediaFileDetailItem>(), false);
                return;
            }

            List<SmartMediaVault.MediaFileDetailItem> files = SmartMediaVault.GetFilesInFolder(_selectedLandscapeFolder.folder_path, "FootageLandscape");
            if (_lblLandscapeGalleryTitle != null)
            {
                _lblLandscapeGalleryTitle.Text = string.Format("Folder: {0} ({1} Klip Video, {2})", _selectedLandscapeFolder.name, files.Count, SmartMediaVault.FormatBytes(_selectedLandscapeFolder.last_scan_size_bytes));
            }
            if (_lblLandscapeGallerySubtitle != null)
            {
                _lblLandscapeGallerySubtitle.Text = _selectedLandscapeFolder.folder_path;
            }
            if (_flpLandscapeMediaGrid != null)
            {
                RenderMediaCards(_flpLandscapeMediaGrid, files, false);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // SHARED HELPER UNTUK VIEW FOOTAGE (VERTIKAL & LANDSCAPE)
        // ═════════════════════════════════════════════════════════════════════
        private ContextMenuStrip _videoContextMenu;
        private ToolTip _mediaToolTip;

        private void EnsureVideoComponents()
        {
            if (_mediaToolTip == null)
            {
                _mediaToolTip = new ToolTip();
                _mediaToolTip.AutoPopDelay = 8000;
                _mediaToolTip.InitialDelay = 400;
                _mediaToolTip.ReshowDelay = 200;
            }

            if (_videoContextMenu == null)
            {
                _videoContextMenu = new ContextMenuStrip();
                _videoContextMenu.Font = new Font("Segoe UI", 9f);

                ToolStripMenuItem mnuPlay = new ToolStripMenuItem("Putar Video (Default Player)");
                mnuPlay.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                mnuPlay.Click += delegate(object s, EventArgs e)
                {
                    SmartMediaVault.MediaFileDetailItem itm = _videoContextMenu.Tag as SmartMediaVault.MediaFileDetailItem;
                    if (itm != null && File.Exists(itm.FullPath))
                    {
                        try { Process.Start(itm.FullPath); } catch { }
                    }
                };
                _videoContextMenu.Items.Add(mnuPlay);

                ToolStripMenuItem mnuExplorer = new ToolStripMenuItem("Buka Lokasi di File Explorer");
                mnuExplorer.Click += delegate(object s, EventArgs e)
                {
                    SmartMediaVault.MediaFileDetailItem itm = _videoContextMenu.Tag as SmartMediaVault.MediaFileDetailItem;
                    if (itm != null && File.Exists(itm.FullPath))
                    {
                        try { Process.Start("explorer.exe", "/select,\"" + itm.FullPath + "\""); } catch { }
                    }
                };
                _videoContextMenu.Items.Add(mnuExplorer);

                _videoContextMenu.Items.Add(new ToolStripSeparator());

                ToolStripMenuItem mnuCopyName = new ToolStripMenuItem("Salin Nama Berkas");
                mnuCopyName.Click += delegate(object s, EventArgs e)
                {
                    SmartMediaVault.MediaFileDetailItem itm = _videoContextMenu.Tag as SmartMediaVault.MediaFileDetailItem;
                    if (itm != null)
                    {
                        try { Clipboard.SetText(itm.FileName); } catch { }
                    }
                };
                _videoContextMenu.Items.Add(mnuCopyName);

                ToolStripMenuItem mnuCopyPath = new ToolStripMenuItem("Salin Jalur Lengkap Berkas");
                mnuCopyPath.Click += delegate(object s, EventArgs e)
                {
                    SmartMediaVault.MediaFileDetailItem itm = _videoContextMenu.Tag as SmartMediaVault.MediaFileDetailItem;
                    if (itm != null)
                    {
                        try { Clipboard.SetText(itm.FullPath); } catch { }
                    }
                };
                _videoContextMenu.Items.Add(mnuCopyPath);
            }
        }

        private void RenderFolderList(Panel listPanel, Label countLabel, string type, StudioMediaFolderItem activeFolder, Action<StudioMediaFolderItem> onFolderSelected)
        {
            if (listPanel == null) return;
            listPanel.SuspendLayout();
            listPanel.Controls.Clear();

            List<StudioMediaFolderItem> folders = SmartMediaVault.GetFolders(type);
            if (countLabel != null)
            {
                countLabel.Text = string.Format("({0} folder)", folders.Count);
            }

            if (folders.Count == 0)
            {
                Panel pnlEmpty = new Panel
                {
                    Location = new Point(0, 8),
                    Size = new Size(Math.Max(200, listPanel.Width - 10), 100),
                    BackColor = Color.FromArgb(248, 249, 250),
                    BorderStyle = BorderStyle.FixedSingle
                };
                Label lblEmpty = new Label
                {
                    Text = "Belum ada folder terdaftar.\n\nKlik '+ Tambah Folder' di atas untuk memilih direktori video Anda.",
                    Font = new Font("Segoe UI", 8.25f),
                    ForeColor = UIFactory.TextSecondary,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                pnlEmpty.Controls.Add(lblEmpty);
                listPanel.Controls.Add(pnlEmpty);
                listPanel.ResumeLayout(true);
                return;
            }

            int curY = 4;
            int cardW = Math.Max(220, listPanel.Width - 16);
            int cardH = 58;

            foreach (StudioMediaFolderItem f in folders)
            {
                StudioMediaFolderItem folderRef = f;
                bool isSelected = (activeFolder != null && string.Equals(activeFolder.folder_path, f.folder_path, StringComparison.OrdinalIgnoreCase));

                Panel folderCard = new Panel
                {
                    Location = new Point(2, curY),
                    Size = new Size(cardW, cardH),
                    BackColor = isSelected ? Color.FromArgb(241, 245, 249) : Color.White,
                    Cursor = Cursors.Hand,
                    Tag = folderRef
                };

                folderCard.Paint += delegate(object s, PaintEventArgs e)
                {
                    Graphics g = e.Graphics;
                    if (isSelected)
                    {
                        using (Pen p = new Pen(UIFactory.BrandDark, 1.5f))
                        {
                            g.DrawRectangle(p, 0, 0, folderCard.Width - 1, folderCard.Height - 1);
                        }
                        using (SolidBrush b = new SolidBrush(UIFactory.BrandDark))
                        {
                            g.FillRectangle(b, 0, 0, 4, folderCard.Height);
                        }
                    }
                    else
                    {
                        using (Pen p = new Pen(UIFactory.BorderColor, 1f))
                        {
                            g.DrawRectangle(p, 0, 0, folderCard.Width - 1, folderCard.Height - 1);
                        }
                    }
                };

                Label lblIcon = new Label
                {
                    Text = "📁",
                    Font = new Font("Segoe UI Emoji", 13f, FontStyle.Regular),
                    Location = new Point(isSelected ? 10 : 8, 14),
                    Size = new Size(24, 28),
                    Cursor = Cursors.Hand
                };
                folderCard.Controls.Add(lblIcon);

                Label lblName = new Label
                {
                    Text = folderRef.name ?? "-",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = isSelected ? UIFactory.BrandDark : UIFactory.TextPrimary,
                    Location = new Point(isSelected ? 36 : 34, 10),
                    Size = new Size(cardW - 44, 20),
                    AutoEllipsis = true,
                    Cursor = Cursors.Hand
                };
                folderCard.Controls.Add(lblName);

                Label lblStats = new Label
                {
                    Text = string.Format("{0} Video • {1}", folderRef.last_scan_files_count, SmartMediaVault.FormatBytes(folderRef.last_scan_size_bytes)),
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = UIFactory.TextSecondary,
                    Location = new Point(isSelected ? 36 : 34, 30),
                    Size = new Size(cardW - 44, 18),
                    AutoEllipsis = true,
                    Cursor = Cursors.Hand
                };
                folderCard.Controls.Add(lblStats);

                EventHandler selectHandler = delegate(object s, EventArgs e)
                {
                    if (onFolderSelected != null) onFolderSelected(folderRef);
                };
                folderCard.Click += selectHandler;
                lblIcon.Click += selectHandler;
                lblName.Click += selectHandler;
                lblStats.Click += selectHandler;

                EventHandler enterHandler = delegate(object s, EventArgs e)
                {
                    if (!isSelected) folderCard.BackColor = Color.FromArgb(248, 249, 250);
                };
                EventHandler leaveHandler = delegate(object s, EventArgs e)
                {
                    if (!isSelected) folderCard.BackColor = Color.White;
                };
                folderCard.MouseEnter += enterHandler;
                folderCard.MouseLeave += leaveHandler;
                lblIcon.MouseEnter += enterHandler;
                lblName.MouseEnter += enterHandler;
                lblStats.MouseEnter += enterHandler;

                if (_mediaToolTip != null)
                {
                    _mediaToolTip.SetToolTip(folderCard, string.Format("Folder: {0}\nLokasi: {1}\nJumlah: {2} Video\nUkuran: {3}", folderRef.name, folderRef.folder_path, folderRef.last_scan_files_count, SmartMediaVault.FormatBytes(folderRef.last_scan_size_bytes)));
                }

                listPanel.Controls.Add(folderCard);
                curY += cardH + 6;
            }

            listPanel.ResumeLayout(true);
        }

        private void RenderMediaCards(FlowLayoutPanel grid, List<SmartMediaVault.MediaFileDetailItem> files, bool isVertical)
        {
            if (grid == null) return;
            grid.SuspendLayout();
            grid.Controls.Clear();

            if (files == null || files.Count == 0)
            {
                Panel pnlEmpty = new Panel
                {
                    Width = Math.Max(300, grid.ClientSize.Width - 30),
                    Height = 160,
                    BackColor = Color.White,
                    Margin = new Padding(10)
                };
                pnlEmpty.Paint += delegate(object s, PaintEventArgs e)
                {
                    using (Pen p = new Pen(UIFactory.BorderColor, 1f))
                    {
                        e.Graphics.DrawRectangle(p, 0, 0, pnlEmpty.Width - 1, pnlEmpty.Height - 1);
                    }
                };

                Label lblEmpty = new Label
                {
                    Text = "Tidak ada berkas video (.mp4/.mov/.mkv/.webm) di folder ini.\n\nSilakan tambahkan berkas video ke direktori folder ini atau pilih folder lainnya.",
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = UIFactory.TextSecondary,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                pnlEmpty.Controls.Add(lblEmpty);
                grid.Controls.Add(pnlEmpty);
                grid.ResumeLayout(true);
                return;
            }

            foreach (SmartMediaVault.MediaFileDetailItem fi in files)
            {
                Control card = CreateVideoCard(fi, isVertical);
                grid.Controls.Add(card);
            }

            grid.ResumeLayout(true);
        }

        private Control CreateVideoCard(SmartMediaVault.MediaFileDetailItem file, bool isVertical)
        {
            SmartMediaVault.MediaFileDetailItem itemRef = file;
            int cardW = isVertical ? 160 : 230;
            int cardH = isVertical ? 245 : 185;
            int previewW = isVertical ? 146 : 216;
            int previewH = isVertical ? 160 : 118;

            Panel card = new Panel
            {
                Size = new Size(cardW, cardH),
                BackColor = Color.White,
                Margin = new Padding(7),
                Cursor = Cursors.Hand,
                Tag = itemRef
            };

            bool isCardHovered = false;
            card.Paint += delegate(object s, PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                using (Pen p = new Pen(isCardHovered ? UIFactory.BrandDark : UIFactory.BorderColor, isCardHovered ? 1.5f : 1f))
                {
                    g.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            // 1. Cinematic Dark Preview Box
            Panel previewBox = new Panel
            {
                Location = new Point(7, 7),
                Size = new Size(previewW, previewH),
                BackColor = Color.FromArgb(15, 23, 42),
                Cursor = Cursors.Hand,
                Tag = itemRef
            };

            Label lblAspect = new Label
            {
                Text = isVertical ? "9:16" : "16:9",
                Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 41, 59),
                Size = new Size(34, 16),
                Location = new Point(6, 6),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            previewBox.Controls.Add(lblAspect);

            string extStr = (itemRef.Extension ?? "").TrimStart('.').ToUpperInvariant();
            if (string.IsNullOrEmpty(extStr)) extStr = "MP4";
            Label lblFormat = new Label
            {
                Text = extStr,
                Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = Color.FromArgb(30, 41, 59),
                Size = new Size(36, 16),
                Location = new Point(previewW - 42, 6),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            previewBox.Controls.Add(lblFormat);

            Label lblPlay = new Label
            {
                Text = "▶",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 55, 72),
                Size = new Size(38, 38),
                Location = new Point((previewW - 38) / 2, (previewH - 38) / 2),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            lblPlay.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen p = new Pen(Color.FromArgb(100, 116, 139), 1f))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, lblPlay.Width - 1, lblPlay.Height - 1);
                }
            };
            previewBox.Controls.Add(lblPlay);

            string sizeStr = SmartMediaVault.FormatBytes(itemRef.SizeBytes);
            Label lblSize = new Label
            {
                Text = sizeStr,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(20, 24, 33),
                AutoSize = true,
                Padding = new Padding(3, 1, 3, 1),
                Cursor = Cursors.Hand
            };
            previewBox.Controls.Add(lblSize);
            lblSize.Location = new Point(previewW - lblSize.PreferredWidth - 6, previewH - lblSize.PreferredHeight - 6);

            card.Controls.Add(previewBox);

            // 2. Metadata Section (Bottom)
            int metaTop = previewH + 12;
            int metaW = cardW - 14;

            Label lblFileName = new Label
            {
                Text = itemRef.FileName ?? "-",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = UIFactory.TextPrimary,
                Location = new Point(7, metaTop),
                Size = new Size(metaW, isVertical ? 36 : 28),
                AutoEllipsis = true,
                Cursor = Cursors.Hand,
                Tag = itemRef
            };
            card.Controls.Add(lblFileName);

            Label lblDate = new Label
            {
                Text = itemRef.LastModified.ToString("dd MMM yyyy, HH:mm"),
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = UIFactory.TextSecondary,
                Location = new Point(7, metaTop + (isVertical ? 38 : 28)),
                Size = new Size(metaW, 16),
                AutoEllipsis = true,
                Cursor = Cursors.Hand,
                Tag = itemRef
            };
            card.Controls.Add(lblDate);

            // 3. Interactive Actions & Context Menu
            Action playVideo = delegate
            {
                if (File.Exists(itemRef.FullPath))
                {
                    try { Process.Start(itemRef.FullPath); } catch (Exception ex)
                    {
                        MessageBox.Show("Gagal memutar video: " + ex.Message, "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            };

            Action showContextMenu = delegate
            {
                if (_videoContextMenu != null)
                {
                    _videoContextMenu.Tag = itemRef;
                    _videoContextMenu.Show(Cursor.Position);
                }
            };

            Control[] allControls = new Control[] { card, previewBox, lblAspect, lblFormat, lblPlay, lblSize, lblFileName, lblDate };
            foreach (Control c in allControls)
            {
                c.DoubleClick += delegate(object s, EventArgs e) { playVideo(); };
                c.MouseClick += delegate(object s, MouseEventArgs e)
                {
                    if (e.Button == MouseButtons.Right) showContextMenu();
                };

                c.MouseEnter += delegate(object s, EventArgs e)
                {
                    isCardHovered = true;
                    lblPlay.BackColor = UIFactory.AccentPrimary;
                    card.Invalidate();
                };

                c.MouseLeave += delegate(object s, EventArgs e)
                {
                    isCardHovered = false;
                    lblPlay.BackColor = Color.FromArgb(45, 55, 72);
                    card.Invalidate();
                };
            }

            lblPlay.Click += delegate(object s, EventArgs e)
            {
                MouseEventArgs me = e as MouseEventArgs;
                if (me != null && me.Button == MouseButtons.Right) showContextMenu();
                else playVideo();
            };

            if (_mediaToolTip != null)
            {
                string tipText = string.Format("Nama: {0}\nUkuran: {1}\nFormat: {2}\nTanggal: {3}\nJalur: {4}\n\n(Klik dua kali untuk memutar, Klik kanan untuk opsi)",
                    itemRef.FileName, SmartMediaVault.FormatBytes(itemRef.SizeBytes), itemRef.Extension, itemRef.LastModified.ToString("yyyy-MM-dd HH:mm:ss"), itemRef.FullPath);
                foreach (Control c in allControls)
                {
                    _mediaToolTip.SetToolTip(c, tipText);
                }
            }

            return card;
        }

        private void AddFolderForType(string type, Action refreshCallback)
        {
            ModernFolderPicker picker = new ModernFolderPicker();
            picker.Title = (type == "FootageVertical")
                ? "Pilih Folder Berisi Video Footage Vertikal (9:16)"
                : "Pilih Folder Berisi Video Footage Landscape (16:9)";
            picker.OkButtonLabel = "Pilih Folder";

            if (picker.ShowDialog(this) == DialogResult.OK)
            {
                string selectedPath = picker.SelectedPath;
                if (!string.IsNullOrEmpty(selectedPath) && Directory.Exists(selectedPath))
                {
                    StudioMediaFolderItem item = SmartMediaVault.AddFolderDirect(type, selectedPath);
                    if (item != null)
                    {
                        if (type == "FootageVertical") _selectedVerticalFolder = item;
                        else if (type == "FootageLandscape") _selectedLandscapeFolder = item;

                        if (refreshCallback != null) refreshCallback();
                        MessageBox.Show(string.Format("Folder '{0}' berhasil didaftarkan!\nTerdeteksi: {1} berkas video ({2}).",
                            item.name, item.last_scan_files_count, SmartMediaVault.FormatBytes(item.last_scan_size_bytes)),
                            "Koleksi Berhasil Ditambahkan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void DeleteActiveFolder(StudioMediaFolderItem folder, string type, Action refreshCallback)
        {
            if (folder != null)
            {
                string msg = string.Format("Apakah Anda yakin ingin melepas folder '{0}' dari daftar aplikasi?\n\nCatatan Penting:\nBerkas video asli di komputer Anda ({1}) tetap aman dan TIDAK AKAN terhapus.", folder.name, folder.folder_path);
                if (MessageBox.Show(msg, "Konfirmasi Pelepasan Folder", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SmartMediaVault.RemoveFolder(folder.folder_path, type);
                    if (type == "FootageVertical") _selectedVerticalFolder = null;
                    else if (type == "FootageLandscape") _selectedLandscapeFolder = null;
                    if (refreshCallback != null) refreshCallback();
                }
            }
            else
            {
                MessageBox.Show("Silakan pilih folder yang ingin dilepas terlebih dahulu.", "Pemberitahuan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OpenActiveFolder(StudioMediaFolderItem folder)
        {
            if (folder != null && !string.IsNullOrEmpty(folder.folder_path))
            {
                if (Directory.Exists(folder.folder_path))
                {
                    Process.Start("explorer.exe", folder.folder_path);
                }
                else
                {
                    MessageBox.Show("Folder tidak ditemukan di disk: " + folder.folder_path, "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Silakan pilih folder terlebih dahulu.", "Pemberitahuan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // VIEW 3: KOLEKSI AUDIO
        // ═════════════════════════════════════════════════════════════════════
        private ListView _lvAudioTracks;

        private void InitializeAudioView()
        {
            _viewAudio = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };
            _pnlViewContainer.Controls.Add(_viewAudio);

            RoundedCardPanel card = UIFactory.CreateCard(0, 0, _pnlViewContainer.ClientSize.Width - 32, _pnlViewContainer.ClientSize.Height - 32, 12);
            card.Dock = DockStyle.Fill;
            _viewAudio.Controls.Add(card);

            Label lblTitle = new Label { Text = "Koleksi Audio & Musik (BGM)", UseMnemonic = false, Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = UIFactory.TextPrimary, Location = new Point(16, 12), AutoSize = true };
            card.Controls.Add(lblTitle);

            Label lblSub = new Label { Text = "Pustaka audio latar (BGM) resmi bebas copyright berdaya hipnosis tinggi untuk video edukasi vertikal", Font = new Font("Segoe UI", 8.5f), ForeColor = UIFactory.TextSecondary, Location = new Point(16, 36), AutoSize = true };
            card.Controls.Add(lblSub);

            RoundedButton btnPlay = UIFactory.CreateButton("Putar Audio", 16, 64, 105, 32, UIFactory.AccentPrimary, 6);
            btnPlay.Click += delegate(object s, EventArgs e) { PlaySelectedTrackFromList(); };
            card.Controls.Add(btnPlay);

            RoundedButton btnStop = UIFactory.CreateButton("Hentikan", 127, 64, 80, 32, UIFactory.AccentPrimaryLight, 6);
            btnStop.Click += delegate(object s, EventArgs e) { StopBgmPlayback(); };
            card.Controls.Add(btnStop);

            RoundedButton btnUpload = UIFactory.CreateButton("+ Tambah BGM", 213, 64, 110, 32, UIFactory.AccentPrimaryLight, 6);
            btnUpload.Click += delegate(object s, EventArgs e) { UploadBgm(); RefreshAudioViewData(); };
            card.Controls.Add(btnUpload);

            RoundedButton btnDeleteAudio = UIFactory.CreateButton("Hapus Track", 329, 64, 105, 32, Color.FromArgb(254, 242, 242), UIFactory.AccentRed, 6);
            btnDeleteAudio.Click += delegate(object s, EventArgs e) { DeleteSelectedAudioTrack(); };
            card.Controls.Add(btnDeleteAudio);

            _lvAudioTracks = new ListView
            {
                Location = new Point(16, 112),
                Size = new Size(card.Width - 32, card.Height - 128),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9f),
                BorderStyle = BorderStyle.FixedSingle
            };
            _lvAudioTracks.Columns.Add("Nama Berkas Audio", 280);
            _lvAudioTracks.Columns.Add("Kategori / Mood", 200);
            _lvAudioTracks.Columns.Add("Format", 80);
            _lvAudioTracks.Columns.Add("Ukuran", 110);
            _lvAudioTracks.Columns.Add("Status SSOT", 140);
            card.Controls.Add(_lvAudioTracks);
        }

        private void RefreshAudioViewData()
        {
            if (_lvAudioTracks == null) return;
            _lvAudioTracks.Items.Clear();

            string bgmDir = StudioBgmLibrary;
            if (Directory.Exists(bgmDir))
            {
                string[] files = Directory.GetFiles(bgmDir, "*.mp3", SearchOption.AllDirectories);
                foreach (string f in files)
                {
                    FileInfo fi = new FileInfo(f);
                    string name = fi.Name;
                    string mood = "Reflektif & Mengalir";
                    if (name.Contains("Dark")) mood = "Misterius & Psikologi";
                    else if (name.Contains("Stoic")) mood = "Stoikisme & Kebijaksanaan";
                    else if (name.Contains("Inspiring")) mood = "Inspiratif & Bangkit";
                    else if (name.Contains("Peaceful")) mood = "Ketenangan Jiwa";

                    ListViewItem itm = new ListViewItem(name);
                    itm.SubItems.Add(mood);
                    itm.SubItems.Add(fi.Extension.ToUpperInvariant());
                    itm.SubItems.Add(SmartMediaVault.FormatBytes(fi.Length));
                    itm.SubItems.Add("Pustaka Resmi");
                    itm.Tag = f;
                    _lvAudioTracks.Items.Add(itm);
                }
            }

            if (_lvAudioTracks.Items.Count == 0)
            {
                ListViewItem itm = new ListViewItem("Pustaka audio masih kosong (Belum ada berkas)");
                itm.SubItems.Add("Tambahkan Berkas Musik");
                itm.SubItems.Add("-");
                itm.SubItems.Add("0 B");
                itm.SubItems.Add("Klik '+ Tambah BGM'");
                itm.ForeColor = Color.Gray;
                _lvAudioTracks.Items.Add(itm);
            }
        }

        private void PlaySelectedTrackFromList()
        {
            if (_lvAudioTracks != null && _lvAudioTracks.SelectedItems.Count > 0)
            {
                string path = _lvAudioTracks.SelectedItems[0].Tag as string;
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    mciSendString("close bgm_alias", null, 0, IntPtr.Zero);
                    mciSendString(string.Format("open \"{0}\" type mpegvideo alias bgm_alias", path), null, 0, IntPtr.Zero);
                    mciSendString("play bgm_alias", null, 0, IntPtr.Zero);
                    _isBgmPlaying = true;
                    AppendStudioLog("[BGM] Memutar audio: " + Path.GetFileName(path));
                }
                else
                {
                    MessageBox.Show("Silakan pilih berkas audio riil dalam daftar atau klik '+ Tambah BGM' untuk memasukkan berkas MP3 baru.", "Pemberitahuan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void DeleteSelectedAudioTrack()
        {
            if (_lvAudioTracks != null && _lvAudioTracks.SelectedItems.Count > 0)
            {
                string path = _lvAudioTracks.SelectedItems[0].Tag as string;
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    if (MessageBox.Show("Apakah Anda yakin ingin menghapus berkas audio '" + Path.GetFileName(path) + "' dari pustaka?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            StopBgmPlayback();
                            File.Delete(path);
                            RefreshAudioViewData();
                            PopulateBgmList();
                            MessageBox.Show("Berkas audio berhasil dihapus.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Gagal menghapus berkas audio: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // VIEW 4: COLOR GRADING (3D LUTs)
        // ═════════════════════════════════════════════════════════════════════
        private void InitializeGradingView()
        {
            _viewGrading = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };
            _pnlViewContainer.Controls.Add(_viewGrading);

            RoundedCardPanel card = UIFactory.CreateCard(0, 0, _pnlViewContainer.ClientSize.Width - 32, _pnlViewContainer.ClientSize.Height - 32, 12);
            card.Dock = DockStyle.Fill;
            _viewGrading.Controls.Add(card);

            Label lblTitle = new Label { Text = "Galeri Color Grading (3D LUTs Cinematic)", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = UIFactory.TextPrimary, Location = new Point(16, 12), AutoSize = true };
            card.Controls.Add(lblTitle);

            Label lblSub = new Label { Text = "Daftar preset 3D LUT (.cube) untuk memberi nuansa visual sinematik khas film layar lebar pada video vertikal", Font = new Font("Segoe UI", 8.5f), ForeColor = UIFactory.TextSecondary, Location = new Point(16, 36), AutoSize = true };
            card.Controls.Add(lblSub);

            RoundedButton btnUpload = UIFactory.CreateButton("+ Tambah File .cube", 16, 64, 170, 32, UIFactory.AccentPrimary, 6);
            btnUpload.Click += delegate(object s, EventArgs e) { UploadLut(); };
            card.Controls.Add(btnUpload);

            FlowLayoutPanel flpLuts = new FlowLayoutPanel
            {
                Location = new Point(16, 110),
                Size = new Size(card.Width - 32, card.Height - 126),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(flpLuts);

            string[][] lutCards = new string[][] {
                new string[] { "Nordic Cold", "lut_nordic_cold.cube", "Dingin stoik, shadow pekat, desaturasi hangat Eropa.", "Karakter Stoik & Reflektif" },
                new string[] { "Teal & Orange", "lut_teal_orange.cube", "Kontras sinematik Hollywood, warna kulit cerah dan punchy.", "Standar Sinematik Modern" },
                new string[] { "Golden Warm", "lut_golden_warm.cube", "Nuansa golden hour sore hari, hangat, tenang, dan nostalgia.", "Senja & Renungan Hidup" },
                new string[] { "Contrasty Cold", "lut_contrasty_cold.cube", "Pekat tajam dengan shadow hitam pekat, aura misterius.", "Psikologi Gelap & Kejahatan" },
                new string[] { "Deep Teal", "lut_deep_teal.cube", "Nuansa hijau kebiruan pekat, futuristik dan atmosferik.", "Cyberpunk & Sains" },
                new string[] { "Neutral Fade", "lut_neutral_fade.cube", "Karakter film analog lembut dengan kontras terangkat halus.", "Film Vintage & Dokumenter" }
            };

            foreach (string[] l in lutCards)
            {
                RoundedCardPanel lutCard = UIFactory.CreateCard(0, 0, 260, 160, 8);
                lutCard.Margin = new Padding(8);

                Label lTitle = new Label { Text = l[0], Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = UIFactory.TextPrimary, Location = new Point(10, 10), AutoSize = true };
                lutCard.Controls.Add(lTitle);

                Label lBadge = new Label { Text = l[3], Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = UIFactory.AccentTeal, Location = new Point(10, 32), AutoSize = true };
                lutCard.Controls.Add(lBadge);

                Label lDesc = new Label { Text = l[2], Font = new Font("Segoe UI", 8.25f), ForeColor = UIFactory.TextSecondary, Location = new Point(10, 52), Size = new Size(240, 50) };
                lutCard.Controls.Add(lDesc);

                RoundedButton btnSelect = UIFactory.CreateButton("Pilih LUT Ini", 10, 114, 110, 28, UIFactory.AccentPrimary, 6);
                string lutName = l[0];
                btnSelect.Click += delegate(object s, EventArgs e)
                {
                    if (_cbStudioGrading != null)
                    {
                        for (int i = 0; i < _cbStudioGrading.Items.Count; i++)
                        {
                            if (_cbStudioGrading.Items[i].ToString().IndexOf(lutName, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                _cbStudioGrading.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                    SwitchSidebarView("Studio");
                    AppendStudioLog("[LUT] Preset dipilih: " + lutName);
                };
                lutCard.Controls.Add(btnSelect);

                flpLuts.Controls.Add(lutCard);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // VIEW 5: RIWAYAT PROYEK
        // ═════════════════════════════════════════════════════════════════════
        private ListView _lvProjects;

        private void InitializeProjectsView()
        {
            _viewProjects = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };
            _pnlViewContainer.Controls.Add(_viewProjects);

            RoundedCardPanel card = UIFactory.CreateCard(0, 0, _pnlViewContainer.ClientSize.Width - 32, _pnlViewContainer.ClientSize.Height - 32, 12);
            card.Dock = DockStyle.Fill;
            _viewProjects.Controls.Add(card);

            Label lblTitle = new Label { Text = "Riwayat Proyek & Video Final Selesai", UseMnemonic = false, Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = UIFactory.TextPrimary, Location = new Point(16, 12), AutoSize = true };
            card.Controls.Add(lblTitle);

            Label lblSub = new Label { Text = "Daftar hasil render video vertikal yang berhasil dirakit dan tersimpan di sistem", Font = new Font("Segoe UI", 8.5f), ForeColor = UIFactory.TextSecondary, Location = new Point(16, 36), AutoSize = true };
            card.Controls.Add(lblSub);

            RoundedButton btnPlay = UIFactory.CreateButton("Putar Video", 16, 64, 105, 32, UIFactory.AccentPrimary, 6);
            btnPlay.Click += delegate(object s, EventArgs e) { PlaySelectedProjectVideo(); };
            card.Controls.Add(btnPlay);

            RoundedButton btnFolder = UIFactory.CreateButton("Buka Folder", 127, 64, 105, 32, UIFactory.AccentPrimaryLight, 6);
            btnFolder.Click += delegate(object s, EventArgs e) { OpenStudioFolder(); };
            card.Controls.Add(btnFolder);

            RoundedButton btnRefresh = UIFactory.CreateButton("Segarkan", 238, 64, 90, 32, UIFactory.BgSubtle, 6);
            btnRefresh.Click += delegate(object s, EventArgs e) { RefreshProjectsList(); };
            card.Controls.Add(btnRefresh);

            _lvProjects = new ListView
            {
                Location = new Point(16, 112),
                Size = new Size(card.Width - 32, card.Height - 128),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9f),
                BorderStyle = BorderStyle.FixedSingle
            };
            _lvProjects.Columns.Add("Nama Berkas Video", 320);
            _lvProjects.Columns.Add("Resolusi", 100);
            _lvProjects.Columns.Add("Ukuran Berkas", 110);
            _lvProjects.Columns.Add("Tanggal Selesai", 150);
            _lvProjects.Columns.Add("Lokasi Penyimpanan", 320);
            card.Controls.Add(_lvProjects);
        }

        private void RefreshProjectsList()
        {
            if (_lvProjects == null) return;
            _lvProjects.Items.Clear();

            List<string> checkDirs = new List<string>();
            string projDir = AppPaths.ProjectsDir;
            if (Directory.Exists(projDir)) checkDirs.Add(projDir);

            foreach (string dir in checkDirs)
            {
                try
                {
                    string[] mp4s = Directory.GetFiles(dir, "*.mp4", SearchOption.AllDirectories);
                    Array.Sort(mp4s);
                    Array.Reverse(mp4s);

                    foreach (string f in mp4s)
                    {
                        FileInfo fi = new FileInfo(f);
                        ListViewItem itm = new ListViewItem(fi.Name);
                        itm.SubItems.Add("1080x1920 (9:16)");
                        itm.SubItems.Add(SmartMediaVault.FormatBytes(fi.Length));
                        itm.SubItems.Add(fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
                        itm.SubItems.Add(fi.DirectoryName);
                        itm.Tag = f;
                        _lvProjects.Items.Add(itm);
                    }
                }
                catch { }
            }
        }

        private void PlaySelectedProjectVideo()
        {
            if (_lvProjects != null && _lvProjects.SelectedItems.Count > 0)
            {
                string path = _lvProjects.SelectedItems[0].Tag as string;
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    Process.Start(path);
                }
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // VIEW 6: KUNCI TOKEN & API
        // ═════════════════════════════════════════════════════════════════════
        private ListView _lvApiKeys;

        private void InitializeApiView()
        {
            _viewApi = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };
            _pnlViewContainer.Controls.Add(_viewApi);

            RoundedCardPanel card = UIFactory.CreateCard(0, 0, _pnlViewContainer.ClientSize.Width - 32, _pnlViewContainer.ClientSize.Height - 32, 12);
            card.Dock = DockStyle.Fill;
            _viewApi.Controls.Add(card);

            Label lblTitle = new Label { Text = "Kunci Token & Sekoci API Terpadu", UseMnemonic = false, Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = UIFactory.TextPrimary, Location = new Point(16, 12), AutoSize = true };
            card.Controls.Add(lblTitle);

            Label lblSub = new Label { Text = "Pengelolaan sekoci kunci Google Gemini (Naskah) dan ElevenLabs (Sintesis Vokal AI)", Font = new Font("Segoe UI", 8.5f), ForeColor = UIFactory.TextSecondary, Location = new Point(16, 36), AutoSize = true };
            card.Controls.Add(lblSub);

            RoundedButton btnManage = UIFactory.CreateButton("Buka Pengelola API", 16, 64, 170, 32, UIFactory.AccentPrimary, 6);
            btnManage.Click += delegate(object s, EventArgs e) { SmartApiVault.ShowDialog(this); RefreshApiViewData(); };
            card.Controls.Add(btnManage);

            RoundedButton btnTestAll = UIFactory.CreateButton("Uji Koneksi & Kuota", 192, 64, 170, 32, UIFactory.AccentTeal, 6);
            btnTestAll.Click += async delegate(object s, EventArgs e)
            {
                btnTestAll.Enabled = false;
                btnTestAll.Text = "⏳ Menguji...";
                try
                {
                    List<StudioApiKeyItem> list = SmartApiVault.LoadVault();
                    foreach (StudioApiKeyItem itm in list)
                    {
                        if (itm.provider == "Gemini") itm.last_tested_status = await SmartApiVault.TestGeminiKeyAsync(itm.key);
                        else if (itm.provider == "ElevenLabs") itm.last_tested_status = await SmartApiVault.TestElevenLabsKeyAsync(itm.key);
                        itm.last_tested_at = DateTime.Now.ToString("HH:mm:ss");
                    }
                    SmartApiVault.SaveVault(list);
                    RefreshApiViewData();
                    MessageBox.Show("Pemeriksaan kuota dan status seluruh kunci API selesai.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                finally
                {
                    btnTestAll.Enabled = true;
                    btnTestAll.Text = "Cek Status & Kuota Semua";
                }
            };
            card.Controls.Add(btnTestAll);

            _lvApiKeys = new ListView
            {
                Location = new Point(16, 112),
                Size = new Size(card.Width - 32, card.Height - 128),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9f),
                BorderStyle = BorderStyle.FixedSingle
            };
            _lvApiKeys.Columns.Add("Provider", 110);
            _lvApiKeys.Columns.Add("Label Sekoci", 220);
            _lvApiKeys.Columns.Add("Kunci (Key)", 180);
            _lvApiKeys.Columns.Add("Status Kuota / Koneksi", 240);
            _lvApiKeys.Columns.Add("Terakhir Diuji", 110);
            card.Controls.Add(_lvApiKeys);
        }

        private void RefreshApiViewData()
        {
            if (_lvApiKeys == null) return;
            _lvApiKeys.Items.Clear();
            List<StudioApiKeyItem> items = SmartApiVault.LoadVault();
            foreach (StudioApiKeyItem itm in items)
            {
                ListViewItem lvi = new ListViewItem(itm.provider ?? "-");
                lvi.SubItems.Add(itm.label ?? "-");
                string masked = (itm.key != null && itm.key.Length > 8) ? itm.key.Substring(0, 4) + "••••••••" + itm.key.Substring(itm.key.Length - 4) : "••••••••";
                lvi.SubItems.Add(masked);
                lvi.SubItems.Add(itm.last_tested_status ?? "Belum diuji");
                lvi.SubItems.Add(itm.last_tested_at ?? "-");
                _lvApiKeys.Items.Add(lvi);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // VIEW 7: PENGATURAN & LISENSI
        // ═════════════════════════════════════════════════════════════════════
        private TextBox _txtSettingsMachineGuid;
        private TextBox _txtSettingsLicenseKey;
        private Label _lblSettingsLicStatus;

        private void InitializeSettingsView()
        {
            _viewSettings = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };
            _pnlViewContainer.Controls.Add(_viewSettings);

            RoundedCardPanel card = UIFactory.CreateCard(0, 0, _pnlViewContainer.ClientSize.Width - 32, _pnlViewContainer.ClientSize.Height - 32, 12);
            card.Dock = DockStyle.Fill;
            _viewSettings.Controls.Add(card);

            Label lblTitle = new Label { Text = "Pengaturan Sistem & Lisensi Aplikasi", UseMnemonic = false, Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = UIFactory.TextPrimary, Location = new Point(16, 12), AutoSize = true };
            card.Controls.Add(lblTitle);

            Label lblSub = new Label { Text = "Status lisensi offline, Machine GUID perangkat, dan konfigurasi akselerasi GPU NVENC", Font = new Font("Segoe UI", 8.5f), ForeColor = UIFactory.TextSecondary, Location = new Point(16, 36), AutoSize = true };
            card.Controls.Add(lblSub);

            int startY = 70;

            Label lblGuid = new Label { Text = "Machine GUID / Hardware ID Perangkat:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = UIFactory.TextSecondary, Location = new Point(16, startY), AutoSize = true };
            card.Controls.Add(lblGuid);

            _txtSettingsMachineGuid = new TextBox
            {
                Location = new Point(16, startY + 22),
                Size = new Size(420, 26),
                ReadOnly = true,
                BackColor = Color.FromArgb(241, 245, 249),
                Font = new Font("Consolas", 9.5f)
            };
            card.Controls.Add(_txtSettingsMachineGuid);

            RoundedButton btnCopyGuid = UIFactory.CreateButton("Salin ID", 446, startY + 20, 90, 28, UIFactory.AccentPrimaryLight, 6);
            btnCopyGuid.Click += delegate(object s, EventArgs e)
            {
                Clipboard.SetText(_txtSettingsMachineGuid.Text);
                MessageBox.Show("Machine GUID berhasil disalin ke clipboard.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            card.Controls.Add(btnCopyGuid);

            startY += 62;

            Label lblLic = new Label { Text = "Kunci Lisensi Aktif:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = UIFactory.TextSecondary, Location = new Point(16, startY), AutoSize = true };
            card.Controls.Add(lblLic);

            _txtSettingsLicenseKey = new TextBox
            {
                Location = new Point(16, startY + 22),
                Size = new Size(420, 26),
                ReadOnly = true,
                BackColor = Color.FromArgb(241, 245, 249),
                Font = new Font("Consolas", 10.5f, FontStyle.Bold)
            };
            card.Controls.Add(_txtSettingsLicenseKey);

            RoundedButton btnReactivate = UIFactory.CreateButton("Aktivasi Serial Baru", 446, startY + 20, 160, 28, UIFactory.AccentPrimary, 6);
            btnReactivate.Click += delegate(object s, EventArgs e)
            {
                using (LicenseActivationForm form = new LicenseActivationForm())
                {
                    form.ShowDialog(this);
                    RefreshSettingsViewData();
                }
            };
            card.Controls.Add(btnReactivate);

            startY += 58;

            _lblSettingsLicStatus = new Label { Text = "Status: -", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = UIFactory.AccentGreen, Location = new Point(16, startY), AutoSize = true };
            card.Controls.Add(_lblSettingsLicStatus);

            startY += 40;

            // Info Akselerasi GPU & Path
            Label lblGpu = new Label
            {
                Text = "Informasi Mesin Produksi:\n" +
                       "• Akselerasi Render: NVIDIA NVENC Hardware Encode (Fallback CPU libx264 Universal)\n" +
                       "• Mesin Storyboard: Smart Semantic Choreographer & SQLite WAL State Ledger\n" +
                       "• Pustaka BGM: Pustaka Audio BGM Portabel (15+ Trek Bebas Royalti Bawaan)\n" +
                       "• Grading 3D LUT: Pustaka Cinematic Color Grading (25 Presets Sinematik Hollywood)\n" +
                       "• Status Pipeline: Siap Produksi Mandiri Portabel",
                Font = new Font("Segoe UI", 9f),
                ForeColor = UIFactory.TextPrimary,
                Location = new Point(16, startY),
                Size = new Size(580, 120)
            };
            card.Controls.Add(lblGpu);
        }

        private void RefreshSettingsViewData()
        {
            if (_txtSettingsMachineGuid != null) _txtSettingsMachineGuid.Text = LicenseManager.GetMachineGuid();
            if (_txtSettingsLicenseKey != null) _txtSettingsLicenseKey.Text = LicenseManager.GetCurrentLicenseKey();
            if (_lblSettingsLicStatus != null)
            {
                string status = LicenseManager.GetLicenseStatusText();
                _lblSettingsLicStatus.Text = "Status: " + status;
                _lblSettingsLicStatus.ForeColor = LicenseManager.IsLicenseValid() ? UIFactory.AccentGreen : UIFactory.AccentAmber;
            }
        }
    }
}
