using FontAwesome.Sharp;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data.SQLite;

namespace D4Cinema
{
    public partial class ProfilUC : UserControl
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private Panel pnlViewport, pnlContent, pnlScrollTrack, pnlScrollThumb;
        private int scrollY = 0;
        private bool isThumbDragging = false;
        private int thumbStartY = 0, mouseStartY = 0;
        private FlowLayoutPanel pnlBiletler;

        public ProfilUC()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(18, 18, 22);
            this.Padding = new Padding(0, 90, 0, 0);

            DoubleBufferAktifEt(this);

            ArayuzuHazirla();
            BiletleriGetir();
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

           
            IconPictureBox iconBiletlerim = new IconPictureBox()
            {
                IconChar = IconChar.TicketAlt,
                IconColor = Color.FromArgb(145, 55, 165),
                IconSize = 36,
                Location = new Point(40, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(iconBiletlerim);

            Label lblBaslik = new Label()
            {
                Text = "Biletlerim",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                Location = new Point(85, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(lblBaslik);

            
            pnlBiletler = new FlowLayoutPanel()
            {
                Location = new Point(40, 90),
                Width = 1000,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            DoubleBufferAktifEt(pnlBiletler);
            pnlContent.Controls.Add(pnlBiletler);
        }

        private void BiletleriGetir()
        {
            pnlBiletler.Controls.Clear();
            bool biletVarMi = false;

            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                string sorgu = @"
                    SELECT 
                        MIN(b.ID) AS BiletNo, 
                        f.FilmAdi, 
                        s.SubeAdi, 
                        GROUP_CONCAT(b.KoltukNo, ', ') AS Koltuklar, 
                        b.Tarih 
                    FROM Biletler b
                    INNER JOIN Filmler f ON b.FilmID = f.ID
                    INNER JOIN Sinemalar s ON b.SinemaID = s.ID
                    WHERE b.KullaniciID = @kullaniciID
                    GROUP BY f.FilmAdi, s.SubeAdi, b.Tarih
                    ORDER BY MIN(b.ID) DESC";

                using (SQLiteCommand komut = new SQLiteCommand(sorgu, baglan))
                {
                    komut.Parameters.AddWithValue("@kullaniciID", Oturum.ID);
                    using (SQLiteDataReader oku = komut.ExecuteReader())
                    {
                        while (oku.Read())
                        {
                            biletVarMi = true;
                            string biletNo = oku["BiletNo"].ToString();
                            string filmAdi = oku["FilmAdi"].ToString();
                            string sinema = oku["SubeAdi"].ToString();
                            string koltuklar = oku["Koltuklar"].ToString();
                            string tarih = oku["Tarih"].ToString();

                            pnlBiletler.Controls.Add(BiletKartiOlustur(biletNo, filmAdi, sinema, koltuklar, tarih));
                        }
                    }
                }
            }

            if (!biletVarMi)
            {
                Label lblBos = new Label()
                {
                    Text = "Henüz alınmış bir biletiniz bulunmuyor.\nHadi vizyondaki harika filmlere bir göz at!",
                    ForeColor = Color.FromArgb(150, 150, 160),
                    Font = new Font("Segoe UI", 14, FontStyle.Italic),
                    AutoSize = true,
                    Margin = new Padding(10, 40, 0, 0)
                };
                pnlBiletler.Controls.Add(lblBos);
            }

            YukseklikGuncelle();
            HerYereScrollEkle(pnlBiletler);
        }

       
        private Panel BiletKartiOlustur(string biletNo, string filmAdi, string sinema, string koltuklar, string tarih)
        {
            string[] koltukListesi = koltuklar.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            int koltukSayisi = koltukListesi.Length;

            
            Panel pnlAna = new Panel()
            {
                Width = 850,
                Height = 130, 
                Margin = new Padding(0, 0, 0, 25),
                BackColor = Color.FromArgb(120, 40, 140)
            };
            pnlAna.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlAna.Width, pnlAna.Height, 20, 20));

            
            Panel pnlUst = new Panel() { Dock = DockStyle.Top, Height = 130, BackColor = Color.Transparent };
            pnlAna.Controls.Add(pnlUst);

            Label lblFilm = new Label()
            {
                Text = filmAdi,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(25, 25),
                AutoSize = true,
                MaximumSize = new Size(550, 0),
                AutoEllipsis = true
            };
            pnlUst.Controls.Add(lblFilm);

            Label lblSinema = new Label()
            {
                Text = "📍 " + sinema,
                ForeColor = Color.FromArgb(230, 230, 240),
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                Location = new Point(30, 75),
                AutoSize = true
            };
            pnlUst.Controls.Add(lblSinema);

            Label lblCizgi = new Label()
            {
                Text = "|\n|\n|\n|\n|\n|\n|\n|\n|",
                ForeColor = Color.FromArgb(180, 255, 255, 255),
                Font = new Font("Consolas", 10, FontStyle.Bold),
                Location = new Point(590, 10),
                AutoSize = true
            };
            pnlUst.Controls.Add(lblCizgi);

            Label lblTarih = new Label()
            {
                Text = tarih,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(620, 25),
                AutoSize = true
            };
            pnlUst.Controls.Add(lblTarih);

            Label lblBiletNo = new Label()
            {
                Text = "Bilet No: #" + biletNo.PadLeft(5, '0'),
                ForeColor = Color.FromArgb(220, 220, 230),
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                Location = new Point(620, 50),
                AutoSize = true
            };
            pnlUst.Controls.Add(lblBiletNo);

           
            IconButton btnKoltukDetay = new IconButton()
            {
                Text = $" {koltukSayisi} Koltuk Görüntüle",
                IconChar = IconChar.ChevronDown,
                IconSize = 20,
                IconColor = Color.White,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(620, 75),
                Size = new Size(200, 35),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(145, 55, 165), 
                TextImageRelation = TextImageRelation.TextBeforeImage,
                ImageAlign = ContentAlignment.MiddleRight,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnKoltukDetay.FlatAppearance.BorderSize = 0;
            btnKoltukDetay.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnKoltukDetay.Width, btnKoltukDetay.Height, 8, 8));
            pnlUst.Controls.Add(btnKoltukDetay);

            
            FlowLayoutPanel pnlAltKoltuklar = new FlowLayoutPanel()
            {
                Location = new Point(0, 130),
                Width = 850,
                AutoSize = true,
                Padding = new Padding(25, 20, 25, 20),
                BackColor = Color.FromArgb(100, 30, 120), 
                Visible = false
            };
            pnlAna.Controls.Add(pnlAltKoltuklar);

            
            foreach (string k in koltukListesi)
            {
                Label lblKapsul = new Label()
                {
                    Text = k,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(145, 55, 165),
                    AutoSize = false,
                    Size = new Size(65, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(0, 0, 10, 10)
                };
                lblKapsul.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, lblKapsul.Width, lblKapsul.Height, 8, 8));
                pnlAltKoltuklar.Controls.Add(lblKapsul);
            }

           
            btnKoltukDetay.Click += (s, e) => {
                if (pnlAltKoltuklar.Visible)
                {
                    
                    pnlAltKoltuklar.Visible = false;
                    pnlAna.Height = 130;
                    btnKoltukDetay.IconChar = IconChar.ChevronDown;
                }
                else
                {
                    
                    pnlAltKoltuklar.Visible = true;
                    pnlAna.Height = 130 + pnlAltKoltuklar.Height;
                    btnKoltukDetay.IconChar = IconChar.ChevronUp;
                }

                
                pnlAna.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlAna.Width, pnlAna.Height, 20, 20));

               
                YukseklikGuncelle();
            };

            return pnlAna;
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
            pnlContent.Height = pnlBiletler.Bottom + 150;
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