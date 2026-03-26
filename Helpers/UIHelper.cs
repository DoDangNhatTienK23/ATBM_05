using System;
using System.Drawing;
using System.Windows.Forms;

namespace OracleAdminApp.Helpers
{
    public static class UIHelper
    {
        public static readonly Color Primary      = Color.FromArgb(30, 90, 200);
        public static readonly Color PrimaryHover = Color.FromArgb(20, 70, 165);
        public static readonly Color Danger       = Color.FromArgb(200, 50, 50);
        public static readonly Color DangerHover  = Color.FromArgb(165, 30, 30);
        public static readonly Color Success      = Color.FromArgb(34, 150, 90);
        public static readonly Color Warning      = Color.FromArgb(200, 130, 0);
        public static readonly Color LightBg      = Color.FromArgb(240, 242, 245);
        public static readonly Color CardBg       = Color.White;
        public static readonly Color Border       = Color.FromArgb(210, 215, 225);
        public static readonly Color TextDark     = Color.FromArgb(30, 40, 60);
        public static readonly Color TextMuted    = Color.FromArgb(120, 130, 150);

        // ── Section Header ───────────────────────────────────────────────────
        public static Panel CreateSectionHeader(string title, string subtitle = "")
        {
            var pnl = new Panel { Height = 60, Dock = DockStyle.Top, BackColor = Color.Transparent };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(0, 4)
            };
            pnl.Controls.Add(lblTitle);

            if (!string.IsNullOrEmpty(subtitle))
            {
                var lblSub = new Label
                {
                    Text = subtitle,
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = TextMuted,
                    AutoSize = true,
                    Location = new Point(0, 32)
                };
                pnl.Controls.Add(lblSub);
            }

            var line = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Border };
            pnl.Controls.Add(line);
            return pnl;
        }

        // ── Card Panel ────────────────────────────────────────────────────────
        public static Panel CreateCard(int x, int y, int w, int h, string cardTitle = "")
        {
            var outer = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            if (!string.IsNullOrEmpty(cardTitle))
            {
                var lbl = new Label
                {
                    Text = cardTitle,
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    ForeColor = TextMuted,
                    AutoSize = true,
                    Location = new Point(10, 8)
                };
                outer.Controls.Add(lbl);

                var sep = new Panel
                {
                    Location = new Point(0, 26),
                    Size = new Size(w, 1),
                    BackColor = Border
                };
                outer.Controls.Add(sep);
            }
            return outer;
        }

        // ── Button ────────────────────────────────────────────────────────────
        public static Button CreateButton(string text, ButtonStyle style = ButtonStyle.Primary)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = false,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;

            Color bg = Primary, hover = PrimaryHover;
            switch (style)
            {
                case ButtonStyle.Danger:
                    bg = Danger; hover = DangerHover; break;
                case ButtonStyle.Success:
                    bg = Success; hover = Color.FromArgb(25, 120, 70); break;
                case ButtonStyle.Warning:
                    bg = Warning; hover = Color.FromArgb(170, 105, 0); break;
                case ButtonStyle.Secondary:
                    bg = Color.FromArgb(220, 225, 235);
                    hover = Color.FromArgb(200, 207, 220);
                    btn.ForeColor = TextDark;
                    break;
            }
            btn.BackColor = bg;
            btn.MouseEnter += (s, e) => btn.BackColor = hover;
            btn.MouseLeave += (s, e) => btn.BackColor = bg;
            return btn;
        }

        // ── Labeled TextBox ───────────────────────────────────────────────────
        // Returns TextBox via out parameter (C# 7.3 compatible)
        public static void CreateLabeledInput(
            Panel parent, string labelText, int x, int y, int width,
            out TextBox txt, bool isPassword = false)
        {
            var lbl = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8.5f)
            };
            txt = new TextBox
            {
                Location = new Point(x, y + 18),
                Size = new Size(width, 26),
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.FromArgb(248, 250, 255),
                BorderStyle = BorderStyle.FixedSingle
            };
            if (isPassword) txt.PasswordChar = '●';
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
        }

        // ── Labeled ComboBox ──────────────────────────────────────────────────
        public static void CreateLabeledCombo(
            Panel parent, string labelText, int x, int y, int width,
            out ComboBox cmb)
        {
            var lbl = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 8.5f)
            };
            cmb = new ComboBox
            {
                Location = new Point(x, y + 18),
                Size = new Size(width, 26),
                Font = new Font("Segoe UI", 9.5f),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(248, 250, 255)
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(cmb);
        }

        // ── DataGridView ──────────────────────────────────────────────────────
        public static DataGridView CreateGrid()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = CardBg,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9f),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                GridColor = Color.FromArgb(225, 228, 235)
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 60, 95);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 60, 95);
            dgv.ColumnHeadersHeight = 36;

            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = TextDark;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 225, 255);
            dgv.DefaultCellStyle.SelectionForeColor = TextDark;
            dgv.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            dgv.RowTemplate.Height = 30;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 255);

            return dgv;
        }

        // ── Status Label ──────────────────────────────────────────────────────
        public static Label CreateStatusLabel(Panel parent)
        {
            var lbl = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = TextMuted,
                Padding = new Padding(5, 0, 0, 0),
                BackColor = Color.FromArgb(235, 238, 244)
            };
            parent.Controls.Add(lbl);
            return lbl;
        }

        public static void SetStatus(Label lbl, string msg, StatusType type = StatusType.Info)
        {
            lbl.Text = msg;
            switch (type)
            {
                case StatusType.Success: lbl.ForeColor = Success; break;
                case StatusType.Error:   lbl.ForeColor = Danger;  break;
                case StatusType.Warning: lbl.ForeColor = Warning; break;
                default:                 lbl.ForeColor = TextMuted; break;
            }
        }
    }

    public enum ButtonStyle { Primary, Danger, Success, Warning, Secondary }
    public enum StatusType  { Info, Success, Error, Warning }
}
