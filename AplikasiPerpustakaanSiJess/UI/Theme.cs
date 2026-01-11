using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AplikasiPerpustakaanSiJess.UI
{
    public static class Theme
    {
        // =========================
        // 1) COLOR PALETTE (Modern Blue)
        // =========================
        public static Color Bg = Color.FromArgb(224, 242, 254);          // biru muda (background)
        public static Color Card = Color.White;                          // kartu/panel putih
        public static Color Primary = Color.FromArgb(37, 99, 235);        // biru utama
        public static Color PrimaryHover = Color.FromArgb(29, 78, 216);   // hover
        public static Color Border = Color.FromArgb(203, 213, 225);       // abu border
        public static Color Text = Color.FromArgb(15, 23, 42);            // teks utama
        public static Color Muted = Color.FromArgb(100, 116, 139);        // teks sekunder
        public static Color Danger = Color.FromArgb(220, 38, 38);         // merah
        public static Color Success = Color.FromArgb(22, 163, 74);        // hijau

        // =========================
        // 2) FONTS (lebih modern)
        // =========================
        public static readonly Font H1 = new Font("Segoe UI Semibold", 16f);
        public static readonly Font H2 = new Font("Segoe UI Semibold", 11f);
        public static readonly Font Body = new Font("Segoe UI", 9.5f);
        public static readonly Font Small = new Font("Segoe UI", 8.5f);

        // =========================
        // 3) APPLY THEME HELPER
        // =========================
        public static void ApplyForm(Form f)
        {
            f.BackColor = Bg;
            f.Font = Body;
        }

        public static Panel MakeCard(int w, int h, int left, int top)
        {
            var p = new Panel
            {
                Width = w,
                Height = h,
                Left = left,
                Top = top,
                BackColor = Card
            };
            p.Paint += (s, e) =>
            {
                // border halus
                using (var pen = new Pen(Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };
            return p;
        }

        public static void StylePrimaryButton(Button b)
        {
            b.BackColor = Primary;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Height = Math.Max(b.Height, 36);
            b.Font = H2;
            b.Cursor = Cursors.Hand;

            b.MouseEnter += (_, __) => b.BackColor = PrimaryHover;
            b.MouseLeave += (_, __) => b.BackColor = Primary;
        }

        public static void StyleSecondaryButton(Button b)
        {
            b.BackColor = Color.White;
            b.ForeColor = Text;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = Border;
            b.FlatAppearance.BorderSize = 1;
            b.Height = Math.Max(b.Height, 36);
            b.Font = H2;
            b.Cursor = Cursors.Hand;
        }

        public static void StyleTextBox(TextBox tb)
        {
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.Font = Body;
            tb.BackColor = Color.White;
        }

        public static void StyleComboBox(ComboBox cb)
        {
            cb.Font = Body;
            cb.BackColor = Color.White;
            cb.FlatStyle = FlatStyle.Standard;
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public static void StyleGrid(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(239, 246, 255);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Text;
            dgv.ColumnHeadersDefaultCellStyle.Font = H2;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dgv.DefaultCellStyle.Font = Body;
            dgv.DefaultCellStyle.ForeColor = Text;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = Text;
            dgv.GridColor = Color.FromArgb(226, 232, 240);
        }

        // =========================
        // 4) OPTIONAL: Rounded Corners helper
        // =========================
        public static void RoundControl(Control c, int radius = 12)
        {
            c.Resize += (_, __) => ApplyRoundedRegion(c, radius);
            ApplyRoundedRegion(c, radius);
        }

        private static void ApplyRoundedRegion(Control c, int radius)
        {
            if (c.Width <= 0 || c.Height <= 0) return;

            using (var path = new GraphicsPath())
            {
                int d = radius * 2;
                path.StartFigure();
                path.AddArc(0, 0, d, d, 180, 90);
                path.AddArc(c.Width - d, 0, d, d, 270, 90);
                path.AddArc(c.Width - d, c.Height - d, d, d, 0, 90);
                path.AddArc(0, c.Height - d, d, d, 90, 90);
                path.CloseFigure();
                c.Region = new Region(path);
            }
        }
    }
}
