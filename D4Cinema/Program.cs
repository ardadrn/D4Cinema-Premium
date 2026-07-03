using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D4Cinema
{
    internal static class Program
    {
       
        [STAThread]
        static void Main()
        {
            try
            {
                AppPaths.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Uygulama verileri hazırlanamadı:\n" + ex.Message,
                    "D4Cinema Başlatma Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SplashForm());
        }
    }
}
