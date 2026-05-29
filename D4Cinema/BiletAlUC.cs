using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data.SQLite;

namespace D4Cinema
{
    public partial class BiletAlUC : UserControl
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private Panel pnlFilmlerOuter, pnlSinemalarOuter, pnlSeanslarOuter, pnlBiletlerOuter;
        private FlowLayoutPanel flpFilmler, flpSinemalar, flpSeanslar, flpBiletler;

        private Panel pnlKoltuklarSahnesi;
        private TableLayoutPanel pnlKoltukIzgarasi;
        private Button btnSatinAl;
        private Label lblSecilenKoltukOzet;

        
        private int tamBiletSayisi = 0;
        private int ogrBiletSayisi = 0;
        private Label lblTamSayi;
        private Label lblOgrSayi;

        private int secilenFilmID = 0;
        private int secilenSinemaID = 0;
        private string secilenTarih = "";
        private string secilenSaat = "";
        private int toplamBilet = 0;
        private List<string> secilenKoltuklar = new List<string>();

        private int tamBiletFiyat = 150;
        private int ogrenciBiletFiyat = 100;

        
        private Color renkAktifArka = Color.FromArgb(28, 28, 34);
        private Color renkPasifArka = Color.FromArgb(18, 18, 22);
        private Color renkD4Moru = Color.FromArgb(120, 40, 140);

        
        private Color renkKoltukKaranlik = Color.FromArgb(40, 40, 45);
        private Color renkKoltukBos = Color.FromArgb(120, 40, 140);
        private Color renkKoltukSecili = Color.LimeGreen;
        private Color renkKoltukDolu = Color.Crimson;

        public BiletAlUC(int gelenFilmID)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = renkPasifArka;
            this.Padding = new Padding(0, 90, 0, 0);
            this.secilenFilmID = gelenFilmID;

            ArayuzIskeletiniKur();
            AdimlariOlustur();

