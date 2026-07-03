using System;
using System.Drawing;
using System.IO;

namespace D4Cinema
{
    public static class AppPaths
    {
        private static readonly object InitLock = new object();
        private static bool initialized;

        public static string DataFolder
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "D4Cinema");
            }
        }

        public static string AfislerFolder
        {
            get { return Path.Combine(DataFolder, "Afisler"); }
        }

        public static string KampanyalarFolder
        {
            get { return Path.Combine(DataFolder, "Kampanyalar"); }
        }

        public static string DatabasePath
        {
            get { return Path.Combine(DataFolder, "D4CinemaDB.sqlite"); }
        }

        public static string LogoPath
        {
            get { return Path.Combine(DataFolder, "logo.png"); }
        }

        public static void Initialize()
        {
            lock (InitLock)
            {
                if (initialized)
                    return;

                Directory.CreateDirectory(DataFolder);
                Directory.CreateDirectory(AfislerFolder);
                Directory.CreateDirectory(KampanyalarFolder);

                string applicationFolder = AppDomain.CurrentDomain.BaseDirectory;
                string seedFolder = Path.Combine(applicationFolder, "DataSeed");

                // Eski sürüm bin klasöründe veri bırakmışsa önce onu koru.
                CopyFirstExistingFile(
                    DatabasePath,
                    Path.Combine(applicationFolder, "D4CinemaDB.sqlite"),
                    Path.Combine(seedFolder, "D4CinemaDB.sqlite"));

                CopyFirstExistingFile(
                    LogoPath,
                    Path.Combine(applicationFolder, "logo.png"),
                    Path.Combine(seedFolder, "logo.png"));

                CopyMissingFiles(
                    Path.Combine(applicationFolder, "Afisler"),
                    AfislerFolder);

                CopyMissingFiles(
                    Path.Combine(seedFolder, "Afisler"),
                    AfislerFolder);

                CopyMissingFiles(
                    Path.Combine(applicationFolder, "Kampanyalar"),
                    KampanyalarFolder);

                CopyMissingFiles(
                    Path.Combine(seedFolder, "Kampanyalar"),
                    KampanyalarFolder);

                initialized = true;
            }
        }

        public static string GetAfisPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            return Path.Combine(AfislerFolder, Path.GetFileName(fileName));
        }

        public static string GetKampanyaPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            return Path.Combine(KampanyalarFolder, Path.GetFileName(fileName));
        }

        public static Image LoadImageWithoutLock(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return null;

            try
            {
                using (FileStream stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    using (Image image = Image.FromStream(stream))
                    {
                        return new Bitmap(image);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static void CopyFirstExistingFile(
            string destination,
            params string[] candidates)
        {
            if (File.Exists(destination))
            {
                FileInfo existingFile = new FileInfo(destination);
                if (existingFile.Length > 0)
                    return;

                File.Delete(destination);
            }

            foreach (string candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate) || !File.Exists(candidate))
                    continue;

                FileInfo candidateFile = new FileInfo(candidate);
                if (candidateFile.Length == 0)
                    continue;

                File.Copy(candidate, destination, false);
                return;
            }
        }

        private static void CopyMissingFiles(
            string sourceFolder,
            string destinationFolder)
        {
            if (!Directory.Exists(sourceFolder))
                return;

            Directory.CreateDirectory(destinationFolder);

            foreach (string sourceFile in Directory.GetFiles(
                sourceFolder,
                "*",
                SearchOption.AllDirectories))
            {
                string relativePath = sourceFile.Substring(sourceFolder.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                string destinationFile = Path.Combine(destinationFolder, relativePath);
                string destinationDirectory = Path.GetDirectoryName(destinationFile);

                if (!Directory.Exists(destinationDirectory))
                    Directory.CreateDirectory(destinationDirectory);

                if (!File.Exists(destinationFile) ||
                    new FileInfo(destinationFile).Length == 0)
                {
                    File.Copy(sourceFile, destinationFile, true);
                }
            }
        }
    }
}
