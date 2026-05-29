using FontAwesome.Sharp;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data.SQLite;

namespace D4Cinema
{
    public partial class AdminPanelUC : UserControl
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        
        private Color renkArkaPlan = Color.FromArgb(18, 18, 22);
        private Color renkSolMenu = Color.FromArgb(22, 22, 26);
        private Color renkD4Moru = Color.FromArgb(120, 40, 140);

        private Panel pnlSolMenu;
        private Panel pnlAdminIcerik;

        public AdminPanelUC()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = renkArkaPlan;
            this.Padding = new Padding(0, 90, 0, 0); 

            ArayuzuCiz();

            
            AdminIcerikDegistir(DashboardOlustur());
        }

        private void ArayuzuCiz()
        {
            
            pnlSolMenu = new Panel() { Dock = DockStyle.Left, Width = 260, BackColor = renkSolMenu };
            this.Controls.Add(pnlSolMenu);

            IconPictureBox ikonBaslik = new IconPictureBox() { IconChar = IconChar.Tools, IconColor = renkD4Moru, IconSize = 26, Location = new Point(20, 25), AutoSize = true, BackColor = Color.Transparent };
            pnlSolMenu.Controls.Add(ikonBaslik);

            Label lblAdminBaslik = new Label() { Text = "D4 Yönetim", ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(50, 21), AutoSize = true };
            pnlSolMenu.Controls.Add(lblAdminBaslik);

            
            MenuButonuEkle("Dashboard (Özet)", IconChar.ChartPie, 90);
            MenuButonuEkle("Film Ekle / Düzenle", IconChar.Film, 145);
            MenuButonuEkle("Sinema Salonu Ayarları", IconChar.MapMarkerAlt, 200);
            MenuButonuEkle("Kullanıcı Yönetimi", IconChar.Users, 255);
            MenuButonuEkle("Çıkış Yap", IconChar.SignOutAlt, 310);

            
            pnlAdminIcerik = new Panel() { Dock = DockStyle.Fill, BackColor = renkArkaPlan };
            this.Controls.Add(pnlAdminIcerik);
            pnlAdminIcerik.BringToFront();
        }

        
        private void MenuButonuEkle(string yazi, IconChar ikon, int yLokasyon)
        {
            IconButton btnMenu = new IconButton()
            {
                Text = "  " + yazi,
                IconChar = ikon,
                IconColor = Color.FromArgb(160, 160, 170),
                IconSize = 26,
                ForeColor = Color.FromArgb(160, 160, 170),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(220, 45),
                Location = new Point(20, yLokasyon),
                TextAlign = ContentAlignment.MiddleLeft,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding(10, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btnMenu.FlatAppearance.BorderSize = 0;
            btnMenu.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnMenu.Width, btnMenu.Height, 8, 8));

            
            if (yazi == "Çıkış Yap")
            {
                btnMenu.MouseEnter += (s, e) => { btnMenu.ForeColor = Color.Crimson; btnMenu.IconColor = Color.Crimson; btnMenu.BackColor = Color.FromArgb(40, 20, 25); };
                btnMenu.MouseLeave += (s, e) => { btnMenu.ForeColor = Color.FromArgb(160, 160, 170); btnMenu.IconColor = Color.FromArgb(160, 160, 170); btnMenu.BackColor = Color.Transparent; };

                btnMenu.Click += (s, e) => {
                    Oturum.CikisYap();
                    Form1 anaForm = (Form1)this.FindForm();
                    if (anaForm != null) { anaForm.KullaniciArayuzunuGuncelle(); anaForm.SayfaYukle(new AnaSayfaUC()); }
                };
            }
            else
            {
                btnMenu.MouseEnter += (s, e) => { btnMenu.ForeColor = Color.White; btnMenu.IconColor = renkD4Moru; btnMenu.BackColor = Color.FromArgb(40, 40, 45); };
                btnMenu.MouseLeave += (s, e) => { btnMenu.ForeColor = Color.FromArgb(160, 160, 170); btnMenu.IconColor = Color.FromArgb(160, 160, 170); btnMenu.BackColor = Color.Transparent; };

                btnMenu.Click += (s, e) => {
                    
                    if (yazi == "Dashboard (Özet)") AdminIcerikDegistir(DashboardOlustur()); 
                    else if (yazi == "Film Ekle / Düzenle") { try { AdminIcerikDegistir(new FilmYonetimUC()); } catch { } }
                    else if (yazi == "Sinema Salonu Ayarları") { try { AdminIcerikDegistir(new SinemaYonetimUC()); } catch { } }
                    else if (yazi == "Kullanıcı Yönetimi") { try { AdminIcerikDegistir(new KullaniciYonetimiUC()); } catch { } }
                };
            }

            pnlSolMenu.Controls.Add(btnMenu);
        }

       
        public static void TabloyuD4TemasinaCevir(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BorderStyle = BorderStyle.None;
            dgv.BackgroundColor = Color.FromArgb(18, 18, 22);
            dgv.GridColor = Color.FromArgb(40, 40, 45);
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToAddRows = false;

           
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 20, 25);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(180, 80, 200);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(20, 20, 25);
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersHeight = 45;

            
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(28, 28, 34);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(220, 220, 230);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(120, 40, 140);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(32, 32, 38);
            dgv.RowTemplate.Height = 40;
        }

        private void AdminIcerikDegistir(Control yeniIcerik)
        {
            if (yeniIcerik == null) return;
            pnlAdminIcerik.Controls.Clear();
            yeniIcerik.Dock = DockStyle.Fill;
            pnlAdminIcerik.Controls.Add(yeniIcerik);
        }

       
        private Panel DashboardOlustur()
        {
            Panel pnlDash = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(40) };

            Label lblBaslik = new Label() { Text = "Sistem İstatistikleri", ForeColor = Color.White, Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Location = new Point(40, 40) };
            pnlDash.Controls.Add(lblBaslik);

            FlowLayoutPanel flpKartlar = new FlowLayoutPanel() { Location = new Point(40, 110), AutoSize = true, MaximumSize = new Size(1000, 0), WrapContents = true };
            pnlDash.Controls.Add(flpKartlar);

            
            int filmSayisi = IstatistikCek("SELECT COUNT(*) FROM Filmler");
            int salonSayisi = IstatistikCek("SELECT COUNT(*) FROM Sinemalar WHERE Durum='Aktif'");
          
            int uyeSayisi = IstatistikCek("SELECT COUNT(*) FROM Kullanicilar");
            int biletSayisi = IstatistikCek("SELECT COUNT(*) FROM Biletler");

            flpKartlar.Controls.Add(IstatistikKartiOlustur("Kayıtlı Film", filmSayisi.ToString(), IconChar.Film));
            flpKartlar.Controls.Add(IstatistikKartiOlustur("Aktif Salon", salonSayisi.ToString(), IconChar.MapMarkerAlt));
            flpKartlar.Controls.Add(IstatistikKartiOlustur("Sistemdeki Üye", uyeSayisi.ToString(), IconChar.Users));
            flpKartlar.Controls.Add(IstatistikKartiOlustur("Satılan Bilet", biletSayisi.ToString(), IconChar.TicketAlt));

            return pnlDash;
        }

        
        private int IstatistikCek(string sorgu)
        {
            try
            {
                SqlBaglantisi bgl = new SqlBaglantisi();
                using (SQLiteConnection baglan = bgl.Baglanti())
                {
                    using (SQLiteCommand komut = new SQLiteCommand(sorgu, baglan))
                    {
                        object sonuc = komut.ExecuteScalar();
                        if (sonuc != null && sonuc != DBNull.Value) return Convert.ToInt32(sonuc);
                    }
                }
            }
            catch { }
            return 0;
        }

        private Panel IstatistikKartiOlustur(string baslik, string deger, IconChar ikon)
        {
            Panel kart = new Panel() { Size = new Size(220, 140), BackColor = Color.FromArgb(28, 28, 34), Margin = new Padding(0, 0, 20, 20) };
            kart.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, kart.Width, kart.Height, 15, 15));

            Panel cizgi = new Panel() { Dock = DockStyle.Left, Width = 6, BackColor = renkD4Moru };
            kart.Controls.Add(cizgi);

            IconPictureBox icon = new IconPictureBox() { IconChar = ikon, IconColor = Color.FromArgb(145, 55, 165), IconSize = 40, Location = new Point(20, 20), AutoSize = true, BackColor = Color.Transparent };
            kart.Controls.Add(icon);

            Label lblDeger = new Label() { Text = deger, ForeColor = Color.White, Font = new Font("Segoe UI", 28, FontStyle.Bold), Location = new Point(15, 65), AutoSize = true };
            kart.Controls.Add(lblDeger);

            Label lblBaslik = new Label() { Text = baslik, ForeColor = Color.FromArgb(160, 160, 170), Font = new Font("Segoe UI", 11, FontStyle.Regular), Location = new Point(20, 110), AutoSize = true };
            kart.Controls.Add(lblBaslik);

            return kart;
        }
    }
}