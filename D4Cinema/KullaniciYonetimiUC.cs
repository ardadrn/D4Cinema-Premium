using FontAwesome.Sharp;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Data;

namespace D4Cinema
{
    public partial class KullaniciYonetimiUC : UserControl
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private DataGridView dgvKullanicilar;
        private DataGridView dgvBiletler;
        private Button btnSil;
        private Label lblSeciliKullanici;
        private int secilenKullaniciID = 0;

        public KullaniciYonetimiUC()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(18, 18, 22);

            ArayuzuCiz();
            KullanicilariYukle();
        }

        private void ArayuzuCiz()
        {
            IconPictureBox iconBaslik = new IconPictureBox()
            {
                IconChar = IconChar.Users,
                IconColor = Color.FromArgb(145, 55, 165),
                IconSize = 36,
                Location = new Point(40, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(iconBaslik);

            Label lblBaslik = new Label()
            {
                Text = "Kullanıcı Yönetimi",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                Location = new Point(85, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblBaslik);

            TableLayoutPanel tlpAna = new TableLayoutPanel() { Location = new Point(40, 80), Width = 1000, Height = 600, ColumnCount = 2, RowCount = 1 };
            tlpAna.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tlpAna.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            this.Controls.Add(tlpAna);

            Panel pnlSol = new Panel() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(22, 22, 26), Margin = new Padding(0, 0, 15, 0), Padding = new Padding(15) };
            pnlSol.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 450, 600, 15, 15));
            tlpAna.Controls.Add(pnlSol, 0, 0);

            Label lblSolBaslik = new Label() { Text = "👥 Sistemdeki Üyeler", ForeColor = Color.FromArgb(160, 160, 170), Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(0, 0, 0, 10) };
            pnlSol.Controls.Add(lblSolBaslik);

            dgvKullanicilar = new DataGridView() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            AdminPanelUC.TabloyuD4TemasinaCevir(dgvKullanicilar);
            pnlSol.Controls.Add(dgvKullanicilar);
            dgvKullanicilar.BringToFront();

            Panel pnlSag = new Panel() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(22, 22, 26), Margin = new Padding(15, 0, 0, 0), Padding = new Padding(15) };
            pnlSag.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 550, 600, 15, 15));
            tlpAna.Controls.Add(pnlSag, 1, 0);

            lblSeciliKullanici = new Label() { Text = "🎟️ Seçili Kullanıcının Biletleri", ForeColor = Color.FromArgb(160, 160, 170), Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(0, 0, 0, 10) };
            pnlSag.Controls.Add(lblSeciliKullanici);

            dgvBiletler = new DataGridView() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            AdminPanelUC.TabloyuD4TemasinaCevir(dgvBiletler);
            pnlSag.Controls.Add(dgvBiletler);
            dgvBiletler.BringToFront();

            btnSil = new Button() { Text = "🗑️ KULLANICIYI SİSTEMDEN SİL", BackColor = Color.Crimson, ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Bottom, Height = 50, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Enabled = false };
            btnSil.FlatAppearance.BorderSize = 0;
            btnSil.Click += BtnSil_Click;
            pnlSag.Controls.Add(btnSil);

            dgvKullanicilar.SelectionChanged += DgvKullanicilar_SelectionChanged;

            this.Resize += (s, e) => {
                tlpAna.Width = this.Width - 80;
                tlpAna.Height = this.Height - 120;
                if (pnlSol.Width > 0 && pnlSol.Height > 0) pnlSol.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlSol.Width, pnlSol.Height, 15, 15));
                if (pnlSag.Width > 0 && pnlSag.Height > 0) pnlSag.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlSag.Width, pnlSag.Height, 15, 15));
            };
        }

        private void KullanicilariYukle()
        {
            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                string sorgu = "SELECT ID, (Ad || ' ' || Soyad) AS [Ad Soyad], Eposta AS [E-Posta Adresi] FROM Kullanicilar WHERE Rol != 'Admin'";

                using (SQLiteDataAdapter da = new SQLiteDataAdapter(sorgu, baglan))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvKullanicilar.DataSource = dt;
                    if (dgvKullanicilar.Columns["ID"] != null) dgvKullanicilar.Columns["ID"].Visible = false;
                }
            }
            dgvKullanicilar.ClearSelection();
            dgvBiletler.DataSource = null;
            if (btnSil != null) btnSil.Enabled = false;
            secilenKullaniciID = 0;
            if (lblSeciliKullanici != null) lblSeciliKullanici.Text = "🎟️ Seçili Kullanıcının Biletleri";
        }

        private void DgvKullanicilar_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvKullanicilar.SelectedRows.Count > 0)
            {
                if (dgvKullanicilar.SelectedRows[0].Cells["ID"].Value == null || dgvKullanicilar.SelectedRows[0].Cells["ID"].Value == DBNull.Value) return;

                secilenKullaniciID = Convert.ToInt32(dgvKullanicilar.SelectedRows[0].Cells["ID"].Value);
                string seciliAd = dgvKullanicilar.SelectedRows[0].Cells["Ad Soyad"].Value?.ToString() ?? "";

                if (lblSeciliKullanici != null) lblSeciliKullanici.Text = $"🎟️ {seciliAd} Biletleri";
                if (btnSil != null) btnSil.Enabled = true;

                BiletleriYukle(secilenKullaniciID);
            }
        }

        private void BiletleriYukle(int kullaniciID)
        {
            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                string sorgu = @"SELECT F.FilmAdi AS [Film Adı], S.SubeAdi AS [Sinema], B.KoltukNo AS [Koltuk], B.Tarih AS [Tarih] 
                                 FROM Biletler B 
                                 INNER JOIN Filmler F ON B.FilmID = F.ID 
                                 INNER JOIN Sinemalar S ON B.SinemaID = S.ID 
                                 WHERE B.KullaniciID = @id";
                using (SQLiteDataAdapter da = new SQLiteDataAdapter(sorgu, baglan))
                {
                    da.SelectCommand.Parameters.AddWithValue("@id", kullaniciID);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvBiletler.DataSource = dt;
                }
            }
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (secilenKullaniciID == 0) return;
            string seciliAd = dgvKullanicilar.SelectedRows[0].Cells["Ad Soyad"].Value.ToString();

            if (MessageBox.Show($"'{seciliAd}' adlı kullanıcıyı ve satın aldığı tüm biletleri silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                SqlBaglantisi bgl = new SqlBaglantisi();
                using (SQLiteConnection baglan = bgl.Baglanti())
                {
                    using (SQLiteCommand kmtBilet = new SQLiteCommand("DELETE FROM Biletler WHERE KullaniciID = @id", baglan))
                    {
                        kmtBilet.Parameters.AddWithValue("@id", secilenKullaniciID);
                        kmtBilet.ExecuteNonQuery();
                    }
                    using (SQLiteCommand kmtKullanici = new SQLiteCommand("DELETE FROM Kullanicilar WHERE ID = @id", baglan))
                    {
                        kmtKullanici.Parameters.AddWithValue("@id", secilenKullaniciID);
                        kmtKullanici.ExecuteNonQuery();
                    }
                }
                KullanicilariYukle();
            }
        }
    }
}