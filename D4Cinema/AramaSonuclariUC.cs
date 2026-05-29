using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data.SQLite;

namespace D4Cinema
{
    public partial class AramaSonuclariUC : UserControl
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private FlowLayoutPanel pnlListe;
        private string arananKelime;

        
        public AramaSonuclariUC(string aranan)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(15, 15, 15);
            this.arananKelime = aranan;

            ArayuzuCiz();
            VeritabanindanAra();

           
            this.Resize += (s, e) => {
                foreach (Control item in pnlListe.Controls)
                {
                    if (item is Panel && item.Name == "FilmKarti")
                    {
                        item.Width = Math.Max(pnlListe.Width - 100, 400);
                        item.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, item.Width, item.Height, 20, 20));
                        foreach (Control child in item.Controls)
                        {
                            if (child.Name == "KonuYazisi") child.Width = item.Width - 220;
                        }
                    }
                }
            };
        }

        private void ArayuzuCiz()
        {
            Panel pnlUst = new Panel() { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(15, 15, 15) };

            Label lblBaslik = new Label() { Text = $"🔍 '{arananKelime}' İçin Arama Sonuçları", ForeColor = Color.White, Font = new Font("Segoe UI", 22, FontStyle.Bold), AutoSize = true, Location = new Point(40, 30) };
            pnlUst.Controls.Add(lblBaslik);
            this.Controls.Add(pnlUst);

            pnlListe = new FlowLayoutPanel() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 15, 15), Padding = new Padding(40, 10, 40, 50), FlowDirection = FlowDirection.TopDown, WrapContents = false };
            this.Controls.Add(pnlListe);
            pnlListe.BringToFront();
        }

        private void VeritabanindanAra()
        {
            int bulunanSayisi = 0;
            SqlBaglantisi bgl = new SqlBaglantisi();

            
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                
                string sorgu = "SELECT * FROM Filmler WHERE FilmAdi LIKE @kelime OR Konu LIKE @kelime";

                using (SQLiteCommand komut = new SQLiteCommand(sorgu, baglan))
                {
                    
                    komut.Parameters.AddWithValue("@kelime", "%" + arananKelime + "%");

                    using (SQLiteDataReader oku = komut.ExecuteReader())
                    {
                        while (oku.Read())
                        {
                            string ad = oku["FilmAdi"].ToString();
                            string tur = oku["Tur"].ToString();
                            string sure = oku["Sure"].ToString();
                            string konu = oku["Konu"].ToString();

                            pnlListe.Controls.Add(SonucKartiOlustur(ad, tur, sure, konu));
                            bulunanSayisi++;
                        }
                    }
                }
            }

            
            if (bulunanSayisi == 0)
            {
                Label lblHata = new Label() { Text = "Maalesef aradığın kriterlere uygun bir film bulamadık.", ForeColor = Color.Gray, Font = new Font("Segoe UI", 14, FontStyle.Italic), AutoSize = true, Margin = new Padding(10, 20, 0, 0) };
                pnlListe.Controls.Add(lblHata);
            }
        }

        private Panel SonucKartiOlustur(string filmAdi, string tur, string sure, string konu)
        {
            int baslangicGenislik = this.Width > 200 ? this.Width - 100 : 1000;
            Panel satir = new Panel() { Name = "FilmKarti", Size = new Size(baslangicGenislik, 220), BackColor = Color.FromArgb(28, 28, 28), Margin = new Padding(0, 0, 0, 25) };
            satir.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, satir.Width, satir.Height, 20, 20));

            Panel afis = new Panel() { Size = new Size(150, 220), Location = new Point(0, 0), BackColor = Color.FromArgb(45, 45, 45) };
            afis.Controls.Add(new Label() { Text = "🎬", Font = new Font("Segoe UI", 30), ForeColor = Color.FromArgb(80, 80, 80), AutoSize = true, Location = new Point(45, 80) });
            satir.Controls.Add(afis);

            int solBosluk = 180;
            satir.Controls.Add(new Label() { Text = filmAdi, ForeColor = Color.White, Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(solBosluk, 20), AutoSize = true });
            satir.Controls.Add(new Label() { Text = "🎭 Tür: " + tur, ForeColor = Color.FromArgb(170, 170, 170), Font = new Font("Segoe UI", 10, FontStyle.Regular), Location = new Point(solBosluk, 60), AutoSize = true });
            satir.Controls.Add(new Label() { Text = "🕒 Süre: " + sure, ForeColor = Color.FromArgb(229, 9, 20), Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(solBosluk + 220, 60), AutoSize = true });
            satir.Controls.Add(new Label() { Name = "KonuYazisi", Text = konu, ForeColor = Color.FromArgb(200, 200, 200), Font = new Font("Segoe UI", 11, FontStyle.Regular), Location = new Point(solBosluk, 100), Size = new Size(780, 50) });

            Button btnBilet = new Button() { Text = "Bilet Al", BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Size = new Size(150, 35), Location = new Point(solBosluk, 165), Cursor = Cursors.Hand };
            btnBilet.FlatAppearance.BorderSize = 0; btnBilet.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnBilet.Width, btnBilet.Height, 10, 10));
            btnBilet.MouseEnter += (s, e) => btnBilet.BackColor = Color.FromArgb(229, 9, 20);
            btnBilet.MouseLeave += (s, e) => btnBilet.BackColor = Color.FromArgb(50, 50, 50);
            satir.Controls.Add(btnBilet);

            return satir;
        }
    }
}