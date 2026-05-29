using FontAwesome.Sharp;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data.SQLite;

namespace D4Cinema
{
    public partial class HesapUC : UserControl
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private bool girisModu;
        private Panel pnlKart;
        private IconPictureBox pbIkon;
        private Label lblBaslik;

       
        private Panel pnlAd;
        private TextBox txtAd;
        private Panel pnlSoyad;
        private TextBox txtSoyad;

        private Panel pnlEposta;
        private TextBox txtEposta;
        private Panel pnlSifre;
        private TextBox txtSifre;
        private Button btnIslem;
        private Label lblGecis;

        public HesapUC(bool baslangicGirisModu = true)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.girisModu = baslangicGirisModu;
            this.BackColor = Color.Transparent;

            ArayuzuCiz();
            ModuUygula();

            this.Resize += (s, e) => {
                if (pnlKart != null) pnlKart.Location = new Point((this.Width - pnlKart.Width) / 2, (this.Height - pnlKart.Height) / 2);
                this.Invalidate();
            };
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            using (LinearGradientBrush firca = new LinearGradientBrush(this.ClientRectangle,
                Color.FromArgb(18, 18, 22),
                Color.FromArgb(40, 15, 50),
                LinearGradientMode.ForwardDiagonal))
            {
                e.Graphics.FillRectangle(firca, this.ClientRectangle);
            }
        }

        private void ArayuzuCiz()
        {
            pnlKart = new Panel() { Size = new Size(420, 560), BackColor = Color.FromArgb(28, 28, 34) };
            pnlKart.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlKart.Width, pnlKart.Height, 20, 20));

            pbIkon = new IconPictureBox() { IconColor = Color.FromArgb(145, 55, 165), IconSize = 64, Size = new Size(64, 64), Location = new Point((pnlKart.Width - 64) / 2, 40), BackColor = Color.Transparent };
            pnlKart.Controls.Add(pbIkon);

            lblBaslik = new Label() { ForeColor = Color.White, Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = false, Size = new Size(420, 40), TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, 110) };
            pnlKart.Controls.Add(lblBaslik);

            // Ayrılmış Yeni Giriş Kutuları
            pnlAd = ModernInputOlustur(out txtAd, "Adınız", false);
            pnlKart.Controls.Add(pnlAd);

            pnlSoyad = ModernInputOlustur(out txtSoyad, "Soyadınız", false);
            pnlKart.Controls.Add(pnlSoyad);

            pnlEposta = ModernInputOlustur(out txtEposta, "E-Posta Adresi", false);
            pnlKart.Controls.Add(pnlEposta);

            pnlSifre = ModernInputOlustur(out txtSifre, "Şifre", true);
            pnlKart.Controls.Add(pnlSifre);

            btnIslem = new Button() { BackColor = Color.FromArgb(120, 40, 140), ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Size = new Size(320, 50), Cursor = Cursors.Hand };
            btnIslem.FlatAppearance.BorderSize = 0;
            btnIslem.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnIslem.Width, btnIslem.Height, 10, 10));
            btnIslem.MouseEnter += (s, e) => btnIslem.BackColor = Color.FromArgb(145, 55, 165);
            btnIslem.MouseLeave += (s, e) => btnIslem.BackColor = Color.FromArgb(120, 40, 140);

            
            btnIslem.Click += (s, e) =>
            {
                string eposta = txtEposta.Text.Trim();
                string sifre = txtSifre.Text.Trim();
                string ad = txtAd.Text.Trim();
                string soyad = txtSoyad.Text.Trim();

                if (girisModu)
                {
                    if (eposta == "E-Posta Adresi" || sifre == "Şifre") { MessageBox.Show("Lütfen tüm alanları doldurun!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                    SqlBaglantisi bgl = new SqlBaglantisi();
                    using (SQLiteConnection baglan = bgl.Baglanti())
                    {
                        using (SQLiteCommand komut = new SQLiteCommand("SELECT * FROM Kullanicilar WHERE Eposta = @eposta AND Sifre = @sifre", baglan))
                        {
                            komut.Parameters.AddWithValue("@eposta", eposta);
                            komut.Parameters.AddWithValue("@sifre", sifre);
                            using (SQLiteDataReader oku = komut.ExecuteReader())
                            {
                                if (oku.Read())
                                {
                                    Oturum.GirisYapildiMi = true;

                                   
                                    Oturum.AdSoyad = oku["Ad"].ToString() + " " + oku["Soyad"].ToString();
                                    Oturum.Rol = oku["Rol"].ToString();
                                    Oturum.Eposta = eposta;
                                    Oturum.ID = Convert.ToInt32(oku["ID"]);

                                    Form1 anaForm = (Form1)this.FindForm();
                                    if (anaForm != null)
                                    {
                                        anaForm.KullaniciArayuzunuGuncelle();
                                        if (Oturum.BekleyenSayfa != null) { anaForm.SayfaYukle(Oturum.BekleyenSayfa); Oturum.BekleyenSayfa = null; }
                                        else if (Oturum.Rol == "Admin") anaForm.SayfaYukle(new AdminPanelUC());
                                        else anaForm.SayfaYukle(new AnaSayfaUC());
                                    }
                                }
                                else { MessageBox.Show("E-posta veya şifre hatalı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                            }
                        }
                    }
                }
                else
                {
                    if (ad == "Adınız" || soyad == "Soyadınız" || eposta == "E-Posta Adresi" || sifre == "Şifre") { MessageBox.Show("Lütfen tüm alanları doldurun!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                    SqlBaglantisi bgl = new SqlBaglantisi();
                    try
                    {
                        using (SQLiteConnection baglan = bgl.Baglanti())
                        {
                            
                            using (SQLiteCommand komut = new SQLiteCommand("INSERT INTO Kullanicilar (Ad, Soyad, Eposta, Sifre, Rol) VALUES (@ad, @soyad, @eposta, @sifre, 'Uye')", baglan))
                            {
                                komut.Parameters.AddWithValue("@ad", ad);
                                komut.Parameters.AddWithValue("@soyad", soyad);
                                komut.Parameters.AddWithValue("@eposta", eposta);
                                komut.Parameters.AddWithValue("@sifre", sifre);
                                komut.ExecuteNonQuery();
                            }
                        }
                        MessageBox.Show("Kayıt başarıyla tamamlandı! Giriş yapabilirsiniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        girisModu = true;
                        ModuUygula();
                    }
                    catch (Exception) { MessageBox.Show("Bu e-posta adresi zaten kayıtlı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            };
            pnlKart.Controls.Add(btnIslem);

            lblGecis = new Label() { ForeColor = Color.FromArgb(180, 180, 190), Font = new Font("Segoe UI", 10, FontStyle.Underline), AutoSize = false, Size = new Size(420, 30), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };
            lblGecis.MouseEnter += (s, e) => lblGecis.ForeColor = Color.FromArgb(145, 55, 165);
            lblGecis.MouseLeave += (s, e) => lblGecis.ForeColor = Color.FromArgb(180, 180, 190);
            lblGecis.Click += (s, e) => { girisModu = !girisModu; ModuUygula(); };
            pnlKart.Controls.Add(lblGecis);

            this.Controls.Add(pnlKart);
        }

        private Panel ModernInputOlustur(out TextBox txtBox, string placeholder, bool isPassword)
        {
            Panel pnlBg = new Panel() { Size = new Size(320, 50), BackColor = Color.FromArgb(40, 40, 45) };
            pnlBg.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlBg.Width, pnlBg.Height, 10, 10));

            txtBox = new TextBox() { Text = placeholder, ForeColor = Color.Gray, BackColor = Color.FromArgb(40, 40, 45), BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 12), Location = new Point(15, 14), Width = 290 };

            TextBox txtRef = txtBox;
            txtBox.Enter += (s, e) => { if (txtRef.Text == placeholder) { txtRef.Text = ""; txtRef.ForeColor = Color.White; if (isPassword) txtRef.UseSystemPasswordChar = true; } };
            txtBox.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtRef.Text)) { txtRef.Text = placeholder; txtRef.ForeColor = Color.Gray; if (isPassword) txtRef.UseSystemPasswordChar = false; } };

            pnlBg.Controls.Add(txtBox);
            return pnlBg;
        }

        private void ModuUygula()
        {
            if (girisModu)
            {
                pbIkon.IconChar = IconChar.UserAstronaut;
                lblBaslik.Text = "Tekrar Hoş Geldin";

                
                pnlAd.Visible = false;
                pnlSoyad.Visible = false;

                pnlEposta.Location = new Point(50, 180);
                pnlSifre.Location = new Point(50, 250);
                btnIslem.Text = "Giriş Yap";
                btnIslem.Location = new Point(50, 320);
                lblGecis.Text = "Hesabın yok mu? Kayıt Ol.";
                lblGecis.Location = new Point(0, 390);

                pnlKart.Height = 450;
                pnlKart.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlKart.Width, pnlKart.Height, 20, 20));
            }
            else
            {
                pbIkon.IconChar = IconChar.UserPlus;
                lblBaslik.Text = "Aramıza Katıl";

                
                pnlAd.Visible = true;
                pnlSoyad.Visible = true;

                pnlAd.Location = new Point(50, 175);
                pnlSoyad.Location = new Point(50, 240);
                pnlEposta.Location = new Point(50, 305);
                pnlSifre.Location = new Point(50, 370);

                btnIslem.Text = "Kayıt Ol";
                btnIslem.Location = new Point(50, 440);
                lblGecis.Text = "Zaten hesabın var mı? Giriş Yap.";
                lblGecis.Location = new Point(0, 510);

                
                pnlKart.Height = 570;
                pnlKart.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlKart.Width, pnlKart.Height, 20, 20));
            }

            pnlKart.Location = new Point((this.Width - pnlKart.Width) / 2, (this.Height - pnlKart.Height) / 2);
        }
    }
}