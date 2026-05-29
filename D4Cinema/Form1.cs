using FontAwesome.Sharp;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data.SQLite;

namespace D4Cinema
{
    public partial class Form1 : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private Panel pnlNavbar;
        private Panel pnlAnaTasiyici;
        private Panel pnlHesapPopup;
        private FlowLayoutPanel pnlAramaPopup;
        private IconButton btnHesap;
        private TextBox txtGenelAra;
        private Panel pnlArama;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SqlBaglantisi bgl = new SqlBaglantisi();
            bgl.VeritabaniniKur();

            this.Text = "D4Cinema - Premium Edition";
            this.Size = new Size(1300, 950);
            this.BackColor = Color.FromArgb(15, 15, 15);
            this.StartPosition = FormStartPosition.CenterScreen;

           
            this.FormBorderStyle = FormBorderStyle.None;

           
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 20, 20));

            TasiyiciPaneliOlustur();
            NavbarOlustur();

            pnlHesapPopup = new Panel() { Size = new Size(160, 95), BackColor = Color.FromArgb(40, 40, 45), Visible = false };
            pnlHesapPopup.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlHesapPopup.Width, pnlHesapPopup.Height, 15, 15));
            this.Controls.Add(pnlHesapPopup);

            KullaniciArayuzunuGuncelle();
            SetupAramaPopup();

            pnlNavbar.BringToFront();
            pnlHesapPopup.BringToFront();
            pnlAramaPopup.BringToFront();

            try { SayfaYukle(new AnaSayfaUC()); } catch { }
        }

        private void NavbarOlustur()
        {
            pnlNavbar = new Panel() { Dock = DockStyle.Top, Height = 90, BackColor = Color.FromArgb(18, 18, 22) };

            
            pnlNavbar.MouseDown += (s, e) => { ReleaseCapture(); SendMessage(this.Handle, 0x112, 0xf012, 0); };
            this.Controls.Add(pnlNavbar);

           
            PictureBox pbLogo = new PictureBox()
            {
                Location = new Point(20, 10),
                Size = new Size(220, 70),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };

            string logoYolu = System.IO.Path.Combine(Application.StartupPath, "logo.png");
            if (System.IO.File.Exists(logoYolu)) pbLogo.Image = Image.FromFile(logoYolu);
            else if (System.IO.File.Exists(System.IO.Path.Combine(Application.StartupPath, "logo.jpg"))) pbLogo.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, "logo.jpg"));
            else pbLogo.BackColor = Color.FromArgb(30, 30, 34);

            pbLogo.Click += (s, e) => { try { SayfaYukle(new AnaSayfaUC()); } catch { } };
            pnlNavbar.Controls.Add(pbLogo);

            
            FlowLayoutPanel pnlNavLinks = new FlowLayoutPanel() { Location = new Point(260, 30), Size = new Size(450, 40), BackColor = Color.Transparent, WrapContents = false };

            string[] menuler = { "Filmler", "Sinemalar", "Kampanyalar" };
            IconChar[] ikonlar = { IconChar.Film, IconChar.MapMarkerAlt, IconChar.TicketAlt };

            for (int i = 0; i < menuler.Length; i++)
            {
                IconButton btnMenu = new IconButton()
                {
                    Text = menuler[i],
                    IconChar = ikonlar[i],
                    IconColor = Color.FromArgb(180, 180, 190),
                    IconSize = 22,
                    ForeColor = Color.FromArgb(180, 180, 190),
                    Font = new Font("Georgia", 13, FontStyle.Regular),
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 20, 0),
                    TextImageRelation = TextImageRelation.ImageBeforeText,
                    ImageAlign = ContentAlignment.MiddleCenter,
                    Padding = new Padding(5, 0, 5, 0)
                };
                btnMenu.FlatAppearance.BorderSize = 0;
                btnMenu.FlatAppearance.MouseDownBackColor = Color.Transparent;
                btnMenu.FlatAppearance.MouseOverBackColor = Color.Transparent;

                btnMenu.MouseEnter += (s, e) => { btnMenu.ForeColor = Color.FromArgb(145, 55, 165); btnMenu.IconColor = Color.FromArgb(145, 55, 165); };
                btnMenu.MouseLeave += (s, e) => { btnMenu.ForeColor = Color.FromArgb(180, 180, 190); btnMenu.IconColor = Color.FromArgb(180, 180, 190); };

                string seciliMenu = menuler[i];
                if (seciliMenu == "Filmler") btnMenu.Click += (s, e) => { try { SayfaYukle(new FilmlerUC()); } catch { } };
                else if (seciliMenu == "Sinemalar") btnMenu.Click += (s, e) => { try { SayfaYukle(new SinemalarUC()); } catch { } };
                else if (seciliMenu == "Kampanyalar") btnMenu.Click += (s, e) => { try { SayfaYukle(new KampanyalarUC()); } catch { } };

                pnlNavLinks.Controls.Add(btnMenu);
            }
            pnlNavbar.Controls.Add(pnlNavLinks);

            
            Panel pnlSagMenu = new Panel() { Dock = DockStyle.Right, Width = 550, BackColor = Color.Transparent };
            pnlNavbar.Controls.Add(pnlSagMenu);

           
            IconButton btnKapat = new IconButton()
            {
                IconChar = IconChar.Times,
                IconColor = Color.FromArgb(120, 40, 140),
                IconSize = 24,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 40),
                Location = new Point(490, 25),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            btnKapat.FlatAppearance.BorderSize = 0;
            btnKapat.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnKapat.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnKapat.MouseEnter += (s, e) => btnKapat.IconColor = Color.Crimson;
            btnKapat.MouseLeave += (s, e) => btnKapat.IconColor = Color.FromArgb(120, 40, 140);
            btnKapat.Click += (s, e) => Application.Exit();
            pnlSagMenu.Controls.Add(btnKapat);

          
            IconButton btnKucult = new IconButton()
            {
                IconChar = IconChar.Minus,
                IconColor = Color.FromArgb(120, 40, 140),
                IconSize = 24,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 40),
                Location = new Point(445, 25),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            btnKucult.FlatAppearance.BorderSize = 0;
            btnKucult.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnKucult.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnKucult.MouseEnter += (s, e) => btnKucult.IconColor = Color.FromArgb(145, 55, 165);
            btnKucult.MouseLeave += (s, e) => btnKucult.IconColor = Color.FromArgb(120, 40, 140);
            btnKucult.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            pnlSagMenu.Controls.Add(btnKucult);

            
            btnHesap = new IconButton()
            {
                Text = " Hesabım",
                IconChar = IconChar.UserAstronaut,
                IconColor = Color.White,
                IconSize = 24,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(120, 40, 140),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(140, 42),
                Location = new Point(285, 24),
                Cursor = Cursors.Hand,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.MiddleCenter
            };
            btnHesap.FlatAppearance.BorderSize = 0;
            btnHesap.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnHesap.Width, btnHesap.Height, 20, 20));
            btnHesap.Click += (s, e) => ToggleHesapPopup();
            pnlSagMenu.Controls.Add(btnHesap);

           
            pnlArama = new Panel() { Size = new Size(240, 42), Location = new Point(25, 24), BackColor = Color.FromArgb(25, 25, 30) };
            pnlArama.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlArama.Width, pnlArama.Height, 20, 20));

            IconPictureBox pbAramaIkon = new IconPictureBox() { IconChar = IconChar.Search, IconColor = Color.FromArgb(120, 40, 140), IconSize = 20, Location = new Point(15, 11), AutoSize = true, BackColor = Color.Transparent };
            pnlArama.Controls.Add(pbAramaIkon);

            txtGenelAra = new TextBox()
            {
                Text = "Film ara...",
                BackColor = Color.FromArgb(25, 25, 30),
                ForeColor = Color.FromArgb(120, 40, 140),
                Font = new Font("Georgia", 11, FontStyle.Italic),
                BorderStyle = BorderStyle.None,
                Location = new Point(45, 11),
                Size = new Size(180, 20)
            };

            txtGenelAra.GotFocus += (s, e) => {
                if (txtGenelAra.Text == "Film ara...")
                {
                    txtGenelAra.Text = "";
                    txtGenelAra.ForeColor = Color.White;
                    txtGenelAra.Font = new Font("Segoe UI", 11, FontStyle.Regular);
                }
            };

            txtGenelAra.LostFocus += async (s, e) => {
                await System.Threading.Tasks.Task.Delay(150);
                if (string.IsNullOrWhiteSpace(txtGenelAra.Text))
                {
                    txtGenelAra.Text = "Film ara...";
                    txtGenelAra.ForeColor = Color.FromArgb(120, 40, 140);
                    txtGenelAra.Font = new Font("Georgia", 11, FontStyle.Italic);
                }

                if (!pnlAramaPopup.ClientRectangle.Contains(pnlAramaPopup.PointToClient(Cursor.Position)))
                {
                    pnlAramaPopup.Visible = false;
                }
            };

            txtGenelAra.TextChanged += (s, e) => { CanliAramaYap(); };
            pnlArama.Controls.Add(txtGenelAra);
            pnlSagMenu.Controls.Add(pnlArama);
        }

        private void TasiyiciPaneliOlustur()
        {
            pnlAnaTasiyici = new Panel();
            pnlAnaTasiyici.Dock = DockStyle.Fill;
            pnlAnaTasiyici.BackColor = Color.FromArgb(15, 15, 15);
            this.Controls.Add(pnlAnaTasiyici);
        }

        private void SetupAramaPopup()
        {
            pnlAramaPopup = new FlowLayoutPanel() { Size = new Size(240, 150), BackColor = Color.FromArgb(35, 35, 40), Visible = false, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            pnlAramaPopup.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlAramaPopup.Width, pnlAramaPopup.Height, 15, 15));
            this.Controls.Add(pnlAramaPopup);
        }

    
        private void CanliAramaYap()
        {
            string kelime = txtGenelAra.Text.Trim();
            if (string.IsNullOrWhiteSpace(kelime) || kelime == "Film ara..." || kelime.Length < 2) { pnlAramaPopup.Visible = false; return; }

            pnlAramaPopup.Controls.Clear();
            bool sonucVar = false;

            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                
                string sorgu = "SELECT ID, FilmAdi, Durum FROM Filmler WHERE FilmAdi LIKE @kelime OR Konu LIKE @kelime LIMIT 5";
                using (SQLiteCommand komut = new SQLiteCommand(sorgu, baglan))
                {
                    komut.Parameters.AddWithValue("@kelime", "%" + kelime + "%");
                    using (SQLiteDataReader oku = komut.ExecuteReader())
                    {
                        while (oku.Read())
                        {
                            sonucVar = true;
                            int filmID = Convert.ToInt32(oku["ID"]);
                            string filmAdi = oku["FilmAdi"].ToString();
                            string durum = oku["Durum"].ToString();

                           
                            string gosterimMetni = durum.Contains("Yakın") ? $"{filmAdi} (Yakında)" : filmAdi;
                            IconChar ikonTuru = durum.Contains("Yakın") ? IconChar.CalendarDays : IconChar.Film;

                            IconButton btnSonuc = new IconButton()
                            {
                                Text = " " + gosterimMetni,
                                IconChar = ikonTuru,
                                IconColor = Color.FromArgb(145, 55, 165),
                                IconSize = 18,
                                ForeColor = Color.White,
                                BackColor = Color.Transparent,
                                FlatStyle = FlatStyle.Flat,
                                Size = new Size(220, 38),
                                TextAlign = ContentAlignment.MiddleLeft,
                                ImageAlign = ContentAlignment.MiddleLeft,
                                TextImageRelation = TextImageRelation.ImageBeforeText,
                                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                                Cursor = Cursors.Hand,
                                Margin = new Padding(10, 5, 0, 0),
                                Padding = new Padding(5, 0, 0, 0)
                            };
                            btnSonuc.FlatAppearance.BorderSize = 0;
                            btnSonuc.MouseEnter += (s, e) => { btnSonuc.BackColor = Color.FromArgb(120, 40, 140); btnSonuc.IconColor = Color.White; };
                            btnSonuc.MouseLeave += (s, e) => { btnSonuc.BackColor = Color.Transparent; btnSonuc.IconColor = Color.FromArgb(145, 55, 165); };

                           
                            btnSonuc.Click += (s, e) => {
                                pnlAramaPopup.Visible = false;
                                txtGenelAra.Text = "Film ara...";
                                txtGenelAra.ForeColor = Color.FromArgb(120, 40, 140);
                                txtGenelAra.Font = new Font("Georgia", 11, FontStyle.Italic);
                                this.Focus();

                                try { SayfaYukle(new FilmDetayUC(filmID)); } catch { MessageBox.Show("Film detay sayfası yüklenemedi!"); }
                            };
                            pnlAramaPopup.Controls.Add(btnSonuc);
                        }
                    }
                }
            }

            if (!sonucVar)
                pnlAramaPopup.Controls.Add(new Label() { Text = "Sonuç bulunamadı...", ForeColor = Color.Gray, Font = new Font("Segoe UI", 9, FontStyle.Italic), AutoSize = true, Margin = new Padding(10, 15, 0, 0) });

            Point screencoord = pnlArama.Parent.PointToScreen(pnlArama.Location);
            Point formcoord = this.PointToClient(screencoord);
            pnlAramaPopup.Location = new Point(formcoord.X, formcoord.Y + pnlArama.Height + 5);
            pnlAramaPopup.Height = Math.Min(220, pnlAramaPopup.Controls.Count * 44 + 12);
            pnlAramaPopup.Visible = true;
            pnlAramaPopup.BringToFront();
        }

        public void KullaniciArayuzunuGuncelle()
        {
            pnlHesapPopup.Controls.Clear();

            if (Oturum.GirisYapildiMi)
            {
                if (Oturum.Rol == "Admin")
                {
                    btnHesap.Text = " Admin";
                    btnHesap.IconChar = IconChar.Crown;
                }
                else
                {
                    btnHesap.Text = " " + Oturum.AdSoyad.Split(' ')[0];
                    btnHesap.IconChar = IconChar.UserAstronaut;
                }

                btnHesap.BackColor = Color.FromArgb(120, 40, 140);
                btnHesap.ForeColor = Color.White;
                btnHesap.IconColor = Color.White;

                IconButton btnCikis = new IconButton() { Text = " Çıkış Yap", IconChar = IconChar.SignOutAlt, IconColor = Color.White, IconSize = 20, TextImageRelation = TextImageRelation.ImageBeforeText, ImageAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Top, Height = 45, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, Padding = new Padding(10, 0, 0, 0) };
                btnCikis.FlatAppearance.BorderSize = 0;
                btnCikis.MouseEnter += (s, e) => { btnCikis.BackColor = Color.Crimson; };
                btnCikis.MouseLeave += (s, e) => { btnCikis.BackColor = Color.Transparent; };
                btnCikis.Click += (s, e) => {
                    pnlHesapPopup.Visible = false;
                    Oturum.CikisYap();
                    KullaniciArayuzunuGuncelle();
                    SayfaYukle(new AnaSayfaUC());
                };

                Panel pnlSep = new Panel() { Dock = DockStyle.Top, Height = 2, BackColor = Color.FromArgb(30, 30, 30) };

                IconButton btnProfil = new IconButton() { Text = Oturum.Rol == "Admin" ? " Yönetim Paneli" : " Biletlerim", IconChar = Oturum.Rol == "Admin" ? IconChar.Cogs : IconChar.TicketAlt, IconColor = Color.White, IconSize = 20, TextImageRelation = TextImageRelation.ImageBeforeText, ImageAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Top, Height = 48, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, Padding = new Padding(10, 0, 0, 0) };
                btnProfil.FlatAppearance.BorderSize = 0;
                btnProfil.MouseEnter += (s, e) => { btnProfil.BackColor = Color.FromArgb(120, 40, 140); };
                btnProfil.MouseLeave += (s, e) => { btnProfil.BackColor = Color.Transparent; };
                btnProfil.Click += (s, e) => {
                    pnlHesapPopup.Visible = false;
                    if (Oturum.Rol == "Admin") SayfaYukle(new AdminPanelUC());
                    else SayfaYukle(new ProfilUC());
                };

                pnlHesapPopup.Controls.Add(btnCikis);
                pnlHesapPopup.Controls.Add(pnlSep);
                pnlHesapPopup.Controls.Add(btnProfil);
            }
            else
            {
                btnHesap.Text = " Hesabım";
                btnHesap.IconChar = IconChar.UserCircle;

                btnHesap.BackColor = Color.FromArgb(120, 40, 140);
                btnHesap.ForeColor = Color.White;
                btnHesap.IconColor = Color.White;

                IconButton btnDropGiris = new IconButton() { Text = " Giriş Yap", IconChar = IconChar.SignInAlt, IconColor = Color.White, IconSize = 20, TextImageRelation = TextImageRelation.ImageBeforeText, ImageAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Top, Height = 45, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, Padding = new Padding(10, 0, 0, 0) };
                btnDropGiris.FlatAppearance.BorderSize = 0;
                btnDropGiris.MouseEnter += (s, e) => { btnDropGiris.BackColor = Color.FromArgb(120, 40, 140); };
                btnDropGiris.MouseLeave += (s, e) => { btnDropGiris.BackColor = Color.Transparent; };
                btnDropGiris.Click += (s, e) => { pnlHesapPopup.Visible = false; try { SayfaYukle(new HesapUC(true)); } catch { } };

                Panel pnlSep = new Panel() { Dock = DockStyle.Top, Height = 2, BackColor = Color.FromArgb(30, 30, 30) };

                IconButton btnDropUye = new IconButton() { Text = " Yeni Üyelik", IconChar = IconChar.UserPlus, IconColor = Color.White, IconSize = 20, TextImageRelation = TextImageRelation.ImageBeforeText, ImageAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Top, Height = 48, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, Padding = new Padding(10, 0, 0, 0) };
                btnDropUye.FlatAppearance.BorderSize = 0;
                btnDropUye.MouseEnter += (s, e) => { btnDropUye.BackColor = Color.FromArgb(120, 40, 140); };
                btnDropUye.MouseLeave += (s, e) => { btnDropUye.BackColor = Color.Transparent; };
                btnDropUye.Click += (s, e) => { pnlHesapPopup.Visible = false; try { SayfaYukle(new HesapUC(false)); } catch { } };

                pnlHesapPopup.Controls.Add(btnDropGiris);
                pnlHesapPopup.Controls.Add(pnlSep);
                pnlHesapPopup.Controls.Add(btnDropUye);
            }
        }

        private void ToggleHesapPopup()
        {
            if (pnlHesapPopup.Visible) pnlHesapPopup.Visible = false;
            else
            {
                Point screencoord = btnHesap.Parent.PointToScreen(btnHesap.Location);
                Point formcoord = this.PointToClient(screencoord);
                pnlHesapPopup.Location = new Point(formcoord.X, formcoord.Y + btnHesap.Height + 5);
                pnlHesapPopup.Visible = true;
                pnlHesapPopup.BringToFront();
            }
        }

        public void SayfaYukle(UserControl sayfa)
        {
            pnlAnaTasiyici.Controls.Clear();
            sayfa.Dock = DockStyle.Fill;
            pnlAnaTasiyici.Controls.Add(sayfa);
            pnlNavbar.BringToFront();
        }
    }
}