            Adim1_FilmleriYukle();
        }

        private void ArayuzIskeletiniKur()
        {
            
            IconButton btnGeri = new IconButton()
            {
                Text = " Geri Dön",
                IconChar = IconChar.ArrowLeft,
                IconSize = 20,
                ForeColor = Color.FromArgb(180, 180, 190),
                IconColor = Color.FromArgb(180, 180, 190),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(40, 15),
                Size = new Size(150, 35),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextImageRelation = TextImageRelation.ImageBeforeText
            };
            btnGeri.FlatAppearance.BorderSize = 0;
            btnGeri.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnGeri.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnGeri.MouseEnter += (s, e) => { btnGeri.ForeColor = Color.White; btnGeri.IconColor = Color.White; };
            btnGeri.MouseLeave += (s, e) => { btnGeri.ForeColor = Color.FromArgb(180, 180, 190); btnGeri.IconColor = Color.FromArgb(180, 180, 190); };
            btnGeri.Click += (s, e) => { this.Dispose(); };
            this.Controls.Add(btnGeri);

            
            TableLayoutPanel anaTlp = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(40, 60, 40, 20)
            };

            anaTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            anaTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            anaTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            anaTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            anaTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            this.Controls.Add(anaTlp);
            anaTlp.SendToBack();

            flpFilmler = PanelOlustur(IconChar.Clapperboard, "1. Film Seçimi", out pnlFilmlerOuter);
            anaTlp.Controls.Add(pnlFilmlerOuter, 0, 0);

            flpSeanslar = PanelOlustur(IconChar.Clock, "3. Tarih ve Seans", out pnlSeanslarOuter);
            anaTlp.Controls.Add(pnlSeanslarOuter, 0, 1);

            flpSinemalar = PanelOlustur(IconChar.MapMarkerAlt, "2. Sinema Salonu", out pnlSinemalarOuter);
            anaTlp.Controls.Add(pnlSinemalarOuter, 2, 0);

            flpBiletler = PanelOlustur(IconChar.Tags, "4. Bilet Türü", out pnlBiletlerOuter);
            anaTlp.Controls.Add(pnlBiletlerOuter, 2, 1);

            pnlKoltuklarSahnesi = new Panel() { Dock = DockStyle.Fill, BackColor = renkAktifArka, Margin = new Padding(10, 0, 10, 15) };

            pnlKoltuklarSahnesi.Resize += (s, e) => {
                if (pnlKoltuklarSahnesi.Width > 0 && pnlKoltuklarSahnesi.Height > 0)
                {
                    pnlKoltuklarSahnesi.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlKoltuklarSahnesi.Width, pnlKoltuklarSahnesi.Height, 20, 20));
                    if (pnlKoltukIzgarasi != null)
                    {
                        pnlKoltukIzgarasi.Location = new Point((pnlKoltuklarSahnesi.Width - pnlKoltukIzgarasi.Width) / 2, 90);
                    }
                }
            };

            anaTlp.Controls.Add(pnlKoltuklarSahnesi, 1, 0);
            anaTlp.SetRowSpan(pnlKoltuklarSahnesi, 2);

            Panel pnlPerde = new Panel() { Dock = DockStyle.Top, Height = 70, Margin = new Padding(0) };
            pnlKoltuklarSahnesi.Controls.Add(pnlPerde);

            pnlPerde.Paint += (s, e) => {
                GraphicsPath path = new GraphicsPath();
                path.AddArc(20, -30, pnlPerde.Width - 40, 80, 0, 180);
                using (LinearGradientBrush firca = new LinearGradientBrush(pnlPerde.ClientRectangle, Color.FromArgb(60, 60, 70), Color.Transparent, 90f))
                {
                    e.Graphics.FillPath(firca, path);
                }
            };

            Label lblPerde = new Label()
            {
                Text = "S İ N E M A   P E R D E S İ",
                ForeColor = Color.FromArgb(140, 140, 150),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            pnlPerde.Controls.Add(lblPerde);

            pnlKoltukIzgarasi = new TableLayoutPanel()
            {
                AutoSize = true,
                MaximumSize = new Size(0, 0),
                BackColor = Color.Transparent
            };
            pnlKoltuklarSahnesi.Controls.Add(pnlKoltukIzgarasi);

            btnSatinAl = new Button()
            {
                Text = "KOLTUKLARI SEÇ",
                BackColor = Color.FromArgb(20, 20, 25),
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Bottom,
                Height = 75,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnSatinAl.FlatAppearance.BorderSize = 0;
            btnSatinAl.Click += BtnSatinAl_Click;
            pnlKoltuklarSahnesi.Controls.Add(btnSatinAl);

            lblSecilenKoltukOzet = new Label()
            {
                Text = "",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomRight,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                AutoSize = true
            };
            pnlKoltuklarSahnesi.Controls.Add(lblSecilenKoltukOzet);
            lblSecilenKoltukOzet.BringToFront();

            pnlKoltuklarSahnesi.Resize += (s, e) => {
                lblSecilenKoltukOzet.Location = new Point(pnlKoltuklarSahnesi.Width - lblSecilenKoltukOzet.Width - 20, btnSatinAl.Top - lblSecilenKoltukOzet.Height - 10);
            };
        }

        private FlowLayoutPanel PanelOlustur(IconChar ikon, string baslik, out Panel disPanel)
        {
            Panel gercekOuter = new Panel() { Dock = DockStyle.Fill, BackColor = renkAktifArka, Margin = new Padding(10, 0, 10, 15), Enabled = false };

            gercekOuter.Resize += (s, e) => {
                if (gercekOuter.Width > 0 && gercekOuter.Height > 0)
                {
                    gercekOuter.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, gercekOuter.Width, gercekOuter.Height, 15, 15));
                }
            };

            Panel pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 45, BackColor = Color.Transparent };
            IconPictureBox pbIcon = new IconPictureBox() { IconChar = ikon, IconColor = Color.FromArgb(145, 55, 165), IconSize = 22, Location = new Point(15, 12), AutoSize = true };
            Label lbl = new Label() { Text = baslik, ForeColor = Color.FromArgb(170, 170, 180), Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(45, 10), AutoSize = true };

            pnlHeader.Controls.Add(pbIcon);
            pnlHeader.Controls.Add(lbl);
            gercekOuter.Controls.Add(pnlHeader);

            Panel pnlScrollContainer = new Panel() { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            gercekOuter.Controls.Add(pnlScrollContainer);
            pnlScrollContainer.BringToFront();

            FlowLayoutPanel flpContent = new FlowLayoutPanel()
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(15, 5, 15, 15),
                BackColor = Color.Transparent
            };
            pnlScrollContainer.Controls.Add(flpContent);

            void SmoothScroll(object sender, MouseEventArgs e)
            {
                int newTop = flpContent.Top + (e.Delta > 0 ? 45 : -45);
                int minTop = pnlScrollContainer.Height - flpContent.Height - 15;
                if (minTop > 0) minTop = 0;
                if (newTop > 0) newTop = 0;
                if (newTop < minTop) newTop = minTop;
                flpContent.Top = newTop;
            }

            pnlScrollContainer.MouseWheel += SmoothScroll;
            flpContent.MouseWheel += SmoothScroll;
            flpContent.Tag = new Action<object, MouseEventArgs>(SmoothScroll);

            flpContent.ControlAdded += (s, e) => {
                
                if (e.Control is Button) e.Control.Width = 260;
            };

            disPanel = gercekOuter;
            return flpContent;
        }

       
        private Panel BiletSayaciOlustur(Action<int> islem, out Label lblSayiGosterge)
        {
            Panel pnl = new Panel() { Width = 260, Height = 40, BackColor = Color.FromArgb(40, 40, 45), Margin = new Padding(0, 0, 0, 15) };
            pnl.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnl.Width, pnl.Height, 8, 8));

            IconButton btnEksi = new IconButton() { IconChar = IconChar.Minus, IconSize = 20, IconColor = Color.White, BackColor = renkD4Moru, FlatStyle = FlatStyle.Flat, Width = 45, Dock = DockStyle.Left, Cursor = Cursors.Hand };
            btnEksi.FlatAppearance.BorderSize = 0;
            btnEksi.MouseEnter += (s, e) => btnEksi.BackColor = Color.FromArgb(145, 55, 165);
            btnEksi.MouseLeave += (s, e) => btnEksi.BackColor = renkD4Moru;
            btnEksi.Click += (s, e) => islem(-1);

            IconButton btnArti = new IconButton() { IconChar = IconChar.Plus, IconSize = 20, IconColor = Color.White, BackColor = renkD4Moru, FlatStyle = FlatStyle.Flat, Width = 45, Dock = DockStyle.Right, Cursor = Cursors.Hand };
            btnArti.FlatAppearance.BorderSize = 0;
            btnArti.MouseEnter += (s, e) => btnArti.BackColor = Color.FromArgb(145, 55, 165);
            btnArti.MouseLeave += (s, e) => btnArti.BackColor = renkD4Moru;
            btnArti.Click += (s, e) => islem(1);

            lblSayiGosterge = new Label() { Text = "0", ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, BackColor = Color.Transparent };

            pnl.Controls.Add(btnEksi);
            pnl.Controls.Add(btnArti);
            pnl.Controls.Add(lblSayiGosterge);
            lblSayiGosterge.BringToFront();

            return pnl;
        }

        private void BileseneKaydirmaEkle(Control c, FlowLayoutPanel flp)
        {
            if (flp.Tag is Action<object, MouseEventArgs> scrollFunc)
            {
                c.MouseWheel += new MouseEventHandler(scrollFunc);
            }
            foreach (Control child in c.Controls) BileseneKaydirmaEkle(child, flp);
        }

        private void AdimlariOlustur()
        {
           
            Label lblTam = new Label() { Text = $"Tam Bilet ({tamBiletFiyat} TL)", ForeColor = Color.White, AutoSize = true, Font = new Font("Segoe UI", 10), Margin = new Padding(0, 0, 0, 5) };
            Panel pnlTam = BiletSayaciOlustur((yon) => {
                if (yon > 0 && tamBiletSayisi < 10) tamBiletSayisi++;
                else if (yon < 0 && tamBiletSayisi > 0) tamBiletSayisi--;
                lblTamSayi.Text = tamBiletSayisi.ToString();
            }, out lblTamSayi);

            Label lblOgr = new Label() { Text = $"Öğrenci Bilet ({ogrenciBiletFiyat} TL)", ForeColor = Color.White, AutoSize = true, Margin = new Padding(0, 5, 0, 5), Font = new Font("Segoe UI", 10) };
            Panel pnlOgr = BiletSayaciOlustur((yon) => {
                if (yon > 0 && ogrBiletSayisi < 10) ogrBiletSayisi++;
                else if (yon < 0 && ogrBiletSayisi > 0) ogrBiletSayisi--;
                lblOgrSayi.Text = ogrBiletSayisi.ToString();
            }, out lblOgrSayi);

            flpBiletler.Controls.Add(lblTam); flpBiletler.Controls.Add(pnlTam);
            flpBiletler.Controls.Add(lblOgr); flpBiletler.Controls.Add(pnlOgr);

            Button btnBiletOnay = new Button()
            {
                Text = "Koltuk Seçimine Geç",
                BackColor = renkD4Moru,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Width = 260,
                Height = 45,
                Margin = new Padding(0, 10, 0, 0),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBiletOnay.FlatAppearance.BorderSize = 0;
            btnBiletOnay.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnBiletOnay.Width, btnBiletOnay.Height, 8, 8));

            btnBiletOnay.Click += (s, e) =>
            {
                toplamBilet = tamBiletSayisi + ogrBiletSayisi;
                if (toplamBilet > 0) Adim5_KoltuklariAydinlat();
                else MessageBox.Show("Lütfen en az 1 bilet seçiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };
            flpBiletler.Controls.Add(btnBiletOnay);

           
            BileseneKaydirmaEkle(lblTam, flpBiletler); BileseneKaydirmaEkle(pnlTam, flpBiletler);
            BileseneKaydirmaEkle(lblOgr, flpBiletler); BileseneKaydirmaEkle(pnlOgr, flpBiletler);
            BileseneKaydirmaEkle(btnBiletOnay, flpBiletler);

           
            string[] harfler = { "A", "B", "C", "D", "E", "F", "G", "H", "I" };
            int sutunSayisi = 10;

            pnlKoltukIzgarasi.ColumnCount = sutunSayisi + 1;
            pnlKoltukIzgarasi.RowCount = harfler.Length + 1;

            pnlKoltukIzgarasi.ColumnStyles.Clear();
            pnlKoltukIzgarasi.RowStyles.Clear();

            pnlKoltukIzgarasi.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35));
            for (int i = 0; i < sutunSayisi; i++) pnlKoltukIzgarasi.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));

            for (int i = 0; i < harfler.Length; i++) pnlKoltukIzgarasi.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            pnlKoltukIzgarasi.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            for (int i = 0; i < harfler.Length; i++)
            {
                Label lblHarf = new Label()
                {
                    Text = harfler[i],
                    ForeColor = Color.FromArgb(100, 100, 110),
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0)
                };
                pnlKoltukIzgarasi.Controls.Add(lblHarf, 0, i);

                for (int j = 1; j <= sutunSayisi; j++)
                {
                    string koltukNo = harfler[i] + j.ToString();

                    IconButton btnKoltuk = new IconButton()
                    {
                        Name = koltukNo,
                        Width = 46,
                        Height = 46,
                        Margin = new Padding(1),
                        BackColor = Color.Transparent,
                        IconChar = IconChar.Couch,
                        IconColor = renkKoltukKaranlik,
                        IconSize = 38,
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Default
                    };
                    btnKoltuk.FlatAppearance.BorderSize = 0;
                    btnKoltuk.FlatAppearance.MouseDownBackColor = Color.Transparent;
                    btnKoltuk.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    btnKoltuk.Click += BtnKoltuk_Click;

                    ToolTip tt = new ToolTip();
                    tt.SetToolTip(btnKoltuk, "Koltuk: " + koltukNo);

                    pnlKoltukIzgarasi.Controls.Add(btnKoltuk, j, i);
                }
            }

            for (int j = 1; j <= sutunSayisi; j++)
            {
                Label lblSayi = new Label()
                {
                    Text = j.ToString(),
                    ForeColor = Color.FromArgb(100, 100, 110),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0)
                };
                pnlKoltukIzgarasi.Controls.Add(lblSayi, j, harfler.Length);
            }
        }

        private void PaneliAydinlat(Panel pnlOuter)
        {
            pnlOuter.Enabled = true;
            foreach (Control c in pnlOuter.Controls)
                if (c is Panel headerPnl && headerPnl.Height == 45)
                    foreach (Control hc in headerPnl.Controls)
                        if (hc is Label lbl) lbl.ForeColor = Color.White;
        }

        private void PaneliKarart(Panel pnlOuter)
        {
            pnlOuter.Enabled = false;
            foreach (Control c in pnlOuter.Controls)
                if (c is Panel headerPnl && headerPnl.Height == 45)
                    foreach (Control hc in headerPnl.Controls)
                        if (hc is Label lbl) lbl.ForeColor = Color.FromArgb(100, 100, 110);
        }

        private void Adim1_FilmleriYukle()
        {
            PaneliAydinlat(pnlFilmlerOuter);
            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                using (SQLiteCommand komut = new SQLiteCommand("SELECT ID, FilmAdi FROM Filmler WHERE Durum='Vizyonda'", baglan))
                {
                    using (SQLiteDataReader oku = komut.ExecuteReader())
                    {
                        while (oku.Read())
                        {
                            int id = Convert.ToInt32(oku["ID"]);
                            string ad = oku["FilmAdi"].ToString();

                            Button btn = new Button() { Tag = id, Text = ad, Width = 260, Height = 45, Margin = new Padding(0, 0, 0, 10), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0), Cursor = Cursors.Hand };
                            btn.FlatAppearance.BorderSize = 0;
                            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 8, 8));

                            if (id == secilenFilmID)
                            {
                                btn.BackColor = renkD4Moru; btn.ForeColor = Color.White;
                                Adim2_SinemalariYukle();
                            }
                            else
                            {
                                btn.BackColor = Color.FromArgb(40, 40, 45); btn.ForeColor = Color.White;
                            }

                            btn.Click += (s, e) =>
                            {
                                foreach (Control c in flpFilmler.Controls) if (c is Button b) { b.BackColor = Color.FromArgb(40, 40, 45); b.ForeColor = Color.White; }
                                btn.BackColor = renkD4Moru;
                                secilenFilmID = (int)btn.Tag;

                                PaneliKarart(pnlSeanslarOuter); PaneliKarart(pnlBiletlerOuter);

                                pnlKoltukIzgarasi.Enabled = false; btnSatinAl.Enabled = false; btnSatinAl.BackColor = Color.FromArgb(20, 20, 25); btnSatinAl.ForeColor = Color.DimGray; btnSatinAl.Text = "KOLTUKLARI SEÇ";
                                secilenKoltuklar.Clear();
                                lblSecilenKoltukOzet.Text = "";
                                foreach (Control c in pnlKoltukIzgarasi.Controls) if (c is IconButton ib)
                                    {
                                        ib.IconColor = renkKoltukKaranlik;
                                        ib.Cursor = Cursors.Default;
                                    }

                                Adim2_SinemalariYukle();
                            };
                            BileseneKaydirmaEkle(btn, flpFilmler);
                            flpFilmler.Controls.Add(btn);
                        }
                    }
                }
            }
        }

        private void Adim2_SinemalariYukle()
        {
            flpSinemalar.Controls.Clear();
            PaneliAydinlat(pnlSinemalarOuter);
            secilenSinemaID = 0;

            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                using (SQLiteCommand komut = new SQLiteCommand("SELECT ID, SubeAdi FROM Sinemalar WHERE Durum='Aktif'", baglan))
                {
                    using (SQLiteDataReader oku = komut.ExecuteReader())
                    {
                        while (oku.Read())
                        {
                            int id = Convert.ToInt32(oku["ID"]);
                            string ad = oku["SubeAdi"].ToString();

                            Button btn = new Button() { Tag = id, Text = ad, Width = 260, Height = 45, Margin = new Padding(0, 0, 0, 10), BackColor = Color.FromArgb(40, 40, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0), Cursor = Cursors.Hand };
                            btn.FlatAppearance.BorderSize = 0;
                            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 8, 8));

                            btn.Click += (s, e) =>
                            {
                                foreach (Control c in flpSinemalar.Controls) if (c is Button b) { b.BackColor = Color.FromArgb(40, 40, 45); b.ForeColor = Color.White; }
                                btn.BackColor = renkD4Moru;
                                secilenSinemaID = (int)btn.Tag;
                                Adim3_SeanslariGoster();
                            };
                            BileseneKaydirmaEkle(btn, flpSinemalar);
                            flpSinemalar.Controls.Add(btn);
                        }
                    }
                }
            }
        }

        private void Adim3_SeanslariGoster()
        {
            flpSeanslar.Controls.Clear();
            PaneliAydinlat(pnlSeanslarOuter);

            string[] tarihler = { DateTime.Now.ToShortDateString(), DateTime.Now.AddDays(1).ToShortDateString() };
            string[] saatler = { "11:00", "14:30", "18:00", "21:15" };

            foreach (string tarih in tarihler)
            {
                Label lbl = new Label() { Text = tarih, ForeColor = Color.FromArgb(200, 200, 210), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 10, 0, 5) };
                BileseneKaydirmaEkle(lbl, flpSeanslar);
                flpSeanslar.Controls.Add(lbl);

               
                Panel pnlSaatWrapper = new Panel() { Width = 260, Height = 55, Margin = new Padding(0, 0, 0, 15), BackColor = Color.Transparent };

                Panel pnlSaatContent = new Panel() { Width = 260, Height = 35, BackColor = Color.Transparent, AutoScroll = false };
                FlowLayoutPanel flpSaatler = new FlowLayoutPanel()
                {
                    WrapContents = false,
                    AutoSize = true,
                    Height = 35,
                    BackColor = Color.Transparent
                };

                pnlSaatContent.Controls.Add(flpSaatler);
                pnlSaatWrapper.Controls.Add(pnlSaatContent);

                
                Panel pnlScrollTrackH = new Panel() { Width = 260, Height = 6, Top = 45, BackColor = Color.FromArgb(40, 40, 45) };
                pnlScrollTrackH.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 260, 6, 6, 6));

               
                Panel pnlScrollThumbH = new Panel() { Width = 80, Height = 6, Left = 0, BackColor = renkD4Moru, Cursor = Cursors.Hand };
                pnlScrollThumbH.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 80, 6, 6, 6));

                pnlScrollTrackH.Controls.Add(pnlScrollThumbH);
                pnlSaatWrapper.Controls.Add(pnlScrollTrackH);

                foreach (string saat in saatler)
                {
                    Button btn = new Button() { Text = saat, Width = 65, Height = 35, Margin = new Padding(0, 0, 10, 0), BackColor = Color.FromArgb(40, 40, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 6, 6));

                    btn.Click += (s, e) =>
                    {
                        foreach (Control c in flpSaatler.Controls) if (c is Button b) b.BackColor = Color.FromArgb(40, 40, 45);
                        btn.BackColor = renkD4Moru;
                        secilenTarih = tarih; secilenSaat = saat;
                        PaneliAydinlat(pnlBiletlerOuter);
                    };

                    BileseneKaydirmaEkle(btn, flpSeanslar);
                    flpSaatler.Controls.Add(btn);
                }

                
                bool isHDragging = false;
                int hStartX = 0;
                int hMouseX = 0;

                pnlScrollThumbH.MouseDown += (s, e) => { isHDragging = true; hStartX = pnlScrollThumbH.Left; hMouseX = Cursor.Position.X; };
                pnlScrollThumbH.MouseMove += (s, e) => {
                    if (isHDragging)
                    {
                        int maxLeft = pnlScrollTrackH.Width - pnlScrollThumbH.Width;
                        int maxScroll = flpSaatler.Width - pnlSaatContent.Width;

                        if (maxLeft <= 0 || maxScroll <= 0) return;

                        int newLeft = hStartX + (Cursor.Position.X - hMouseX);
                        if (newLeft < 0) newLeft = 0;
                        if (newLeft > maxLeft) newLeft = maxLeft;

                        pnlScrollThumbH.Left = newLeft;

                        
                        float ratio = (float)newLeft / maxLeft;
                        flpSaatler.Left = -(int)(maxScroll * ratio);
                    }
                };
                pnlScrollThumbH.MouseUp += (s, e) => { isHDragging = false; };

                
                pnlSaatWrapper.Layout += (s, e) => {
                    pnlScrollTrackH.Visible = flpSaatler.Width > pnlSaatContent.Width;
                };

                flpSeanslar.Controls.Add(pnlSaatWrapper);

                
                BileseneKaydirmaEkle(pnlSaatWrapper, flpSeanslar);
                BileseneKaydirmaEkle(pnlSaatContent, flpSeanslar);
                BileseneKaydirmaEkle(pnlScrollTrackH, flpSeanslar);
                BileseneKaydirmaEkle(pnlScrollThumbH, flpSeanslar);
            }
        }

        private void Adim5_KoltuklariAydinlat()
        {
            pnlKoltukIzgarasi.Enabled = true;
            secilenKoltuklar.Clear();
            lblSecilenKoltukOzet.Text = "";

            foreach (Control c in pnlKoltukIzgarasi.Controls)
            {
                if (c is IconButton btnKoltuk)
                {
                    btnKoltuk.IconColor = renkKoltukBos;
                    btnKoltuk.Enabled = true;
                    btnKoltuk.Cursor = Cursors.Hand;
                }
            }

            btnSatinAl.BackColor = Color.FromArgb(20, 20, 25);
            btnSatinAl.ForeColor = Color.DimGray;
            btnSatinAl.Text = $"LÜTFEN {toplamBilet} ADET KOLTUK SEÇİNİZ";
            btnSatinAl.Enabled = false;

            DoluKoltuklariIsaretle();
        }

        private void DoluKoltuklariIsaretle()
        {
            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                string sorgu = "SELECT KoltukNo FROM Biletler WHERE FilmID = @film AND SinemaID = @sinema AND Tarih = @tarih";
                using (SQLiteCommand komut = new SQLiteCommand(sorgu, baglan))
                {
                    komut.Parameters.AddWithValue("@film", secilenFilmID);
                    komut.Parameters.AddWithValue("@sinema", secilenSinemaID);
                    komut.Parameters.AddWithValue("@tarih", secilenTarih + " - " + secilenSaat);

                    using (SQLiteDataReader oku = komut.ExecuteReader())
                    {
                        while (oku.Read())
                        {
                            string doluKoltuk = oku["KoltukNo"].ToString();

                            Control[] bulunanlar = pnlKoltukIzgarasi.Controls.Find(doluKoltuk, false);
                            if (bulunanlar.Length > 0)
                            {
                                IconButton btnDolu = (IconButton)bulunanlar[0];

                               
                                btnDolu.IconColor = renkKoltukDolu; 
                                btnDolu.Enabled = true; 
                                btnDolu.Cursor = Cursors.No; 
                            }
                        }
                    }
                }
            }
        }

        private void BtnKoltuk_Click(object sender, EventArgs e)
        {
            IconButton btn = (IconButton)sender;
            string koltuk = btn.Name;

            
            if (btn.IconColor == renkKoltukKaranlik || btn.IconColor == renkKoltukDolu) return;

            if (secilenKoltuklar.Contains(koltuk))
            {
                secilenKoltuklar.Remove(koltuk);
                btn.IconColor = renkKoltukBos;
            }
            else
            {
                if (secilenKoltuklar.Count < toplamBilet)
                {
                    secilenKoltuklar.Add(koltuk);
                    btn.IconColor = renkKoltukSecili;
                }
                else
                {
                    MessageBox.Show($"En fazla {toplamBilet} koltuk seçebilirsiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            lblSecilenKoltukOzet.Text = secilenKoltuklar.Count > 0 ? $"Seçilen Koltuklar: {string.Join(", ", secilenKoltuklar)}" : "";
            lblSecilenKoltukOzet.Location = new Point(pnlKoltuklarSahnesi.Width - lblSecilenKoltukOzet.Width - 20, btnSatinAl.Top - lblSecilenKoltukOzet.Height - 10);
            lblSecilenKoltukOzet.BringToFront();

            if (secilenKoltuklar.Count == toplamBilet && toplamBilet > 0)
            {
                btnSatinAl.Enabled = true;
                btnSatinAl.BackColor = renkKoltukSecili;
                btnSatinAl.ForeColor = Color.White;
                btnSatinAl.Text = "BİLETİ ONAYLA VE SATIN AL";
                btnSatinAl.Cursor = Cursors.Hand;
            }
            else
            {
                btnSatinAl.Enabled = false;
                btnSatinAl.BackColor = Color.FromArgb(20, 20, 25);
                btnSatinAl.ForeColor = Color.DimGray;
                int kalanKoltuk = toplamBilet - secilenKoltuklar.Count;
                btnSatinAl.Text = kalanKoltuk > 0 ? $"LÜTFEN {kalanKoltuk} KOLTUK DAHA SEÇİNİZ" : $"LÜTFEN {toplamBilet} KOLTUK SEÇİNİZ";
                btnSatinAl.Cursor = Cursors.Default;
            }
        }

        private void BtnSatinAl_Click(object sender, EventArgs e)
        {
            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                foreach (string koltuk in secilenKoltuklar)
                {
                    string sorgu = "INSERT INTO Biletler (KullaniciID, FilmID, SinemaID, KoltukNo, Tarih) VALUES (@kullanici, @film, @sinema, @koltuk, @tarih)";
                    using (SQLiteCommand komut = new SQLiteCommand(sorgu, baglan))
                    {
                        komut.Parameters.AddWithValue("@kullanici", Oturum.ID);
                        komut.Parameters.AddWithValue("@film", secilenFilmID);
                        komut.Parameters.AddWithValue("@sinema", secilenSinemaID);
                        komut.Parameters.AddWithValue("@koltuk", koltuk);
                        komut.Parameters.AddWithValue("@tarih", secilenTarih + " - " + secilenSaat);
                        komut.ExecuteNonQuery();
                    }
                }
            }

            MessageBox.Show("Biletleriniz başarıyla satın alındı! İşleminizi 'Biletlerim' sekmesinden görüntüleyebilirsiniz.", "İşlem Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Form1 anaForm = (Form1)this.FindForm();
            if (anaForm != null)
            {
                anaForm.SayfaYukle(new ProfilUC());
            }
        }
    }
}