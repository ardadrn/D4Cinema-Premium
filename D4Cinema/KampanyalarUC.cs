using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace D4Cinema
{
    public partial class KampanyalarUC : UserControl
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private Panel pnlViewport, pnlContent, pnlScrollTrack, pnlScrollThumb;
        private FlowLayoutPanel flpKampanyalar;
        private Panel pnlBanner;
        private int scrollY = 0;
        private bool isThumbDragging = false;
        private int thumbStartY = 0, mouseStartY = 0;

        private Color renkArkaPlan = Color.FromArgb(18, 18, 22);
        private Color renkKartArka = Color.FromArgb(28, 28, 34);
        private Color renkD4Moru = Color.FromArgb(120, 40, 140);
        private Color renkD4MoruHover = Color.FromArgb(145, 55, 165);

        public KampanyalarUC()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = renkArkaPlan;
            this.Padding = new Padding(0, 90, 0, 0);

            DoubleBufferAktifEt(this);

            ArayuzuHazirla();
            KampanyalariDoldur();
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

            pnlScrollThumb = new Panel() { Width = 8, Height = 100, Left = 2, Top = 2, BackColor = renkD4Moru, Cursor = Cursors.Hand };
            pnlScrollThumb.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 8, 100, 8, 8));
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

            pnlViewport = new Panel() { Dock = DockStyle.Fill, AutoScroll = false };
            this.Controls.Add(pnlViewport);

            pnlContent = new Panel() { Location = new Point(0, 0), Width = pnlViewport.Width, Height = 1500, BackColor = Color.Transparent };
            pnlViewport.Controls.Add(pnlContent);

            pnlViewport.MouseWheel += Icerik_Kaydir;

            this.Resize += (s, e) => {
                pnlContent.Width = pnlViewport.Width;
                YukseklikGuncelle();
            };

            IconPictureBox iconBaslik = new IconPictureBox()
            {
                IconChar = IconChar.Gift,
                IconColor = renkD4MoruHover,
                IconSize = 36,
                Location = new Point(40, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(iconBaslik);

            Label lblBaslik = new Label()
            {
                Text = "Güncel Kampanyalar",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                Location = new Point(85, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(lblBaslik);

            pnlBanner = new Panel()
            {
                Location = new Point(40, 80),
                Width = 1120,
                Height = 200,
                BackColor = renkKartArka
            };
            pnlBanner.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlBanner.Width, pnlBanner.Height, 20, 20));

            string bannerYolu = Path.Combine(Application.StartupPath, "Kampanyalar", "halk_gunu.png");

            if (File.Exists(bannerYolu))
            {
                pnlBanner.BackgroundImage = Image.FromFile(bannerYolu);
                pnlBanner.BackgroundImageLayout = ImageLayout.Stretch;

                pnlBanner.Paint += (s, e) => {
                    using (SolidBrush firca = new SolidBrush(Color.FromArgb(150, 18, 18, 22)))
                    {
                        e.Graphics.FillRectangle(firca, pnlBanner.ClientRectangle);
                    }
                };
            }
            else
            {
                pnlBanner.Paint += (s, e) => {
                    using (LinearGradientBrush firca = new LinearGradientBrush(pnlBanner.ClientRectangle, Color.FromArgb(90, 20, 100), Color.FromArgb(28, 28, 34), 0f))
                    {
                        e.Graphics.FillRectangle(firca, pnlBanner.ClientRectangle);
                    }
                };
            }

            IconPictureBox pbBannerIcon = new IconPictureBox()
            {
                IconChar = IconChar.Users,
                IconColor = Color.White,
                IconSize = 110,
                Size = new Size(110, 110),
                Location = new Point(40, 45),
                BackColor = Color.Transparent
            };
            pnlBanner.Controls.Add(pbBannerIcon);

            Label lblBannerBaslik = new Label()
            {
                Text = "HER ÇARŞAMBA HALK GÜNÜ!",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(170, 45),
                BackColor = Color.Transparent
            };
            pnlBanner.Controls.Add(lblBannerBaslik);

            Label lblBannerDetay = new Label()
            {
                Text = "Haftanın tam ortasında sinema keyfini ikiye katla! Çarşamba günleri tüm seanslarda, tüm filmlerde biletler anında %50 İNDİRİMLİ.",
                ForeColor = Color.FromArgb(220, 220, 230),
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(178, 110),
                MaximumSize = new Size(850, 0),
                BackColor = Color.Transparent
            };
            pnlBanner.Controls.Add(lblBannerDetay);
            pnlContent.Controls.Add(pnlBanner);

            flpKampanyalar = new FlowLayoutPanel()
            {
                Location = new Point(40, pnlBanner.Bottom + 40),
                Width = 1150,
                MaximumSize = new Size(1150, 0),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };
            DoubleBufferAktifEt(flpKampanyalar);
            pnlContent.Controls.Add(flpKampanyalar);
        }

        private void KampanyalariDoldur()
        {
            flpKampanyalar.Controls.Clear();

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

            YukseklikGuncelle();
            HerYereScrollEkle(flpKampanyalar);
            HerYereScrollEkle(pnlBanner);
        }

        private Panel KampanyaKartiOlustur(string baslik, string detay, IconChar ikon, string resimAdi)
        {
            Panel pnlKart = new Panel()
            {
                Size = new Size(360, 420),
                Margin = new Padding(0, 0, 20, 40),
                BackColor = renkKartArka
            };
            pnlKart.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlKart.Width, pnlKart.Height, 15, 15));

            PictureBox pbVitrin = new PictureBox()
            {
                Dock = DockStyle.Top,
                Height = 180,
                BackColor = Color.FromArgb(20, 20, 25),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

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

                IconPictureBox pbIcon = new IconPictureBox()
                {
                    IconChar = ikon,
                    IconColor = Color.FromArgb(180, 80, 200),
                    IconSize = 80,
                    Size = new Size(80, 80),
                    BackColor = Color.Transparent
                };
                pbIcon.Location = new Point((pbVitrin.Width - pbIcon.Width) / 2, (pbVitrin.Height - pbIcon.Height) / 2);
                pbVitrin.Controls.Add(pbIcon);
            }

            pnlKart.Controls.Add(pbVitrin);

            Label lblBaslik = new Label()
            {
                Text = baslik,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 200),
                Size = new Size(320, 60),
                AutoEllipsis = true,
                TextAlign = ContentAlignment.TopLeft
            };
            pnlKart.Controls.Add(lblBaslik);

            Label lblDetay = new Label()
            {
                Text = detay,
                ForeColor = Color.FromArgb(160, 160, 170),
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                Location = new Point(20, 265),
                Size = new Size(320, 70),
                TextAlign = ContentAlignment.TopLeft
            };
            pnlKart.Controls.Add(lblDetay);

            Button btnIncele = new Button()
            {
                Size = new Size(320, 45),
                Location = new Point(20, 350),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Text = "Kampanyayı İncele",
                BackColor = renkD4Moru,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnIncele.FlatAppearance.BorderSize = 0;
            btnIncele.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnIncele.Width, btnIncele.Height, 8, 8));

            btnIncele.MouseEnter += (s, e) => btnIncele.BackColor = renkD4MoruHover;
            btnIncele.MouseLeave += (s, e) => btnIncele.BackColor = renkD4Moru;
            btnIncele.Click += (s, e) => {
                Form1 anaForm = (Form1)this.FindForm();
                if (anaForm != null) anaForm.SayfaYukle(new KampanyalarUC());
            };

            pnlKart.Controls.Add(btnIncele);

            return pnlKart;
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
            pnlContent.Height = flpKampanyalar.Bottom + 150;
            ScrollGuncelle();
        }

        private void HerYereScrollEkle(Control nesne)
        {
            nesne.MouseWheel -= Icerik_Kaydir;
            nesne.MouseWheel += Icerik_Kaydir;
            foreach (Control cocuk in nesne.Controls) HerYereScrollEkle(cocuk);
        }
    }
}