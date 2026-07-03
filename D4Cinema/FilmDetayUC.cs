using FontAwesome.Sharp;
using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace D4Cinema
{
    public partial class FilmDetayUC : UserControl
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private int seciliFilmID;

        public FilmDetayUC(int filmID)
        {
            InitializeComponent();
            seciliFilmID = filmID;

            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(0, 90, 0, 0);

            DoubleBufferAktifEt(this);

            VerileriCekVeArayuzuOlustur();
        }

        public static void DoubleBufferAktifEt(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        private void VerileriCekVeArayuzuOlustur()
        {
            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                using (SQLiteCommand komut = new SQLiteCommand("SELECT * FROM Filmler WHERE ID = @id", baglan))
                {
                    komut.Parameters.AddWithValue("@id", seciliFilmID);
                    using (SQLiteDataReader oku = komut.ExecuteReader())
                    {
                        if (oku.Read())
                        {
                            string ad = oku["FilmAdi"].ToString();
                            string tur = oku["Tur"].ToString();
                            string sure = oku["Sure"].ToString();
                            string konu = oku["Konu"].ToString();
                            string yonetmen = oku["Yonetmen"].ToString();
                            string vizyon = oku["VizyonTarihi"].ToString();
                            string afisYolu = oku["AfisYolu"].ToString();
                            string durum = oku["Durum"].ToString();

                            ArayuzuCiz(ad, tur, sure, konu, yonetmen, vizyon, afisYolu, durum);
                        }
                    }
                }
            }
        }

        private void ArayuzuCiz(string ad, string tur, string sure, string konu, string yonetmen, string vizyon, string afisYolu, string durum)
        {
            string afisTamYolu = AppPaths.GetAfisPath(afisYolu);
            Image arkaPlanAfisi = AppPaths.LoadImageWithoutLock(afisTamYolu);

            if (arkaPlanAfisi != null)
            {
                using (arkaPlanAfisi)
                {
                    Bitmap bmp = new Bitmap(1300, 950);

                    using (Graphics g = Graphics.FromImage(bmp))
                    using (Image kucukAfis = new Bitmap(arkaPlanAfisi, new Size(50, 75)))
                    {
                        g.InterpolationMode =
                            System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                        g.DrawImage(
                            kucukAfis,
                            new Rectangle(0, 0, bmp.Width, bmp.Height));

                        using (SolidBrush firca =
                            new SolidBrush(Color.FromArgb(235, 18, 18, 22)))
                        {
                            g.FillRectangle(
                                firca,
                                new Rectangle(0, 0, bmp.Width, bmp.Height));
                        }
                    }

                    this.BackgroundImage = bmp;
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            else
            {
                this.BackColor = Color.FromArgb(18, 18, 22);
            }

            IconButton btnGeri = new IconButton()
            {
                Text = " Geri Dön",
                IconChar = IconChar.ArrowLeft,
                IconSize = 20,
                IconColor = Color.FromArgb(180, 180, 190),
                ForeColor = Color.FromArgb(180, 180, 190),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(50, 110),
                Size = new Size(150, 35),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                BackColor = Color.Transparent
            };
            btnGeri.FlatAppearance.BorderSize = 0;
            btnGeri.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnGeri.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnGeri.MouseEnter += (s, e) => { btnGeri.ForeColor = Color.White; btnGeri.IconColor = Color.White; };
            btnGeri.MouseLeave += (s, e) => { btnGeri.ForeColor = Color.FromArgb(180, 180, 190); btnGeri.IconColor = Color.FromArgb(180, 180, 190); };
            btnGeri.Click += (s, e) => { this.Dispose(); };
            this.Controls.Add(btnGeri);

            PictureBox pbAfis = new PictureBox()
            {
                Size = new Size(360, 520),
                Location = new Point(50, 160),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };
            pbAfis.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pbAfis.Width, pbAfis.Height, 20, 20));

            if (!string.IsNullOrEmpty(afisTamYolu) && File.Exists(afisTamYolu))
            {
                pbAfis.Image = AppPaths.LoadImageWithoutLock(afisTamYolu);
            }
            this.Controls.Add(pbAfis);

            FlowLayoutPanel flpBilgiler = new FlowLayoutPanel()
            {
                Location = new Point(50, 700),
                Size = new Size(360, 200),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            flpBilgiler.Controls.Add(BilgiSatiriOlustur(IconChar.CameraRetro, "Yönetmen", yonetmen));
            flpBilgiler.Controls.Add(BilgiSatiriOlustur(IconChar.Tags, "Tür", tur));

            string sureMetni = sure.Contains("dk") ? sure : sure + " dk";
            flpBilgiler.Controls.Add(BilgiSatiriOlustur(IconChar.Stopwatch, "Süre", sureMetni));

            flpBilgiler.Controls.Add(BilgiSatiriOlustur(IconChar.CalendarCheck, "Vizyon Tarihi", vizyon));

            this.Controls.Add(flpBilgiler);

            int sagX = 460;

            Label lblFilmAdi = new Label()
            {
                Text = ad,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                Location = new Point(sagX, 150),
                AutoSize = true,
                MaximumSize = new Size(750, 0)
            };
            this.Controls.Add(lblFilmAdi);

            IconButton btnBilet = new IconButton()
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(350, 55),
                Location = new Point(sagX, lblFilmAdi.Bottom + 30),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            btnBilet.FlatAppearance.BorderSize = 0;
            btnBilet.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnBilet.Width, btnBilet.Height, 20, 20));

            if (durum.Contains("Yakın") || durum.Contains("Yakinda"))
            {
                btnBilet.Text = " Biletler Satışa Çıkınca Hatırlat";
                btnBilet.IconChar = IconChar.Bell;
                btnBilet.BackColor = Color.FromArgb(40, 40, 45);
                btnBilet.ForeColor = Color.White;
                btnBilet.IconColor = Color.White;

                btnBilet.MouseEnter += (s, e) => btnBilet.BackColor = Color.FromArgb(60, 60, 65);
                btnBilet.MouseLeave += (s, e) => btnBilet.BackColor = Color.FromArgb(40, 40, 45);

                btnBilet.Click += (s, e) => {
                    MessageBox.Show("Bu film için hatırlatıcı başarıyla kuruldu! Vizyona girdiği an sana bildirim göndereceğiz.", "Hatırlatıcı Kuruldu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
            }
            else
            {
                btnBilet.Text = " SEANS SEÇ VE BİLET AL";
                btnBilet.IconChar = IconChar.TicketAlt;
                btnBilet.BackColor = Color.FromArgb(120, 40, 140);
                btnBilet.ForeColor = Color.White;
                btnBilet.IconColor = Color.White;

                btnBilet.MouseEnter += (s, e) => btnBilet.BackColor = Color.FromArgb(145, 55, 165);
                btnBilet.MouseLeave += (s, e) => btnBilet.BackColor = Color.FromArgb(120, 40, 140);

                btnBilet.Click += (s, e) => {
                    if (Oturum.GirisYapildiMi)
                    {
                        BiletAlUC biletSayfasi = new BiletAlUC(seciliFilmID);
                        this.Parent.Controls.Add(biletSayfasi);
                        biletSayfasi.BringToFront();
                    }
                    else
                    {
                        MessageBox.Show("Bilet almak için lütfen önce giriş yapın veya kayıt olun.", "Giriş Gerekli", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Oturum.BekleyenSayfa = new BiletAlUC(seciliFilmID);

                        Form1 anaForm = (Form1)this.FindForm();
                        anaForm.SayfaYukle(new HesapUC(true));
                    }
                };
            }
            this.Controls.Add(btnBilet);

            Label lblKonuBaslik = new Label()
            {
                Text = "Filmin Konusu",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(sagX, btnBilet.Bottom + 50),
                AutoSize = true
            };
            this.Controls.Add(lblKonuBaslik);

            Label lblKonuMetni = new Label()
            {
                Text = string.IsNullOrWhiteSpace(konu) ? "Bu film için henüz bir açıklama girilmemiş." : konu,
                ForeColor = Color.FromArgb(220, 220, 220),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 13, FontStyle.Regular),
                Location = new Point(sagX, lblKonuBaslik.Bottom + 15),
                AutoSize = true,
                MaximumSize = new Size(780, 0)
            };
            this.Controls.Add(lblKonuMetni);
        }

        private Panel BilgiSatiriOlustur(IconChar ikon, string baslik, string deger)
        {
            Panel pnl = new Panel() { Width = 360, Height = 35, Margin = new Padding(0, 0, 0, 10), BackColor = Color.Transparent };

            IconPictureBox pbIcon = new IconPictureBox()
            {
                IconChar = ikon,
                IconColor = Color.FromArgb(165, 75, 185),
                IconSize = 22,
                Size = new Size(22, 22),
                Location = new Point(0, 5),
                BackColor = Color.Transparent
            };
            pnl.Controls.Add(pbIcon);

            Label lblBaslik = new Label()
            {
                Text = baslik + ":",
                ForeColor = Color.FromArgb(170, 170, 180),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(30, 5),
                AutoSize = true
            };
            pnl.Controls.Add(lblBaslik);

            Label lblDeger = new Label()
            {
                Text = string.IsNullOrWhiteSpace(deger) ? "Belirtilmemiş" : deger,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                Location = new Point(lblBaslik.Right + 5, 5),
                AutoSize = true
            };
            pnl.Controls.Add(lblDeger);

            return pnl;
        }
    }
}