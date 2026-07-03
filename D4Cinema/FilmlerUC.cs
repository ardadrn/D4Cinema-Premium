using FontAwesome.Sharp;
using System;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace D4Cinema
{
    public partial class FilmlerUC : UserControl
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        private Panel pnlViewport, pnlContent, pnlScrollTrack, pnlScrollThumb;
        private int scrollY = 0;
        private bool isThumbDragging = false;
        private int thumbStartY = 0, mouseStartY = 0;

        private FlowLayoutPanel pnlKartlar;
        private FlowLayoutPanel pnlButonlar;
        private IconButton btnVizyondakiler, btnYakindakiler;

        public FilmlerUC()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(18, 18, 22);

            this.Padding = new Padding(0, 90, 0, 0);

            DoubleBufferAktifEt(this);

            ArayuzuHazirla();
            VerileriYukle("Vizyonda");
        }

        public static void DoubleBufferAktifEt(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        private void ArayuzuHazirla()
        {
            pnlScrollTrack = new Panel() { Dock = DockStyle.Right, Width = 12, BackColor = Color.FromArgb(22, 22, 26) };
            this.Controls.Add(pnlScrollTrack);

            pnlScrollThumb = new Panel() { Width = 8, Height = 100, Left = 2, Top = 2, BackColor = Color.FromArgb(120, 40, 140), Cursor = Cursors.Hand };
            pnlScrollThumb.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlScrollThumb.Width, pnlScrollThumb.Height, 8, 8));
            pnlScrollTrack.Controls.Add(pnlScrollThumb);

            pnlScrollThumb.MouseDown += (s, e) => { isThumbDragging = true; thumbStartY = pnlScrollThumb.Top; mouseStartY = Cursor.Position.Y; };
            pnlScrollThumb.MouseMove += (s, e) => {
                if (isThumbDragging)
                {
                    int maxTop = pnlScrollTrack.Height - pnlScrollThumb.Height;
                    if (maxTop <= 0) return;
                    int newTop = thumbStartY + (Cursor.Position.Y - mouseStartY);
                    if (newTop < 0) newTop = 0;
                    if (newTop > maxTop) newTop = maxTop;
                    pnlScrollThumb.Top = newTop;
                    float oran = (float)newTop / maxTop;
                    scrollY = (int)(oran * (pnlContent.Height - pnlViewport.Height));
                    pnlContent.Top = -scrollY;
                }
            };
            pnlScrollThumb.MouseUp += (s, e) => { isThumbDragging = false; };
            pnlScrollTrack.MouseUp += (s, e) => { isThumbDragging = false; };

            pnlViewport = new Panel() { Dock = DockStyle.Fill, AutoScroll = false, BackColor = Color.FromArgb(18, 18, 22) };
            DoubleBufferAktifEt(pnlViewport);
            this.Controls.Add(pnlViewport);

            pnlContent = new Panel() { Location = new Point(0, 0), Width = pnlViewport.Width, Height = 1500, BackColor = Color.FromArgb(18, 18, 22) };
            DoubleBufferAktifEt(pnlContent);
            pnlViewport.Controls.Add(pnlContent);

            pnlButonlar = new FlowLayoutPanel() { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Location = new Point(40, 20), Margin = new Padding(0), BackColor = Color.FromArgb(18, 18, 22) };

            btnVizyondakiler = SekmeButonuTasarla(IconChar.Fire, "VİZYONDA", true);
            btnVizyondakiler.Click += (s, e) => SekmeGuncelle("Vizyonda");
            pnlButonlar.Controls.Add(btnVizyondakiler);

            btnYakindakiler = SekmeButonuTasarla(IconChar.HourglassHalf, "YAKINDA", false);
            btnYakindakiler.Click += (s, e) => SekmeGuncelle("Yakinda");
            pnlButonlar.Controls.Add(btnYakindakiler);

            pnlContent.Controls.Add(pnlButonlar);

            pnlKartlar = new FlowLayoutPanel()
            {
                Location = new Point(40, 130),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.FromArgb(18, 18, 22)
            };
            DoubleBufferAktifEt(pnlKartlar);
            pnlContent.Controls.Add(pnlKartlar);

            pnlViewport.MouseWheel += Icerik_Kaydir;

            this.Resize += (s, e) => {
                pnlContent.Width = pnlViewport.Width;
                pnlKartlar.MaximumSize = new Size(pnlContent.Width - 80, 0);
                pnlKartlar.Width = pnlContent.Width - 80;
                YukseklikGuncelle();
            };
        }

        private void Icerik_Kaydir(object sender, MouseEventArgs e)
        {
            int maxScroll = pnlContent.Height - pnlViewport.Height;
            if (maxScroll <= 0) return;

            int kaydirmaHizi = 60;
            scrollY -= (e.Delta > 0 ? kaydirmaHizi : -kaydirmaHizi);

            if (scrollY < 0) scrollY = 0;
            if (scrollY > maxScroll) scrollY = maxScroll;

            pnlContent.Top = -scrollY;
            ScrollGuncelle();
        }

        private void ScrollGuncelle()
        {
            int maxScroll = pnlContent.Height - pnlViewport.Height;
            if (maxScroll <= 0)
            {
                pnlScrollThumb.Visible = false;
                pnlContent.Top = 0;
                scrollY = 0;
                return;
            }

            pnlScrollThumb.Visible = true;
            int trackBoyu = pnlScrollTrack.Height;
            int thumbBoyu = Math.Max(40, (int)((float)trackBoyu * pnlViewport.Height / pnlContent.Height));
            pnlScrollThumb.Height = thumbBoyu;

            if (!isThumbDragging)
            {
                float oran = (float)scrollY / maxScroll;
                int yenitop = (int)(oran * (trackBoyu - thumbBoyu));
                if (yenitop < 0) yenitop = 0;
                if (yenitop > trackBoyu - thumbBoyu) yenitop = trackBoyu - thumbBoyu;
                pnlScrollThumb.Top = yenitop;
            }
        }

        private void YukseklikGuncelle()
        {
            pnlContent.Height = pnlKartlar.Bottom + 150;
            ScrollGuncelle();
        }

        private void HerYereScrollEkle(Control nesne)
        {
            nesne.MouseWheel -= Icerik_Kaydir;
            nesne.MouseWheel += Icerik_Kaydir;
            foreach (Control cocuk in nesne.Controls) HerYereScrollEkle(cocuk);
        }

        private IconButton SekmeButonuTasarla(IconChar ikon, string metin, bool aktifMi)
        {
            IconButton btn = new IconButton()
            {
                Text = metin,
                IconChar = ikon,
                IconSize = 24,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Size = new Size(220, 45),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 15, 0),
                IconColor = aktifMi ? Color.White : Color.Gray
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 12, 12));
            btn.BackColor = aktifMi ? Color.FromArgb(120, 40, 140) : Color.FromArgb(28, 28, 34);
            btn.ForeColor = aktifMi ? Color.White : Color.Gray;
            return btn;
        }

        private void SekmeGuncelle(string durum)
        {
            bool vizyonMu = (durum == "Vizyonda");

            btnVizyondakiler.BackColor = vizyonMu ? Color.FromArgb(120, 40, 140) : Color.FromArgb(28, 28, 34);
            btnVizyondakiler.ForeColor = vizyonMu ? Color.White : Color.Gray;
            btnVizyondakiler.IconColor = vizyonMu ? Color.White : Color.Gray;

            btnYakindakiler.BackColor = !vizyonMu ? Color.FromArgb(120, 40, 140) : Color.FromArgb(28, 28, 34);
            btnYakindakiler.ForeColor = !vizyonMu ? Color.White : Color.Gray;
            btnYakindakiler.IconColor = !vizyonMu ? Color.White : Color.Gray;

            scrollY = 0;
            pnlContent.Top = 0;
            VerileriYukle(durum);
        }

        private void VerileriYukle(string durum)
        {
            while (pnlKartlar.Controls.Count > 0) pnlKartlar.Controls[0].Dispose();

            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                using (SQLiteCommand komut = new SQLiteCommand("SELECT * FROM Filmler WHERE Durum = @durum OR (Durum = 'Yakında' AND @durum = 'Yakinda')", baglan))
                {
                    komut.Parameters.AddWithValue("@durum", durum);
                    using (SQLiteDataReader oku = komut.ExecuteReader())
                    {
                        while (oku.Read())
                        {
                            int id = Convert.ToInt32(oku["ID"]);
                            string ad = oku["FilmAdi"].ToString();
                            string tur = oku["Tur"].ToString();
                            string afis = oku["AfisYolu"].ToString();
                            string sure = oku["Sure"].ToString();
                            string vizyonTarihi = oku["VizyonTarihi"].ToString();

                            pnlKartlar.Controls.Add(FilmKartiOlustur(id, ad, tur, sure, afis, durum, vizyonTarihi));
                        }
                    }
                }
            }

            pnlKartlar.MaximumSize = new Size(pnlContent.Width - 80, 0);
            pnlKartlar.Width = pnlContent.Width - 80;
            YukseklikGuncelle();
            HerYereScrollEkle(pnlKartlar);
        }

        private Panel FilmKartiOlustur(int id, string ad, string tur, string sure, string afisYolu, string durum, string vizyonTarihi)
        {
            Panel pnl = new Panel() { Size = new Size(240, 460), BackColor = Color.FromArgb(28, 28, 34), Margin = new Padding(0, 0, 30, 30) };
            pnl.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnl.Width, pnl.Height, 15, 15));
            DoubleBufferAktifEt(pnl);

            PictureBox pb = new PictureBox() { Size = new Size(240, 320), Location = new Point(0, 0), SizeMode = PictureBoxSizeMode.StretchImage, BackColor = Color.FromArgb(28, 28, 34) };
            pb.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pb.Width, pb.Height + 20, 15, 15));

            string afisTamYolu = AppPaths.GetAfisPath(afisYolu);
            if (!string.IsNullOrEmpty(afisTamYolu) && File.Exists(afisTamYolu))
                pb.Image = AppPaths.LoadImageWithoutLock(afisTamYolu);
            pnl.Controls.Add(pb);

            Label lblAd = new Label() { Text = ad, ForeColor = Color.White, BackColor = Color.FromArgb(28, 28, 34), Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(15, 325), Size = new Size(210, 25), AutoEllipsis = true };
            pnl.Controls.Add(lblAd);

            if (durum == "Vizyonda")
            {
                pnl.Controls.Add(new Label() { Text = sure, ForeColor = Color.FromArgb(200, 200, 200), BackColor = Color.FromArgb(28, 28, 34), Font = new Font("Segoe UI", 9), Location = new Point(15, 355), AutoSize = true });
                pnl.Controls.Add(new Label() { Text = tur, ForeColor = Color.FromArgb(150, 150, 160), BackColor = Color.FromArgb(28, 28, 34), Font = new Font("Segoe UI", 9), Location = new Point(15, 375), AutoSize = true });
            }
            else
            {
                pnl.Controls.Add(new Label() { Text = "Vizyon Tarihi: " + vizyonTarihi, ForeColor = Color.FromArgb(200, 200, 200), BackColor = Color.FromArgb(28, 28, 34), Font = new Font("Segoe UI", 9), Location = new Point(15, 355), AutoSize = true });
            }

            Button btn = new Button()
            {
                Text = "Bilet Al",
                Size = new Size(200, 40),
                Location = new Point(20, 405),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(120, 40, 140),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 15, 15));

            btn.Click += (s, e) => { FilmDetayUC detay = new FilmDetayUC(id); this.Parent.Controls.Add(detay); detay.BringToFront(); };

            pnl.Controls.Add(btn);

            Timer glowTimer = new Timer() { Interval = 15 };
            int currentAlpha = 0, targetAlpha = 0;

            glowTimer.Tick += (s, e) => {
                if (currentAlpha < targetAlpha) { currentAlpha += 15; if (currentAlpha > targetAlpha) currentAlpha = targetAlpha; pb.Invalidate(); }
                else if (currentAlpha > targetAlpha) { currentAlpha -= 15; if (currentAlpha < targetAlpha) currentAlpha = targetAlpha; pb.Invalidate(); }
                else { glowTimer.Stop(); }
            };

            pb.Paint += (s, e) => {
                if (currentAlpha > 0)
                {
                    using (LinearGradientBrush brush = new LinearGradientBrush(pb.ClientRectangle, Color.Transparent, Color.FromArgb(currentAlpha, 145, 55, 165), 90f))
                        e.Graphics.FillRectangle(brush, pb.ClientRectangle);
                }
            };

            btn.MouseEnter += (s, e) => { targetAlpha = 220; glowTimer.Start(); btn.BackColor = Color.FromArgb(145, 55, 165); };
            btn.MouseLeave += (s, e) => { targetAlpha = 0; glowTimer.Start(); btn.BackColor = Color.FromArgb(120, 40, 140); };

            return pnl;
        }
    }
}