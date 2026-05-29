using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Data;

namespace D4Cinema
{
    public partial class FilmYonetimUC : UserControl
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private DataGridView dgvFilmler;
        private TextBox txtAd, txtTur, txtSure, txtYonetmen;
        private RichTextBox rtxtKonu;
        private DateTimePicker dtpVizyon;
        private ComboBox cmbDurum;
        private PictureBox pbAfis;

        private string secilenAfisYolu = "";
        private int secilenFilmID = 0;

        public FilmYonetimUC()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(18, 18, 22);

            ArayuzuCiz();
            FilmleriListele();
        }

        private void ArayuzuCiz()
        {
            TableLayoutPanel anaTablo = new TableLayoutPanel();
            anaTablo.Dock = DockStyle.Fill;
            anaTablo.ColumnCount = 2;
            anaTablo.RowCount = 1;
            anaTablo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380));
            anaTablo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            this.Controls.Add(anaTablo);

            FlowLayoutPanel pnlForm = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(22, 22, 26),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20, 20, 20, 20),
                Margin = new Padding(0)
            };
            anaTablo.Controls.Add(pnlForm, 0, 0);

            Label lblBaslik = new Label() { Text = "🎬 Film Detayları", ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Margin = new Padding(10, 0, 10, 20) };
            pnlForm.Controls.Add(lblBaslik);

            txtAd = AddInput(pnlForm, "Film Adı");
            txtTur = AddInput(pnlForm, "Türü (Aksiyon, Dram vs.)");
            txtSure = AddInput(pnlForm, "Süresi (120 dk)");
            txtYonetmen = AddInput(pnlForm, "Yönetmen");

            Label lblTarih = new Label() { Text = "Vizyon Tarihi", ForeColor = Color.FromArgb(160, 160, 170), AutoSize = true, Margin = new Padding(10, 5, 0, 0) };
            dtpVizyon = new DateTimePicker() { Width = 310, Format = DateTimePickerFormat.Short, Margin = new Padding(10, 0, 0, 15) };
            pnlForm.Controls.Add(lblTarih); pnlForm.Controls.Add(dtpVizyon);

            Label lblDurum = new Label() { Text = "Durum", ForeColor = Color.FromArgb(160, 160, 170), AutoSize = true, Margin = new Padding(10, 5, 0, 0) };
            cmbDurum = new ComboBox() { Width = 310, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(28, 28, 34), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11), Margin = new Padding(10, 0, 0, 15) };
            cmbDurum.Items.AddRange(new string[] { "Vizyonda", "Yakinda" });
            cmbDurum.SelectedIndex = 0;
            pnlForm.Controls.Add(lblDurum); pnlForm.Controls.Add(cmbDurum);

            Label lblKonu = new Label() { Text = "Film Özeti", ForeColor = Color.FromArgb(160, 160, 170), AutoSize = true, Margin = new Padding(10, 5, 0, 0) };
            rtxtKonu = new RichTextBox() { Width = 310, Height = 80, BackColor = Color.FromArgb(28, 28, 34), ForeColor = Color.White, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 11), Margin = new Padding(10, 0, 0, 15) };
            pnlForm.Controls.Add(lblKonu); pnlForm.Controls.Add(rtxtKonu);

            pbAfis = new PictureBox() { Size = new Size(110, 150), BackColor = Color.FromArgb(28, 28, 34), SizeMode = PictureBoxSizeMode.Zoom, Cursor = Cursors.Hand, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(10) };
            pbAfis.Click += PbAfis_Click;
            pnlForm.Controls.Add(pbAfis);

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

            Panel pnlTabloKutusu = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 22),
                Padding = new Padding(10, 20, 20, 20),
                Margin = new Padding(0)
            };
            anaTablo.Controls.Add(pnlTabloKutusu, 1, 0);

            dgvFilmler = new DataGridView()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            AdminPanelUC.TabloyuD4TemasinaCevir(dgvFilmler);
            dgvFilmler.CellClick += DgvFilmler_CellClick;
            pnlTabloKutusu.Controls.Add(dgvFilmler);
        }

        private TextBox AddInput(FlowLayoutPanel parent, string labelText)
        {
            Label lbl = new Label() { Text = labelText, ForeColor = Color.FromArgb(160, 160, 170), AutoSize = true, Margin = new Padding(10, 5, 0, 0) };
            TextBox txt = new TextBox() { Width = 310, BackColor = Color.FromArgb(28, 28, 34), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 11), Margin = new Padding(10, 0, 0, 15) };
            parent.Controls.Add(lbl); parent.Controls.Add(txt);
            return txt;
        }

        private Button ButonOlustur(string text, Color bgColor, int x, int y, int width)
        {
            Button btn = new Button() { Text = text, BackColor = bgColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(x, y), Size = new Size(width, 40), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 8, 8));
            return btn;
        }

        private void PbAfis_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pbAfis.Image = Image.FromFile(ofd.FileName);
                string hedefKlasor = Path.Combine(Application.StartupPath, "Afisler");
                if (!Directory.Exists(hedefKlasor)) Directory.CreateDirectory(hedefKlasor);

                string benzersizAd = Guid.NewGuid().ToString() + Path.GetExtension(ofd.FileName);
                string hedefYol = Path.Combine(hedefKlasor, benzersizAd);
                File.Copy(ofd.FileName, hedefYol);
                secilenAfisYolu = benzersizAd;
            }
        }

        private void FilmleriListele()
        {
            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT * FROM Filmler", baglan);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvFilmler.DataSource = dt;

                if (dgvFilmler.Columns["Konu"] != null) dgvFilmler.Columns["Konu"].Visible = false;
                if (dgvFilmler.Columns["Yonetmen"] != null) dgvFilmler.Columns["Yonetmen"].Visible = false;
                if (dgvFilmler.Columns["VizyonTarihi"] != null) dgvFilmler.Columns["VizyonTarihi"].Visible = false;
                if (dgvFilmler.Columns["AfisYolu"] != null) dgvFilmler.Columns["AfisYolu"].Visible = false;

                if (dgvFilmler.Columns["ID"] != null) { dgvFilmler.Columns["ID"].HeaderText = "ID"; dgvFilmler.Columns["ID"].FillWeight = 15; }
                if (dgvFilmler.Columns["FilmAdi"] != null) { dgvFilmler.Columns["FilmAdi"].HeaderText = "Film Adı"; dgvFilmler.Columns["FilmAdi"].FillWeight = 40; }
                if (dgvFilmler.Columns["Tur"] != null) { dgvFilmler.Columns["Tur"].HeaderText = "Tür"; dgvFilmler.Columns["Tur"].FillWeight = 25; }
                if (dgvFilmler.Columns["Sure"] != null) { dgvFilmler.Columns["Sure"].HeaderText = "Süre"; dgvFilmler.Columns["Sure"].FillWeight = 15; }
                if (dgvFilmler.Columns["Durum"] != null) { dgvFilmler.Columns["Durum"].HeaderText = "Durum"; dgvFilmler.Columns["Durum"].FillWeight = 20; }

                dgvFilmler.ClearSelection();
            }
        }

        private void DgvFilmler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvFilmler.Rows[e.RowIndex];
            if (row == null || row.Cells[0].Value == null) return;

            try
            {
                secilenFilmID = Convert.ToInt32(row.Cells[0].Value);

                txtAd.Text = row.Cells["FilmAdi"].Value?.ToString() ?? "";
                txtTur.Text = row.Cells["Tur"].Value?.ToString() ?? "";
                txtSure.Text = row.Cells["Sure"].Value?.ToString() ?? "";
                txtYonetmen.Text = row.Cells["Yonetmen"].Value?.ToString() ?? "";
                rtxtKonu.Text = row.Cells["Konu"].Value?.ToString() ?? "";

                string dbDurum = row.Cells["Durum"].Value?.ToString() ?? "";
                if (dbDurum == "Yakında") dbDurum = "Yakinda";
                cmbDurum.Text = dbDurum;

                string tarihStr = row.Cells["VizyonTarihi"].Value?.ToString();
                if (!string.IsNullOrEmpty(tarihStr))
                    try { dtpVizyon.Value = Convert.ToDateTime(tarihStr); } catch { }

                secilenAfisYolu = row.Cells["AfisYolu"].Value?.ToString() ?? "";
                if (!string.IsNullOrEmpty(secilenAfisYolu))
                {
                    string tamYol = Path.Combine(Application.StartupPath, "Afisler", secilenAfisYolu);
                    if (File.Exists(tamYol)) pbAfis.Image = Image.FromFile(tamYol);
                    else pbAfis.Image = null;
                }
                else pbAfis.Image = null;
            }
            catch (Exception ex) { MessageBox.Show("Veri yüklenirken hata: " + ex.Message); }
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAd.Text)) { MessageBox.Show("Film adı zorunludur!"); return; }

            string kaydedilecekDurum = cmbDurum.Text;
            if (kaydedilecekDurum == "Yakında") kaydedilecekDurum = "Yakinda";

            SqlBaglantisi bgl = new SqlBaglantisi();
            using (SQLiteConnection baglan = bgl.Baglanti())
            {
                string sorgu = secilenFilmID == 0
                    ? "INSERT INTO Filmler (FilmAdi, Tur, Sure, Konu, Durum, Yonetmen, VizyonTarihi, AfisYolu) VALUES (@ad, @tur, @sure, @konu, @durum, @yonetmen, @tarih, @afis)"
                    : "UPDATE Filmler SET FilmAdi=@ad, Tur=@tur, Sure=@sure, Konu=@konu, Durum=@durum, Yonetmen=@yonetmen, VizyonTarihi=@tarih, AfisYolu=@afis WHERE ID=@id";

                using (SQLiteCommand komut = new SQLiteCommand(sorgu, baglan))
                {
                    komut.Parameters.AddWithValue("@ad", txtAd.Text);
                    komut.Parameters.AddWithValue("@tur", txtTur.Text);
                    komut.Parameters.AddWithValue("@sure", txtSure.Text);
                    komut.Parameters.AddWithValue("@konu", rtxtKonu.Text);
                    komut.Parameters.AddWithValue("@durum", kaydedilecekDurum);
                    komut.Parameters.AddWithValue("@yonetmen", txtYonetmen.Text);
                    komut.Parameters.AddWithValue("@tarih", dtpVizyon.Value.ToShortDateString());
                    komut.Parameters.AddWithValue("@afis", secilenAfisYolu);
                    if (secilenFilmID != 0) komut.Parameters.AddWithValue("@id", secilenFilmID);

                    komut.ExecuteNonQuery();
                }
            }
            FormuTemizle();
            FilmleriListele();
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (secilenFilmID != 0 && MessageBox.Show("Bu filmi kalıcı olarak silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                SqlBaglantisi bgl = new SqlBaglantisi();
                using (SQLiteConnection baglan = bgl.Baglanti())
                {
                    using (SQLiteCommand komut = new SQLiteCommand("DELETE FROM Filmler WHERE ID=@id", baglan))
                    {
                        komut.Parameters.AddWithValue("@id", secilenFilmID);
                        komut.ExecuteNonQuery();
                    }
                }
                FormuTemizle();
                FilmleriListele();
            }
        }

        private void FormuTemizle()
        {
            secilenFilmID = 0;
            txtAd.Clear(); txtTur.Clear(); txtSure.Clear(); txtYonetmen.Clear(); rtxtKonu.Clear();
            cmbDurum.SelectedIndex = 0; dtpVizyon.Value = DateTime.Now;
            pbAfis.Image = null; secilenAfisYolu = "";
            dgvFilmler.ClearSelection();
        }
    }
}