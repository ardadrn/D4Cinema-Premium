using System.Windows.Forms;

namespace D4Cinema
{
    public static class Oturum
    {
       
        public static bool GirisYapildiMi = false;

       
        public static int ID = 0;

        public static string AdSoyad = "Ziyaretçi";
        public static string Eposta = "";
        public static string Rol = "Ziyaretçi";

        
        public static UserControl BekleyenSayfa = null;

        
        public static void CikisYap()
        {
            GirisYapildiMi = false;
            ID = 0;
            AdSoyad = "Ziyaretçi";
            Eposta = "";
            Rol = "Ziyaretçi";
            BekleyenSayfa = null;
        }
    }
}