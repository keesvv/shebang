using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shebang
{
    public class Utils
    {
        public static void Update()
        {
        }

        public static void ForceDeleteDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            NormalizeAttributes(directoryPath);
            Directory.Delete(directoryPath, true);
        }

        private static void NormalizeAttributes(string directoryPath)
        {
            string[] files = Directory.GetFiles(directoryPath);
            string[] subDirectoryFiles = Directory.GetDirectories(directoryPath);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            foreach (string file in subDirectoryFiles)
            {
                NormalizeAttributes(file);
            }

            File.SetAttributes(directoryPath, FileAttributes.Normal);
        }
    }
}
