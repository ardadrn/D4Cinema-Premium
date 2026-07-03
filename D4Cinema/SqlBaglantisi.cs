using System;
using System.Data.SQLite;
using System.IO;

namespace D4Cinema
{
    public class SqlBaglantisi
    {
        private readonly string baglantiCumlesi;

        public SqlBaglantisi()
        {
            AppPaths.Initialize();
            baglantiCumlesi = $"Data Source={AppPaths.DatabasePath};Version=3;";
        }

        public SQLiteConnection Baglanti()
        {
            SQLiteConnection baglan = new SQLiteConnection(baglantiCumlesi);
            baglan.Open();
            return baglan;
        }

        public void VeritabaniniKur()
        {
            bool yeniKurulum = false;

            
            if (!File.Exists(AppPaths.DatabasePath))
            {
                SQLiteConnection.CreateFile(AppPaths.DatabasePath);
                yeniKurulum = true;
            }

            using (SQLiteConnection baglan = Baglanti())
            {
                
                bool eskiTabloVar = false;

                if (!yeniKurulum)
                {
                    
                    using (SQLiteCommand kontrolCmd = new SQLiteCommand("PRAGMA table_info(Kullanicilar);", baglan))
                    {
                        using (SQLiteDataReader oku = kontrolCmd.ExecuteReader())
                        {
                            while (oku.Read())
                            {
                                if (oku["name"].ToString() == "AdSoyad")
                                {
                                    eskiTabloVar = true;
                                    break;
                                }
                            }
                        }
                    }

                   
                    if (eskiTabloVar)
                    {
                        using (SQLiteCommand dropCmd = new SQLiteCommand("PRAGMA foreign_keys = OFF; DROP TABLE Kullanicilar; PRAGMA foreign_keys = ON;", baglan))
                        {
                            dropCmd.ExecuteNonQuery();
                        }
                    }
                }

               
                string sqlTablolar = @"
                        CREATE TABLE IF NOT EXISTS Kullanicilar (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Ad TEXT NOT NULL,
                            Soyad TEXT NOT NULL,
                            Eposta TEXT UNIQUE NOT NULL,
                            Sifre TEXT NOT NULL,
                            Rol TEXT DEFAULT 'Uye' 
                        );

                        CREATE TABLE IF NOT EXISTS Filmler (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            FilmAdi TEXT NOT NULL,
                            Tur TEXT,
                            Sure TEXT,
                            Konu TEXT,
                            Durum TEXT,
                            Yonetmen TEXT,
                            VizyonTarihi TEXT,
                            AfisYolu TEXT
                        );

                        CREATE TABLE IF NOT EXISTS Sinemalar (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            SubeAdi TEXT NOT NULL,
                            Sehir TEXT NOT NULL,
                            Durum TEXT DEFAULT 'Aktif'
                        );

                        CREATE TABLE IF NOT EXISTS Kampanyalar (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Baslik TEXT NOT NULL,
                            Detay TEXT,
                            Populer INTEGER 
                        );

                        CREATE TABLE IF NOT EXISTS Biletler (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            KullaniciID INTEGER,
                            FilmID INTEGER,
                            SinemaID INTEGER,
                            KoltukNo TEXT,
                            Tarih TEXT,
                            FOREIGN KEY(KullaniciID) REFERENCES Kullanicilar(ID),
                            FOREIGN KEY(FilmID) REFERENCES Filmler(ID),
                            FOREIGN KEY(SinemaID) REFERENCES Sinemalar(ID)
                        );
                        
                        CREATE TABLE IF NOT EXISTS Seanslar (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            FilmID INTEGER,
                            SinemaID INTEGER,
                            Tarih TEXT,
                            Saat TEXT
                        );";

                using (SQLiteCommand komut = new SQLiteCommand(sqlTablolar, baglan))
                {
                    komut.ExecuteNonQuery();
                }

                
                if (yeniKurulum)
                {
                    
                    string sqlVeriler = @"
                        INSERT INTO Kullanicilar (Ad, Soyad, Eposta, Sifre, Rol) VALUES 
                        ('Sistem', 'Yöneticisi', 'admin@d4cinema.com', 'admin123', 'Admin'),
                        ('Ahmet', 'Yılmaz', 'ahmet@gmail.com', '123456', 'Uye');

                        INSERT INTO Filmler (FilmAdi, Tur, Sure, Konu, Durum, Yonetmen, VizyonTarihi, AfisYolu) VALUES 
                        ('Cehennem Mutfağı', 'Aksiyon, Suç', '115 dk', 'Gündüzleri adalet arayan bir avukatın, geceleri maskeli bir kahramana dönüşme öyküsü.', 'Vizyonda', 'Matt Murdock', '10.04.2026', ''),
                        ('Kızıltaş''ın Sırrı', 'Gizem, Bilim Kurgu', '150 dk', 'Yeraltının derinliklerinde bulunan gizemli bir madenin hikayesi.', 'Yakinda', 'Steve Block', '20.08.2026', '');

                        INSERT INTO Sinemalar (SubeAdi, Sehir, Durum) VALUES 
                        ('D4Cinema Kızılay Sahnesi', 'Ankara', 'Aktif'),
                        ('D4Cinema 17 Burda Kordon', 'Çanakkale', 'Aktif'),
                        ('D4Cinema Valhalla Metal Pub', 'Çanakkale', 'Pasif');

                        INSERT INTO Kampanyalar (Baslik, Detay, Populer) VALUES 
                        ('Öğrenciye %50 İndirim', 'Hafta içi tüm seanslarda geçerli dev fırsat.', 1),
                        ('Metal Pub Öncesi Bilet', 'Valhalla''da özel paketle eğlenceye doy.', 0);
                    ";

                    using (SQLiteCommand komutVeri = new SQLiteCommand(sqlVeriler, baglan))
                    {
                        komutVeri.ExecuteNonQuery();
                    }
                }
                else if (eskiTabloVar)
                {
                    string sqlKurtarma = "INSERT INTO Kullanicilar (Ad, Soyad, Eposta, Sifre, Rol) VALUES ('Sistem', 'Yöneticisi', 'admin@d4cinema.com', 'admin123', 'Admin');";
                    using (SQLiteCommand kurtarmaKomut = new SQLiteCommand(sqlKurtarma, baglan))
                    {
                        kurtarmaKomut.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}