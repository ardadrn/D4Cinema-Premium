using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data.SQLite;

namespace D4Cinema
{
    public partial class AnaSayfaUC : UserControl
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
        private FlowLayoutPanel flpVizyon, flpYakinda, flpKampanyalar;
        private Panel pnlKampanyalarWrapper;
        private int scrollY = 0;
        private bool isDragging = false;
        private int startMouseX, startPanelX;
        private FlowLayoutPanel draggedPanel;

        public AnaSayfaUC()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(18, 18, 22);
            this.AutoScroll = false;
            this.Padding = new Padding(0, 90, 0, 0);

            DoubleBufferAktifEt(this);

            ArayuzuCiz();
        }

        public static void DoubleBufferAktifEt(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        private void ArayuzuCiz()
        {
            pnlScrollTrack = new Panel() { Dock = DockStyle.Right, Width = 12, BackColor = Color.FromArgb(22, 22, 26) };
            this.Controls.Add(pnlScrollTrack);

            pnlScrollThumb = new Panel() { Width = 8, Height = 100, Left = 2, Top = 2, BackColor = Color.FromArgb(120, 40, 140), Cursor = Cursors.Hand };
            pnlScrollThumb.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlScrollThumb.Width, pnlScrollThumb.Height, 8, 8));
            pnlScrollTrack.Controls.Add(pnlScrollThumb);

            pnlViewport = new Panel() { Dock = DockStyle.Fill, AutoScroll = false, BackColor = Color.FromArgb(18, 18, 22) };
            DoubleBufferAktifEt(pnlViewport);
            this.Controls.Add(pnlViewport);

            pnlContent = new Panel() { Location = new Point(0, 0), Width = pnlViewport.Width, Height = 1800, BackColor = Color.FromArgb(18, 18, 22) };
            DoubleBufferAktifEt(pnlContent);
            pnlViewport.Controls.Add(pnlContent);

            this.Resize += (s, e) => {
                pnlContent.Width = pnlViewport.Width;
                ScrollGuncelle();
            };

            IconPictureBox ikonVizyon = new IconPictureBox() { IconChar = IconChar.Fire, IconColor = Color.FromArgb(145, 55, 165), IconSize = 32, Location = new Point(40, 20), AutoSize = true, BackColor = Color.FromArgb(18, 18, 22) };
            pnlContent.Controls.Add(ikonVizyon);

            Label lblVizyon = new Label() { Text = "Vizyonda", ForeColor = Color.White, BackColor = Color.FromArgb(18, 18, 22), Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true, Location = new Point(75, 18) };
            pnlContent.Controls.Add(lblVizyon);

            Panel pnlVizyonWrapper = new Panel() { Location = new Point(40, 60), Height = 460, AutoScroll = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.FromArgb(18, 18, 22) };
            pnlVizyonWrapper.Width = pnlContent.Width - 60;
            DoubleBufferAktifEt(pnlVizyonWrapper);
            pnlContent.Controls.Add(pnlVizyonWrapper);

            flpVizyon = new FlowLayoutPanel() { Location = new Point(0, 0), Height = 460, AutoSize = true, WrapContents = false, BackColor = Color.FromArgb(18, 18, 22), AutoScroll = false };
            DoubleBufferAktifEt(flpVizyon);
            pnlVizyonWrapper.Controls.Add(flpVizyon);
            SuruklemeOlaylariniEkle(flpVizyon);

            IconPictureBox ikonYakinda = new IconPictureBox() { IconChar = IconChar.HourglassHalf, IconColor = Color.FromArgb(145, 55, 165), IconSize = 32, Location = new Point(40, 540), AutoSize = true, BackColor = Color.FromArgb(18, 18, 22) };
            pnlContent.Controls.Add(ikonYakinda);

            Label lblYakinda = new Label() { Text = "Yakında", ForeColor = Color.White, BackColor = Color.FromArgb(18, 18, 22), Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true, Location = new Point(75, 538) };
            pnlContent.Controls.Add(lblYakinda);

            Panel pnlYakindaWrapper = new Panel() { Location = new Point(40, 580), Height = 460, AutoScroll = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.FromArgb(18, 18, 22) };
            pnlYakindaWrapper.Width = pnlContent.Width - 60;
            DoubleBufferAktifEt(pnlYakindaWrapper);
            pnlContent.Controls.Add(pnlYakindaWrapper);

            flpYakinda = new FlowLayoutPanel() { Location = new Point(0, 0), Height = 460, AutoSize = true, WrapContents = false, BackColor = Color.FromArgb(18, 18, 22), AutoScroll = false };
            DoubleBufferAktifEt(flpYakinda);
            pnlYakindaWrapper.Controls.Add(flpYakinda);
            SuruklemeOlaylariniEkle(flpYakinda);

            IconPictureBox ikonCampanya = new IconPictureBox() { IconChar = IconChar.Gift, IconColor = Color.FromArgb(145, 55, 165), IconSize = 32, Location = new Point(40, 1060), AutoSize = true, BackColor = Color.FromArgb(18, 18, 22) };
            pnlContent.Controls.Add(ikonCampanya);

            Label lblKampanyalar = new Label() { Text = "Güncel Kampanyalar", ForeColor = Color.White, BackColor = Color.FromArgb(18, 18, 22), Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true, Location = new Point(75, 1058) };
            pnlContent.Controls.Add(lblKampanyalar);

            pnlKampanyalarWrapper = new Panel() { Location = new Point(40, 1100), Height = 450, AutoScroll = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.FromArgb(18, 18, 22) };
            pnlKampanyalarWrapper.Width = pnlContent.Width - 60;
            DoubleBufferAktifEt(pnlKampanyalarWrapper);
            pnlContent.Controls.Add(pnlKampanyalarWrapper);

            flpKampanyalar = new FlowLayoutPanel() { Location = new Point(0, 0), Height = 450, AutoSize = true, WrapContents = false, BackColor = Color.FromArgb(18, 18, 22), AutoScroll = false };
            DoubleBufferAktifEt(flpKampanyalar);
            pnlKampanyalarWrapper.Controls.Add(flpKampanyalar);
            SuruklemeOlaylariniEkle(flpKampanyalar);

            VerileriYukle();
        }

        private void VerileriYukle()
        {
            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                using (SQLiteCommand komut = new SQLiteCommand("SELECT * FROM Filmler", baglan))
                {
                    using (SQLiteDataReader oku = komut.ExecuteReader())
                    {
                        while (oku.Read())
                        {
                            int id = Convert.ToInt32(oku["ID"]);
                            string ad = oku["FilmAdi"].ToString();
                            string tur = oku["Tur"].ToString();
                            string afis = oku["AfisYolu"].ToString();
                            string durum = oku["Durum"].ToString();
                            string sure = oku["Sure"].ToString();

                            Panel kart = FilmKartiOlustur(id, ad, tur, sure, afis);

                            if (durum == "Vizyonda") flpVizyon.Controls.Add(kart);
                            else if (durum == "Yakında" || durum == "Yakinda") flpYakinda.Controls.Add(kart);
                        }
                    }
                }
            }

            var kampanyalar = new List<dynamic>
            {
                new { Baslik = "Öğrenciye %50 İndirim", Detay = "Hafta içi saat 16:00'a kadar tüm seanslarda öğrenci kimliğini göster, yarı fiyatına sinema keyfini yaşa.", Ikon = IconChar.UserGraduate, Resim = "ogrenci.png" },
                new { Baslik = "Valhalla Metal Pub Öncesi", Detay = "Sinema biletini gösteren herkese Valhalla Metal Pub'da ilk içecek müesseseden! Eğlenceye doy.", Ikon = IconChar.BeerMugEmpty, Resim = "valhalla.png" },
                new { Baslik = "Gece Kuşlarına Özel", Detay = "22:00 ve sonrası tüm geç seanslarda orta boy mısır menüsü bizden hediye! Uykusuz kalmaya değer.", Ikon = IconChar.Moon, Resim = "gece_kusu.png" },
                new { Baslik = "Çift Kişilik Bilet Fırsatı", Detay = "Özel haftalara özel, yan yana iki koltuk alımında ikinci bilet anında %30 indirimli.", Ikon = IconChar.Heart, Resim = "cift_kisilik.png" },
                new { Baslik = "D4 Sadakat Programı", Detay = "D4Cinema hesabınla aldığın her 5 biletin ardından 1 adet dilediğin filme bedava bilet kazan.", Ikon = IconChar.Crown, Resim = "sadakat.png" },
                new { Baslik = "Ön Gösterim Ayrıcalığı", Detay = "Beklenen Marvel ve DC filmlerini herkesten tam 1 gün önce, VIP salonlarımızda izleme şansı.", Ikon = IconChar.TicketAlt, Resim = "ongosterim.png" }
            };

            foreach (var kampanya in kampanyalar)
            {
                flpKampanyalar.Controls.Add(KampanyaKartiOlustur(kampanya.Baslik, kampanya.Detay, kampanya.Ikon, kampanya.Resim));
            }

            pnlContent.Height = pnlKampanyalarWrapper.Bottom + 100;
            HerYereScrollEkle(pnlContent);
            ScrollGuncelle();
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
            if (maxScroll <= 0) { pnlScrollThumb.Visible = false; return; }
            pnlScrollThumb.Visible = true;

            int trackBoyu = pnlScrollTrack.Height;
            int thumbBoyu = Math.Max(40, (int)((float)trackBoyu * pnlViewport.Height / pnlContent.Height));
            pnlScrollThumb.Height = thumbBoyu;

            float oran = (float)scrollY / maxScroll;
            int yenitop = (int)(oran * (trackBoyu - thumbBoyu));

            if (yenitop < 0) yenitop = 0;
            if (yenitop > trackBoyu - thumbBoyu) yenitop = trackBoyu - thumbBoyu;

            pnlScrollThumb.Top = yenitop;
        }

        private void HerYereScrollEkle(Control nesne)
        {
            nesne.MouseWheel -= Icerik_Kaydir;
            nesne.MouseWheel += Icerik_Kaydir;
            foreach (Control cocuk in nesne.Controls) HerYereScrollEkle(cocuk);
        }

        private void SuruklemeOlaylariniEkle(Control parent)
        {
            parent.MouseDown += Pan_MouseDown;
            parent.MouseMove += Pan_MouseMove;
            parent.MouseUp += Pan_MouseUp;
        }

        private void Pan_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                startMouseX = Cursor.Position.X;

                Control c = sender as Control;
                while (c != null && !(c is FlowLayoutPanel)) c = c.Parent;

                if (c is FlowLayoutPanel)
                {
                    draggedPanel = (FlowLayoutPanel)c;
                    startPanelX = draggedPanel.Left;
                }
            }
        }

        private void Pan_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedPanel != null)
            {
                int fark = Cursor.Position.X - startMouseX;
                int yeniSol = startPanelX + fark;

                int minSol = draggedPanel.Parent.Width - draggedPanel.Width;
                if (minSol > 0) minSol = 0;

                if (yeniSol > 0) yeniSol = 0;
                if (yeniSol < minSol) yeniSol = minSol;

                if (draggedPanel.Left != yeniSol)
                {
                    draggedPanel.Left = yeniSol;
                }
            }
        }

        private void Pan_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            draggedPanel = null;
        }

        private Panel FilmKartiOlustur(int id, string ad, string tur, string sure, string afisYolu)
        {
            Panel pnl = new Panel() { Size = new Size(240, 440), BackColor = Color.FromArgb(28, 28, 34), Margin = new Padding(0, 0, 30, 0) };
            pnl.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnl.Width, pnl.Height, 15, 15));
            DoubleBufferAktifEt(pnl);

            PictureBox pb = new PictureBox() { Size = new Size(240, 320), Location = new Point(0, 0), SizeMode = PictureBoxSizeMode.StretchImage, BackColor = Color.FromArgb(28, 28, 34) };
            if (!string.IsNullOrEmpty(afisYolu))
            {
                string tamYol = Path.Combine(Application.StartupPath, "Afisler", afisYolu);
                if (File.Exists(tamYol)) pb.Image = Image.FromFile(tamYol);
            }

            Label lblAd = new Label() { Text = ad, ForeColor = Color.White, BackColor = Color.FromArgb(28, 28, 34), Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(15, 325), AutoSize = false, Size = new Size(210, 25), AutoEllipsis = true };
            Label lblSure = new Label() { Text = sure, ForeColor = Color.FromArgb(200, 200, 200), BackColor = Color.FromArgb(28, 28, 34), Font = new Font("Segoe UI", 9, FontStyle.Regular), Location = new Point(15, 350), AutoSize = true };
            Label lblTur = new Label() { Text = tur, ForeColor = Color.FromArgb(150, 150, 160), BackColor = Color.FromArgb(28, 28, 34), Font = new Font("Segoe UI", 9, FontStyle.Regular), Location = new Point(15, 370), AutoSize = true };

            Button btnBilet = new Button()
            {
                Text = "Bilet Al",
                BackColor = Color.FromArgb(120, 40, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(200, 40),
                Location = new Point(20, 390),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnBilet.FlatAppearance.BorderSize = 0;
            btnBilet.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnBilet.Width, btnBilet.Height, 15, 15));

            SuruklemeOlaylariniEkle(pnl);
            SuruklemeOlaylariniEkle(pb);
            SuruklemeOlaylariniEkle(lblAd);
            SuruklemeOlaylariniEkle(lblSure);
            SuruklemeOlaylariniEkle(lblTur);

            pnl.MouseWheel += Icerik_Kaydir;
            pb.MouseWheel += Icerik_Kaydir;
            lblAd.MouseWheel += Icerik_Kaydir;
            lblSure.MouseWheel += Icerik_Kaydir;
            lblTur.MouseWheel += Icerik_Kaydir;
            btnBilet.MouseWheel += Icerik_Kaydir;

            Timer glowTimer = new Timer() { Interval = 10 };
            int currentAlpha = 0;
            int targetAlpha = 0;

            glowTimer.Tick += (s, e) => {
                if (currentAlpha < targetAlpha)
                {
                    currentAlpha += 15;
                    if (currentAlpha > targetAlpha) currentAlpha = targetAlpha;
                    pb.Invalidate();
                }
                else if (currentAlpha > targetAlpha)
                {
                    currentAlpha -= 15;
                    if (currentAlpha < targetAlpha) currentAlpha = targetAlpha;
                    pb.Invalidate();
                }
                else { glowTimer.Stop(); }
            };

            pb.Paint += (s, e) => {
                if (currentAlpha > 0)
                {
                    using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(pb.ClientRectangle, Color.Transparent, Color.FromArgb(currentAlpha, 165, 50, 185), 90f))
                    {
                        e.Graphics.FillRectangle(brush, pb.ClientRectangle);
                    }
                }
            };

            btnBilet.MouseEnter += (s, e) => { targetAlpha = 220; glowTimer.Start(); btnBilet.BackColor = Color.FromArgb(145, 55, 165); };
            btnBilet.MouseLeave += (s, e) => { targetAlpha = 0; glowTimer.Start(); btnBilet.BackColor = Color.FromArgb(120, 40, 140); };

            btnBilet.Click += (s, e) => {
                FilmDetayUC detaySayfasi = new FilmDetayUC(id);
                this.Parent.Controls.Add(detaySayfasi);
                detaySayfasi.BringToFront();
            };

            pnl.Controls.Add(pb);
            pnl.Controls.Add(lblAd);
            pnl.Controls.Add(lblSure);
            pnl.Controls.Add(lblTur);
            pnl.Controls.Add(btnBilet);

            return pnl;
        }

        private Panel KampanyaKartiOlustur(string baslik, string detay, IconChar ikon, string resimAdi)
        {
            Panel pnlKart = new Panel() { Size = new Size(360, 420), Margin = new Padding(0, 0, 30, 0), BackColor = Color.FromArgb(28, 28, 34) };
            pnlKart.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlKart.Width, pnlKart.Height, 15, 15));

            PictureBox pbVitrin = new PictureBox() { Dock = DockStyle.Top, Height = 180, BackColor = Color.FromArgb(20, 20, 25), SizeMode = PictureBoxSizeMode.StretchImage };

            string resimKlasoru = Path.Combine(Application.StartupPath, "Kampanyalar");
            string resimYolu = Path.Combine(resimKlasoru, resimAdi);

            if (File.Exists(resimYolu))
            {
                pbVitrin.Image = Image.FromFile(resimYolu);
            }
            else
            {
                pbVitrin.Paint += (s, e) => {
                    using (LinearGradientBrush firca = new LinearGradientBrush(pbVitrin.ClientRectangle, Color.FromArgb(90, 20, 100), Color.FromArgb(20, 20, 25), 90f))
                    {
                        e.Graphics.FillRectangle(firca, pbVitrin.ClientRectangle);
                    }
                };

                IconPictureBox pbIcon = new IconPictureBox() { IconChar = ikon, IconColor = Color.FromArgb(180, 80, 200), IconSize = 80, Size = new Size(80, 80), BackColor = Color.FromArgb(20, 20, 25) };
                pbIcon.Location = new Point((pbVitrin.Width - pbIcon.Width) / 2, (pbVitrin.Height - pbIcon.Height) / 2);
                pbVitrin.Controls.Add(pbIcon);
            }

            pnlKart.Controls.Add(pbVitrin);

            Label lblBaslik = new Label() { Text = baslik, ForeColor = Color.White, BackColor = Color.FromArgb(28, 28, 34), Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 200), Size = new Size(320, 60), AutoEllipsis = true, TextAlign = ContentAlignment.TopLeft };
            pnlKart.Controls.Add(lblBaslik);

            Label lblDetay = new Label() { Text = detay, ForeColor = Color.FromArgb(160, 160, 170), BackColor = Color.FromArgb(28, 28, 34), Font = new Font("Segoe UI", 11, FontStyle.Regular), Location = new Point(20, 265), Size = new Size(320, 70), TextAlign = ContentAlignment.TopLeft };
            pnlKart.Controls.Add(lblDetay);

            Button btnIncele = new Button() { Size = new Size(320, 45), Location = new Point(20, 350), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Text = "Kampanyayı İncele", BackColor = Color.FromArgb(120, 40, 140), ForeColor = Color.White, Cursor = Cursors.Hand };
            btnIncele.FlatAppearance.BorderSize = 0;
            btnIncele.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnIncele.Width, btnIncele.Height, 8, 8));

            btnIncele.MouseEnter += (s, e) => btnIncele.BackColor = Color.FromArgb(145, 55, 165);
            btnIncele.MouseLeave += (s, e) => btnIncele.BackColor = Color.FromArgb(120, 40, 140);
            btnIncele.Click += (s, e) => {
                Form1 anaForm = (Form1)this.FindForm();
                if (anaForm != null) anaForm.SayfaYukle(new KampanyalarUC());
            };

            pnlKart.Controls.Add(btnIncele);

            SuruklemeOlaylariniEkle(pnlKart);
            SuruklemeOlaylariniEkle(pbVitrin);
            SuruklemeOlaylariniEkle(lblBaslik);
            SuruklemeOlaylariniEkle(lblDetay);

            return pnlKart;
        }
    }
}