using System;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace BetterNotes {
    public static class Archive {
        public static void ArchiveFile(string folder, string savePath)
        {
            File.Delete(savePath);
            ZipFile.CreateFromDirectory(folder, savePath);
        }

        public static void UnarchiveFile(string fullPath, string archivePath, string extractPath)
        {
            if (Directory.Exists(extractPath)) if (MessageBox.Show("There is an active note of this name, overwrite?", "Delete Active Note?", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes) Directory.Delete(fullPath, true);
            ZipFile.ExtractToDirectory(archivePath, extractPath);
        }     
    }
}