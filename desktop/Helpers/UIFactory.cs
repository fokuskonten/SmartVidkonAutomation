using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SmartVidkonAutomation.Helpers
{
    public static class UIFactory
    {
        // ── Palet Tema Modern Enterprise ─────────────────────────────────────
        public static readonly Color BgCanvas = Color.FromArgb(248, 249, 250);       // Slate 50 Canvas (#F8F9FA)
        public static readonly Color BgLight = Color.FromArgb(248, 249, 250);        // Slate 50
        public static readonly Color BgPanel = Color.FromArgb(255, 255, 255);        // Pure White Card
        public static readonly Color BgSubtle = Color.FromArgb(242, 244, 246);       // Slate 100 Input (#F2F4F6)
        public static readonly Color TextPrimary = Color.FromArgb(19, 27, 46);       // Deep Navy (#131B2E)
        public static readonly Color TextSecondary = Color.FromArgb(71, 85, 105);    // Slate 600
        public static readonly Color TextMuted = Color.FromArgb(148, 163, 184);      // Slate 400
        public static readonly Color BorderColor = Color.FromArgb(226, 232, 240);    // Slate 200 (#E2E8F0)
        public static readonly Color BorderDark = Color.FromArgb(148, 163, 184);     // Slate 400
        
        // Brand & Aksen
        public static readonly Color BrandDark = Color.FromArgb(19, 27, 46);         // Dark Navy (#131B2E)
        public static readonly Color BrandDarkHover = Color.FromArgb(30, 41, 59);    // Slate 800 (#1E293B)
        public static readonly Color AccentPrimary = Color.FromArgb(9, 9, 11);       // Solid Black / Navy (#09090B)
        public static readonly Color AccentPrimaryHover = Color.FromArgb(39, 39, 42);
        public static readonly Color AccentPrimaryLight = Color.FromArgb(244, 244, 245); // Neutral 100 (#F4F4F5)
        public static readonly Color AccentBlue = Color.FromArgb(37, 99, 235);        // Royal Blue
        public static readonly Color AccentGreen = Color.FromArgb(16, 185, 129);      // Emerald 500
        public static readonly Color AccentGreenLight = Color.FromArgb(236, 253, 245); // Emerald 50
        public static readonly Color AccentAmber = Color.FromArgb(217, 119, 6);       // Amber 600
        public static readonly Color AccentRed = Color.FromArgb(220, 38, 38);         // Red 600
        public static readonly Color AccentRedLight = Color.FromArgb(254, 242, 242);   // Red 50
        public static readonly Color AccentTeal = Color.FromArgb(15, 118, 110);      // Teal 700 (#0F766E)

        public static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));

            // Top Left
            path.AddArc(arcRect, 180, 90);

            // Top Right
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);

            // Bottom Right
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);

            // Bottom Left
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            return CreateRoundedRectanglePath(rect, radius);
        }

        public static RoundedCardPanel CreateCard(int x, int y, int width, int height, int radius = 12)
        {
            RoundedCardPanel p = new RoundedCardPanel();
            p.Location = new Point(x, y);
            p.Size = new Size(width, height);
            p.BorderRadius = radius;
            p.FillColor = BgPanel;
            p.BorderColor = BorderColor;
            p.Padding = new Padding(14);
            return p;
        }

        public static RoundedButton CreateButton(string text, int x, int y, int width, int height, Color bgColor, int radius = 8)
        {
            RoundedButton btn = new RoundedButton();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, height);
            btn.BorderRadius = radius;
            btn.NormalBg = bgColor;
            btn.HoverBg = ControlPaint.Light(bgColor, 0.08f);
            btn.NormalBorder = (bgColor == AccentPrimaryLight || bgColor == BgSubtle) ? BorderColor : Color.Transparent;
            btn.NormalText = (bgColor == AccentPrimary || bgColor == BrandDark || bgColor == AccentTeal || bgColor == AccentRed || bgColor == AccentGreen) 
                ? Color.White 
                : TextPrimary;
            btn.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        public static RoundedButton CreateButton(string text, int x, int y, int width, int height, Color bgColor, Color foreColor, int radius = 8)
        {
            RoundedButton btn = CreateButton(text, x, y, width, height, bgColor, radius);
            btn.NormalText = foreColor;
            return btn;
        }

        public static Label CreateSectionHeader(string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 13.5f, FontStyle.Bold);
            lbl.ForeColor = TextPrimary;
            lbl.Location = new Point(x, y);
            lbl.AutoSize = true;
            return lbl;
        }

        public static Label CreateSubHeader(string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 9f);
            lbl.ForeColor = TextSecondary;
            lbl.Location = new Point(x, y);
            lbl.AutoSize = true;
            return lbl;
        }

        public static bool ConfirmAction(string message, string title = "Konfirmasi Tindakan", MessageBoxIcon icon = MessageBoxIcon.Question)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, icon, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        public static bool ConfirmCriticalAction(string message, string title = "Peringatan Kritis")
        {
            return MessageBox.Show(message + "\n\nApakah Anda benar-benar yakin ingin melanjutkan tindakan ini?", title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }
    }

    public static class RoundedControlHelper
    {
        public static Color GetRealParentBackColor(Control ctrl)
        {
            Control p = ctrl != null ? ctrl.Parent : null;
            while (p != null)
            {
                if (p is RoundedCardPanel) return ((RoundedCardPanel)p).FillColor;
                if (p.BackColor != Color.Transparent && p.BackColor.A > 0) return p.BackColor;
                p = p.Parent;
            }
            return UIFactory.BgCanvas;
        }
    }

    public class RoundedCardPanel : Panel
    {
        public int BorderRadius { get; set; }
        public Color BorderColor { get; set; }
        public Color FillColor { get; set; }

        public RoundedCardPanel()
        {
            this.BorderRadius = 12;
            this.BorderColor = Color.FromArgb(226, 232, 240);
            this.FillColor = Color.White;
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            this.UpdateStyles();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Color parentBg = RoundedControlHelper.GetRealParentBackColor(this);
            using (SolidBrush parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, this.ClientRectangle);
            }

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (GraphicsPath path = UIFactory.CreateRoundedRectanglePath(rect, BorderRadius))
            {
                using (SolidBrush brush = new SolidBrush(FillColor))
                {
                    g.FillPath(brush, path);
                }

                if (BorderColor != Color.Transparent)
                {
                    using (Pen pen = new Pen(BorderColor, 1f))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }
        }
    }

    public class ModernSidebarButton : Button
    {
        public bool IsActive { get; set; }
        public string IconSymbol { get; set; }
        private bool _isHovered = false;

        public ModernSidebarButton()
        {
            this.IsActive = false;
            this.IconSymbol = " ";
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 9.25f, FontStyle.Regular);
            this.Height = 40;
            this.Cursor = Cursors.Hand;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            this.UpdateStyles();

            this.MouseEnter += delegate(object s, EventArgs e) { _isHovered = true; this.Invalidate(); };
            this.MouseLeave += delegate(object s, EventArgs e) { _isHovered = false; this.Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Color parentBg = RoundedControlHelper.GetRealParentBackColor(this);
            using (SolidBrush pBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(pBrush, this.ClientRectangle);
            }

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            Color bg = IsActive ? UIFactory.BrandDark : (_isHovered ? Color.FromArgb(241, 245, 249) : Color.Transparent);
            Color textCol = IsActive ? Color.White : (_isHovered ? UIFactory.BrandDark : Color.FromArgb(71, 85, 105));

            if (bg != Color.Transparent)
            {
                using (GraphicsPath path = UIFactory.CreateRoundedRectanglePath(rect, 10))
                {
                    using (SolidBrush brush = new SolidBrush(bg))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }

            if (!string.IsNullOrEmpty(IconSymbol))
            {
                Rectangle iconRect = new Rectangle(12, 0, 24, this.Height);
                Font iconFont = null;
                try
                {
                    iconFont = new Font("Segoe UI Emoji", 11f, FontStyle.Regular);
                }
                catch
                {
                    iconFont = new Font("Segoe UI", 11f, FontStyle.Regular);
                }
                using (iconFont)
                {
                    TextRenderer.DrawText(g, IconSymbol, iconFont, iconRect, textCol,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
            }

            int textLeft = string.IsNullOrEmpty(IconSymbol) ? 14 : 44;
            Rectangle textRect = new Rectangle(textLeft, 0, this.Width - textLeft - 8, this.Height);
            Font useFont = IsActive ? new Font(this.Font, FontStyle.Bold) : this.Font;
            TextRenderer.DrawText(g, this.Text, useFont, textRect, textCol,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }

    public class RoundedButton : Button
    {
        public int BorderRadius { get; set; }
        public Color NormalBg { get; set; }
        public Color HoverBg { get; set; }
        public Color NormalBorder { get; set; }
        public Color NormalText { get; set; }
        private bool _isHovered = false;
        private bool _isPressed = false;

        public RoundedButton()
        {
            this.BorderRadius = 8;
            this.NormalBg = Color.FromArgb(19, 27, 46);
            this.HoverBg = Color.FromArgb(30, 41, 59);
            this.NormalBorder = Color.Transparent;
            this.NormalText = Color.White;
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            this.UpdateStyles();

            this.MouseEnter += delegate(object s, EventArgs e) { _isHovered = true; this.Invalidate(); };
            this.MouseLeave += delegate(object s, EventArgs e) { _isHovered = false; _isPressed = false; this.Invalidate(); };
            this.MouseDown += delegate(object s, MouseEventArgs e) { _isPressed = true; this.Invalidate(); };
            this.MouseUp += delegate(object s, MouseEventArgs e) { _isPressed = false; this.Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Color parentBg = RoundedControlHelper.GetRealParentBackColor(this);
            using (SolidBrush parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, this.ClientRectangle);
            }

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            Color currentBg = _isHovered ? HoverBg : NormalBg;
            if (_isPressed) currentBg = Color.FromArgb(Math.Max(0, currentBg.R - 25), Math.Max(0, currentBg.G - 25), Math.Max(0, currentBg.B - 25));

            using (GraphicsPath path = UIFactory.CreateRoundedRectanglePath(rect, BorderRadius))
            {
                using (SolidBrush brush = new SolidBrush(currentBg))
                {
                    g.FillPath(brush, path);
                }

                if (NormalBorder != Color.Transparent)
                {
                    using (Pen pen = new Pen(NormalBorder, 1f))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            TextRenderer.DrawText(g, this.Text, this.Font, rect, NormalText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordEllipsis | TextFormatFlags.NoPrefix);
        }
    }

    public class SmoothPanel : Panel
    {
        public SmoothPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }
}
