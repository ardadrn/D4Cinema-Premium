using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace D4Cinema
{
    public partial class SplashForm : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private Timer timerAnimasyon;
        private Timer timerGecis;
        private int donmeAcisi = 0;
        private int beklemeSuresi = 0;
        private bool kapaniyor = false;

        public SplashForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(600, 350);
            this.BackColor = Color.FromArgb(18, 18, 22);
            this.Opacity = 0; 
            this.DoubleBuffered = true;
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 20, 20));

            ArayuzuCiz();

            
            timerAnimasyon = new Timer() { Interval = 16 };
            timerAnimasyon.Tick += Animasyon_Tick;
            timerAnimasyon.Start();

            
            timerGecis = new Timer() { Interval = 50 };
            timerGecis.Tick += Gecis_Tick;
            timerGecis.Start();
        }

        private void ArayuzuCiz()
        {
            PictureBox pbLogo = new PictureBox()
            {
                Size = new Size(220, 100),
                Location = new Point((this.Width - 220) / 2, 70),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            
            string logoYolu = Path.Combine(Application.StartupPath, "logo.png");
            if (File.Exists(logoYolu)) pbLogo.Image = Image.FromFile(logoYolu);
            else
            {
                Label lblYedekLogo = new Label() { Text = "D4 CINEMA", ForeColor = Color.White, Font = new Font("Segoe UI", 28, FontStyle.Bold), AutoSize = true, BackColor = Color.Transparent };
                lblYedekLogo.Location = new Point((this.Width - 230) / 2, 90);
                this.Controls.Add(lblYedekLogo);
            }
            this.Controls.Add(pbLogo);

            Label lblBilgi = new Label()
            {
                Text = "Sistem Başlatılıyor, Lütfen Bekleyiniz...",
                ForeColor = Color.FromArgb(160, 160, 170),
                Font = new Font("Segoe UI", 11, FontStyle.Italic),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            lblBilgi.Location = new Point((this.Width - 250) / 2, 280);
            this.Controls.Add(lblBilgi);
        }

        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; 

            int x = (this.Width - 50) / 2;
            int y = 200;

           
            using (Pen pArka = new Pen(Color.FromArgb(40, 40, 45), 5))
            {
                e.Graphics.DrawEllipse(pArka, x, y, 50, 50);
            }

            
            using (Pen pOn = new Pen(Color.FromArgb(145, 55, 165), 5))
            {
                pOn.StartCap = LineCap.Round; 
                pOn.EndCap = LineCap.Round;
                e.Graphics.DrawArc(pOn, x, y, 50, 50, donmeAcisi, 120); 
            }
        }

        private void Animasyon_Tick(object sender, EventArgs e)
        {
            donmeAcisi = (donmeAcisi + 12) % 360; 
            this.Invalidate(); 
        }

        private void Gecis_Tick(object sender, EventArgs e)
        {
            if (!kapaniyor)
            {
                if (this.Opacity < 1) this.Opacity += 0.05; 
                else
                {
                    beklemeSuresi += timerGecis.Interval;
                    if (beklemeSuresi >= 2500) kapaniyor = true; 
                }
            }
            else
            {
                if (this.Opacity > 0) this.Opacity -= 0.05; 
                else
                {
                    timerGecis.Stop();
                    timerAnimasyon.Stop();
                    this.Hide();

                    
                    Form1 anaSayfa = new Form1();
                    anaSayfa.ShowDialog();

                    this.Close(); 
                }
            }
        }
    }
}