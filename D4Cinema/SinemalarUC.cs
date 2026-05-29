using FontAwesome.Sharp;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Reflection;

namespace D4Cinema
{
    public partial class SinemalarUC : UserControl
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private Panel pnlViewport, pnlContent, pnlScrollTrack, pnlScrollThumb;
        private FlowLayoutPanel flpSinemalar;
        private int scrollY = 0;
        private bool isThumbDragging = false;
        private int thumbStartY = 0, mouseStartY = 0;

        
        private Color renkArkaPlan = Color.FromArgb(18, 18, 22);
        private Color renkKartArka = Color.FromArgb(28, 28, 34);
        private Color renkD4Moru = Color.FromArgb(120, 40, 140);
        private Color renkD4MoruHover = Color.FromArgb(145, 55, 165);
        private Color renkKapali = Color.Crimson;

        public SinemalarUC()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = renkArkaPlan;
            this.Padding = new Padding(0, 90, 0, 0); 

            DoubleBufferAktifEt(this);

            ArayuzuHazirla();
            SinemalariGetir();
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
                if (flpSinemalar != null) flpSinemalar.Width = pnlContent.Width - 80;
                YukseklikGuncelle();
            };

            IconPictureBox iconBaslik = new IconPictureBox()
            {
                IconChar = IconChar.MapMarkedAlt,
                IconColor = renkD4MoruHover,
                IconSize = 36,
                Location = new Point(40, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(iconBaslik);

            Label lblBaslik = new Label()
            {
                Text = "Sinema Salonlarımız",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                Location = new Point(85, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(lblBaslik);

            flpSinemalar = new FlowLayoutPanel()
            {
                Location = new Point(40, 90),
                Width = 1100, // Başlangıç genişliği
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };
            DoubleBufferAktifEt(flpSinemalar);
            pnlContent.Controls.Add(flpSinemalar);
        }

        private void SinemalariGetir()
        {
            flpSinemalar.Controls.Clear();

            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                using (SQLiteCommand komut = new SQLiteCommand("SELECT * FROM Sinemalar", baglan))
                {
                    using (SQLiteDataReader oku = komut.ExecuteReader())
                    {
                        while (oku.Read())
                        {
                            int id = Convert.ToInt32(oku["ID"]);
                            string ad = oku["SubeAdi"].ToString();
                            string durum = oku["Durum"].ToString();

                            
                            string sehir = "Bilinmiyor";
                            try { sehir = oku["Sehir"].ToString(); } catch { }
                            if (string.IsNullOrWhiteSpace(sehir)) sehir = ad.Contains("Kızılay") ? "Ankara" : (ad.Contains("Kordon") || ad.Contains("Pub") ? "Çanakkale" : "Türkiye");

                            flpSinemalar.Controls.Add(SinemaKartiOlustur(id, ad, sehir, durum));
                        }
                    }
                }
            }

            YukseklikGuncelle();
            HerYereScrollEkle(flpSinemalar);
        }

     
        private Panel SinemaKartiOlustur(int id, string ad, string sehir, string durum)
        {
           
            Panel pnlKart = new Panel()
            {
                Size = new Size(380, 160),
                Margin = new Padding(0, 0, 30, 30),
                BackColor = renkKartArka
            };
            pnlKart.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlKart.Width, pnlKart.Height, 15, 15));

            bool isAcik = (durum.ToLower() == "aktif" || durum.ToLower() == "açık");

            
            IconChar mekanIkoni = IconChar.MapMarkerAlt;
            if (ad.ToLower().Contains("valhalla") || ad.ToLower().Contains("pub"))
            {
                mekanIkoni = IconChar.BeerMugEmpty;
            }

            IconPictureBox pbIcon = new IconPictureBox()
            {
                IconChar = mekanIkoni,
                IconColor = Color.FromArgb(100, 100, 110),
                IconSize = 24,
                Location = new Point(20, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlKart.Controls.Add(pbIcon);

            
            Label lblAd = new Label()
            {
                Text = ad,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(50, 18),
                AutoSize = true,
                MaximumSize = new Size(310, 0),
                AutoEllipsis = true
            };
            pnlKart.Controls.Add(lblAd);

           
            Label lblSehir = new Label()
            {
                Text = sehir,
                ForeColor = Color.FromArgb(150, 150, 160),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(20, 65),
                AutoSize = true
            };
            pnlKart.Controls.Add(lblSehir);

           
            Label lblDurum = new Label()
            {
                Text = isAcik ? "● AÇIK" : "● KAPALI",
                ForeColor = isAcik ? Color.LimeGreen : renkKapali,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlKart.Controls.Add(lblDurum);
           
            lblDurum.Location = new Point(360 - lblDurum.PreferredWidth, 65);

           
            Button btnIslem = new Button()
            {
                Size = new Size(340, 45),
                Location = new Point(20, 95),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Text = isAcik ? "Seansları Gör" : "Tadilat / Yakında",
                BackColor = isAcik ? renkD4Moru : renkKapali,
                ForeColor = Color.White
            };
            btnIslem.FlatAppearance.BorderSize = 0;
            btnIslem.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnIslem.Width, btnIslem.Height, 8, 8));

            if (isAcik)
            {
                btnIslem.Cursor = Cursors.Hand;
                btnIslem.MouseEnter += (s, e) => btnIslem.BackColor = renkD4MoruHover;
                btnIslem.MouseLeave += (s, e) => btnIslem.BackColor = renkD4Moru;
                btnIslem.Click += (s, e) => {
                    Form1 anaForm = (Form1)this.FindForm();
                    if (anaForm != null) anaForm.SayfaYukle(new FilmlerUC()); 
                };
            }
            else
            {
                btnIslem.Cursor = Cursors.No; 
                btnIslem.FlatAppearance.MouseDownBackColor = renkKapali;
                btnIslem.FlatAppearance.MouseOverBackColor = renkKapali;
            }

            pnlKart.Controls.Add(btnIslem);

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
            pnlContent.Height = flpSinemalar.Bottom + 150;
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