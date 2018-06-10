using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using ColorfulConsole = Colorful.Console;
using static Shebang.Models;
using static Shebang.Logger;

namespace Shebang
{
    public class Utils
    {
        public static void Update()
        {
            Log("This function has not been implemented yet. Try again later.", LogType.WARN);
        }

        public static void PrintSyntax()
        {
            string[] syntax = new string[]
            {
                "General syntax: shebang <command> [arguments ...]",
                "Install syntax: shebang install <username>/<repository> [branch]",
                "Example:",
                "    shebang install keesvv/shebang",
                "To remove a package, you can use the following command:",
                "    shebang remove package-id",
                "",
                "If you would like to create a package, type shebang create.",
                "You can also easily update Shebang by typing shebang update."
            };

            foreach (string line in syntax)
            {
                Console.WriteLine(line);
            }
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

            ColorfulConsole.WriteAscii("Shebang", Colorful.FigletFont.Load(Path.Combine("Fonts", "font.flf")));

            // TO-DO: Print splash screen with colors and ASCII art
            WriteColoredText(new List<ColoredString>()
            {
                new ColoredString()
                {
                    Text = "Shebang",
                    ForegroundColor = ConsoleColor.Cyan
                },

                new ColoredString()
                {
                    Text = " - dotNET Core beta - ",
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
