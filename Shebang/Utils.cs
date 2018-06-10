using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using static Shebang.Models;

namespace Shebang
{
    public class Utils
    {
        public static void Update()
        {
        }

        public static Version GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        public static void WriteColoredText(string text, ConsoleColor color, ConsoleColor? bgColor, bool sameLine = false)
        {
            var oldColor = Console.ForegroundColor;
            var oldBackgroundColor = Console.BackgroundColor;

            Console.ForegroundColor = color;                
            if (bgColor.HasValue) Console.BackgroundColor = bgColor.Value;

            if (sameLine)
                Console.Write(text);
            else
                Console.WriteLine(text);

            Console.ForegroundColor = oldColor;
            Console.BackgroundColor = oldBackgroundColor;
        }

        public static void WriteColoredText(List<ColoredString> coloredStrings)
        {
            foreach (var item in coloredStrings)
            {
                WriteColoredText(item.Text, item.ForegroundColor, item.BackgroundColor, true);
            }

            Console.WriteLine();
        }

        public static void PrintSplash()
        {
            // Get the application version
            var version = GetVersion();

            // TO-DO: Print splash screen with colors and ASCII art
            WriteColoredText(new List<ColoredString>()
            {
                new ColoredString()
                {
                    Text = "Shebang (dotNET Core)",
                    ForegroundColor = ConsoleColor.Cyan
                },

                new ColoredString()
                {
                    Text = " - Early beta - ",
                    ForegroundColor = ConsoleColor.Yellow
                },

                new ColoredString()
                {
                    Text = $"v{version.Major}.{version.Minor}.{version.Build}",
                    ForegroundColor = ConsoleColor.Green
                }
            });

            Console.WriteLine();
        }

        public static void ForceDeleteDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return;
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

        public static void CrossPlatformAction(Action windowsAction, Action unixAction)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    // Windows-based systems
                    windowsAction.Invoke();
                    break;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    // Unix-based systems
                    unixAction.Invoke();
                    break;
                default:
                    break;
            }
        }

        public static void RunShellCommand(string command, bool showOutput = true)
        {
            Process process = new Process();
            CrossPlatformAction(
                () =>
                {
                    if (string.IsNullOrWhiteSpace(command)) return;
                    process.StartInfo = new ProcessStartInfo()
                    {
                        FileName = "powershell",
                        Arguments = "-Command \"" + command + "\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };
                },
                
                () =>
                {
                    if (string.IsNullOrWhiteSpace(command)) return;
                    process.StartInfo = new ProcessStartInfo()
                    {
                        FileName = "bash",
                        Arguments = "-c \"" + command + "\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };
                });

            process.OutputDataReceived += (o, ev) =>
            {
                Console.WriteLine(ev.Data);
            };

            process.Start();                
            if (showOutput) process.BeginOutputReadLine();

            process.WaitForExit();
        }
    }
}
