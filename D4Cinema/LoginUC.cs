using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data.SQLite;

namespace D4Cinema
{
    public partial class LoginUC : UserControl
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private TextBox txtEmail, txtSifre;

        
        public LoginUC()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(15, 15, 15);

            ArayuzuCiz();
        }

        private void ArayuzuCiz()
        {
            Panel pnlKutu = new Panel() { Size = new Size(400, 430), BackColor = Color.FromArgb(25, 25, 25) };
            pnlKutu.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlKutu.Width, pnlKutu.Height, 20, 20));
            this.Controls.Add(pnlKutu);

            this.Resize += (s, e) => {
                pnlKutu.Location = new Point((this.Width - pnlKutu.Width) / 2, (this.Height - pnlKutu.Height) / 2);
            };

            Label lblBaslik = new Label() { Text = "🔑 D4Cinema Giriş", ForeColor = Color.White, Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, Location = new Point(40, 40) };
            pnlKutu.Controls.Add(lblBaslik);

            pnlKutu.Controls.Add(new Label() { Text = "E-Posta Adresi", ForeColor = Color.Gray, Location = new Point(40, 120), AutoSize = true });
            txtEmail = new TextBox() { Width = 320, Location = new Point(40, 145), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 11) };
            pnlKutu.Controls.Add(txtEmail);

            pnlKutu.Controls.Add(new Label() { Text = "Şifre", ForeColor = Color.Gray, Location = new Point(40, 200), AutoSize = true });
            txtSifre = new TextBox() { Width = 320, Location = new Point(40, 225), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 11), UseSystemPasswordChar = true };
            pnlKutu.Controls.Add(txtSifre);

            Button btnGris = new Button() { Text = "GİRİŞ YAP", BackColor = Color.FromArgb(229, 9, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(40, 300), Size = new Size(320, 45), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnGris.FlatAppearance.BorderSize = 0;
            btnGris.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnGris.Width, btnGris.Height, 10, 10));
            btnGris.Click += BtnGiris_Click;
            pnlKutu.Controls.Add(btnGris);

            Label lblIpucu = new Label() { Text = "💡 Test Üye: ahmet@gmail.com / 123456", ForeColor = Color.DimGray, Font = new Font("Segoe UI", 9, FontStyle.Italic), Location = new Point(40, 370), AutoSize = true };
            pnlKutu.Controls.Add(lblIpucu);
        }

        private void BtnGiris_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtSifre.Text))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun!"); return;
            }

            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                using (SQLiteCommand komut = new SQLiteCommand("SELECT * FROM Kullanicilar WHERE Eposta=@email AND Sifre=@sifre", baglan))
                {
                    komut.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    komut.Parameters.AddWithValue("@sifre", txtSifre.Text);

                    using (SQLiteDataReader oku = komut.ExecuteReader())
                    {
                        if (oku.Read())
                        {
                           
                            MevcutKullanici.ID = Convert.ToInt32(oku["ID"]);
                            MevcutKullanici.AdSoyad = oku["AdSoyad"].ToString();
                            MevcutKullanici.Rol = oku["Rol"].ToString();

                            MessageBox.Show($"Hoş geldiniz, {MevcutKullanici.AdSoyad}!", "D4Cinema", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            
                            Control anaKonteyner = this.Parent;
                            if (anaKonteyner != null)
                            {
                                
                                if (MevcutKullanici.BekleyenSayfa != null)
                                {
                                    
                                    anaKonteyner.Controls.Add(MevcutKullanici.BekleyenSayfa);
                                    MevcutKullanici.BekleyenSayfa.BringToFront();

                                    
                                    MevcutKullanici.BekleyenSayfa = null;
                                }
                                else
                                {
                                    
                                    AnaSayfaUC anaSayfa = new AnaSayfaUC();
                                    anaKonteyner.Controls.Add(anaSayfa);
                                    anaSayfa.BringToFront();
                                }
                                this.Dispose(); 
                            }
                        }
                        else
                        {
                            MessageBox.Show("E-posta veya şifre hatalı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}