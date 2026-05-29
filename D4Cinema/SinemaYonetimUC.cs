using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Data;

namespace D4Cinema
{
    public partial class SinemaYonetimUC : UserControl
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private DataGridView dgvSinemalar;
        private TextBox txtSubeAdi;
        private ComboBox cmbSehir, cmbDurum;

        private int secilenSinemaID = 0;

        private string[] iller = { "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Aksaray", "Amasya", "Ankara", "Antalya", "Ardahan", "Artvin", "Aydın", "Balıkesir", "Bartın", "Batman", "Bayburt", "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur", "Bursa", "Çanakkale", "Çankırı", "Çorum", "Denizli", "Diyarbakır", "Düzce", "Edirne", "Elazığ", "Erzincan", "Erzurum", "Eskişehir", "Gaziantep", "Giresun", "Gümüşhane", "Hakkâri", "Hatay", "Iğdır", "Isparta", "İstanbul", "İzmir", "Kahramanmaraş", "Karabük", "Karaman", "Kars", "Kastamonu", "Kayseri", "Kırıkkale", "Kırklareli", "Kırşehir", "Kilis", "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa", "Mardin", "Mersin", "Muğla", "Muş", "Nevşehir", "Niğde", "Ordu", "Osmaniye", "Rize", "Sakarya", "Samsun", "Siirt", "Sinop", "Sivas", "Şanlıurfa", "Şırnak", "Tekirdağ", "Tokat", "Trabzon", "Tunceli", "Uşak", "Van", "Yalova", "Yozgat", "Zonguldak" };

        public SinemaYonetimUC()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(18, 18, 22);

