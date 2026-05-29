using System.Windows.Forms;

namespace D4Cinema
{
    public static class MevcutKullanici
    {
        public static int ID = 0;
        public static string AdSoyad = "Ziyaretçi";
        public static string Rol = "Ziyaretçi";

       
        public static UserControl BekleyenSayfa = null;

        public static bool GirisYapildiMi()
        {
            return ID > 0;
        }

        public static void CikisYap()
        {
            ID = 0;
            AdSoyad = "Ziyaretçi";
            Rol = "Ziyaretçi";
            BekleyenSayfa = null;
        }
    }
}