            ArayuzuCiz();
            SinemalariListele();
        }

        private void ArayuzuCiz()
        {
            TableLayoutPanel anaTablo = new TableLayoutPanel() { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            anaTablo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380));
            anaTablo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            this.Controls.Add(anaTablo);

            FlowLayoutPanel pnlForm = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(22, 22, 26),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20),
                Margin = new Padding(0)
            };
            anaTablo.Controls.Add(pnlForm, 0, 0);

            pnlForm.Controls.Add(new Label() { Text = "🍿 Sinema Salonu Yönetimi", ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Margin = new Padding(10, 0, 10, 30) });

            pnlForm.Controls.Add(new Label() { Text = "Şube Adı (D4Cinema otomatik eklenir)", ForeColor = Color.FromArgb(160, 160, 170), AutoSize = true, Margin = new Padding(10, 5, 0, 0) });
            txtSubeAdi = new TextBox() { Width = 310, BackColor = Color.FromArgb(28, 28, 34), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 11), Margin = new Padding(10, 5, 0, 20) };
            pnlForm.Controls.Add(txtSubeAdi);

            pnlForm.Controls.Add(new Label() { Text = "Şehir", ForeColor = Color.FromArgb(160, 160, 170), AutoSize = true, Margin = new Padding(10, 5, 0, 0) });
            cmbSehir = new ComboBox() { Width = 310, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(28, 28, 34), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11), Margin = new Padding(10, 5, 0, 20) };
            cmbSehir.Items.AddRange(iller);
            pnlForm.Controls.Add(cmbSehir);

            pnlForm.Controls.Add(new Label() { Text = "Salon Durumu", ForeColor = Color.FromArgb(160, 160, 170), AutoSize = true, Margin = new Padding(10, 5, 0, 0) });
            cmbDurum = new ComboBox() { Width = 310, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(28, 28, 34), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11), Margin = new Padding(10, 5, 0, 30) };
            cmbDurum.Items.AddRange(new string[] { "Aktif", "Pasif" });
            cmbDurum.SelectedIndex = 0;
            pnlForm.Controls.Add(cmbDurum);

            Panel pnlButonlar = new Panel() { Width = 320, Height = 110, Margin = new Padding(10, 10, 0, 20) };

            Button btnKaydet = ButonOlustur("Kaydet / Güncelle", Color.FromArgb(120, 40, 140), 0, 0, 310);
            btnKaydet.Click += BtnKaydet_Click;
            pnlButonlar.Controls.Add(btnKaydet);

            Button btnSil = ButonOlustur("Sil", Color.Crimson, 0, 50, 145);
            btnSil.Click += BtnSil_Click;
            pnlButonlar.Controls.Add(btnSil);

            Button btnTemizle = ButonOlustur("Temizle", Color.FromArgb(40, 40, 45), 165, 50, 145);
            btnTemizle.Click += (s, e) => FormuTemizle();
            pnlButonlar.Controls.Add(btnTemizle);

            pnlForm.Controls.Add(pnlButonlar);

            Panel pnlTabloKutusu = new Panel() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 18, 22), Padding = new Padding(10, 20, 20, 20), Margin = new Padding(0) };
            anaTablo.Controls.Add(pnlTabloKutusu, 1, 0);

            dgvSinemalar = new DataGridView()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            AdminPanelUC.TabloyuD4TemasinaCevir(dgvSinemalar);
            dgvSinemalar.CellClick += DgvSinemalar_CellClick;
            pnlTabloKutusu.Controls.Add(dgvSinemalar);
        }

        private Button ButonOlustur(string text, Color bgColor, int x, int y, int width)
        {
            Button btn = new Button() { Text = text, BackColor = bgColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(x, y), Size = new Size(width, 40), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 8, 8));
            return btn;
        }

        private void SinemalariListele()
        {
            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT * FROM Sinemalar", baglan);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvSinemalar.DataSource = dt;

                if (dgvSinemalar.Columns["ID"] != null) { dgvSinemalar.Columns["ID"].HeaderText = "No"; dgvSinemalar.Columns["ID"].FillWeight = 15; }
                if (dgvSinemalar.Columns["SubeAdi"] != null) { dgvSinemalar.Columns["SubeAdi"].HeaderText = "Şube Adı"; dgvSinemalar.Columns["SubeAdi"].FillWeight = 50; }
                if (dgvSinemalar.Columns["Sehir"] != null) { dgvSinemalar.Columns["Sehir"].HeaderText = "Şehir"; dgvSinemalar.Columns["Sehir"].FillWeight = 20; }
                if (dgvSinemalar.Columns["Durum"] != null) { dgvSinemalar.Columns["Durum"].HeaderText = "Durum"; dgvSinemalar.Columns["Durum"].FillWeight = 15; }

                dgvSinemalar.ClearSelection();
            }
        }

        private void DgvSinemalar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvSinemalar.Rows[e.RowIndex];
            if (row == null || row.Cells[0].Value == null) return;

            secilenSinemaID = Convert.ToInt32(row.Cells[0].Value);

            string tamSubeAdi = row.Cells["SubeAdi"].Value?.ToString() ?? "";
            if (tamSubeAdi.StartsWith("D4Cinema ")) txtSubeAdi.Text = tamSubeAdi.Substring(9);
            else txtSubeAdi.Text = tamSubeAdi;

            cmbSehir.Text = row.Cells["Sehir"].Value?.ToString() ?? "";
            cmbDurum.Text = row.Cells["Durum"].Value?.ToString() ?? "";
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSubeAdi.Text) || cmbSehir.SelectedIndex == -1)
            {
                MessageBox.Show("Şube adı ve Şehir zorunludur!"); return;
            }

            string islenecekSubeAdi = txtSubeAdi.Text.Trim();
            if (!islenecekSubeAdi.StartsWith("D4Cinema", StringComparison.OrdinalIgnoreCase))
            {
                islenecekSubeAdi = "D4Cinema " + islenecekSubeAdi;
            }

            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                string sorgu = secilenSinemaID == 0
                    ? "INSERT INTO Sinemalar (SubeAdi, Sehir, Durum) VALUES (@ad, @sehir, @durum)"
                    : "UPDATE Sinemalar SET SubeAdi=@ad, Sehir=@sehir, Durum=@durum WHERE ID=@id";

                using (SQLiteCommand komut = new SQLiteCommand(sorgu, baglan))
                {
                    komut.Parameters.AddWithValue("@ad", islenecekSubeAdi);
                    komut.Parameters.AddWithValue("@sehir", cmbSehir.Text);
                    komut.Parameters.AddWithValue("@durum", cmbDurum.Text);
                    if (secilenSinemaID != 0) komut.Parameters.AddWithValue("@id", secilenSinemaID);

                    komut.ExecuteNonQuery();
                }
            }
            FormuTemizle();
            SinemalariListele();
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (secilenSinemaID != 0 && MessageBox.Show("Bu şubeyi silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                SqlBaglantisi bgl = new SqlBaglantisi();
                using (SQLiteConnection baglan = bgl.Baglanti())
                {
                    using (SQLiteCommand komut = new SQLiteCommand("DELETE FROM Sinemalar WHERE ID=@id", baglan))
                    {
                        komut.Parameters.AddWithValue("@id", secilenSinemaID);
                        komut.ExecuteNonQuery();
                    }
                }
                FormuTemizle();
                SinemalariListele();
            }
        }

        private void FormuTemizle()
        {
            secilenSinemaID = 0;
            txtSubeAdi.Clear();
            cmbSehir.SelectedIndex = -1;
            cmbDurum.SelectedIndex = 0;
            dgvSinemalar.ClearSelection();
        }
    }
}