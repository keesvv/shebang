using System;
using System.Diagnostics;
using Newtonsoft.Json;
using static Shebang.Logger;
using static Shebang.Models;

namespace Shebang
{
    public class Serialization
    {
        [Serializable()]
        public class Package
        {
            [JsonProperty("id")]
            public string ID { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("properties")]
            public PackageProperties Properties { get; set; }
        }

        public class PackageProperties
        {
            [JsonProperty("executable")]
            public string ExecutablePath { get; set; }

            [JsonProperty("symlink_name")]
            public string SymlinkName { get; set; }

            [JsonProperty("is_registered_package")]
            public bool IsRegistered { get; set; }

            [JsonProperty("commands")]
            public PackageCommands Commands { get; set; }
        }

        public class PackageCommand
        {
            [JsonProperty("windows")]
            public string WindowsCommand { get; set; }

            [JsonProperty("unix")]
            public string UnixCommand { get; set; }
        }

        public class PackageCommands
        {
            [JsonProperty("before_install")]
            public PackageCommand BeforeInstall { get; set; }

            [JsonProperty("after_install")]
            public PackageCommand AfterInstall { get; set; }

            public void Execute(ScriptType type)
            {
                PackageCommand command = null;
                switch (type)
                {
                    case ScriptType.BEFORE_INSTALL:
                        command = BeforeInstall;
                        break;
                    case ScriptType.AFTER_INSTALL:
                        command = AfterInstall;
                        break;
                    default:
                        break;
                }

                var process = new Process();
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        // Windows-based systems
                        if (string.IsNullOrWhiteSpace(command.WindowsCommand)) return;
                        process.StartInfo = new ProcessStartInfo()
                        {
                            FileName = "powershell",
                            Arguments = "-Command \"" + command.WindowsCommand + "\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true
                        };
                        break;
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        // Unix-based systems
                        if (string.IsNullOrWhiteSpace(command.UnixCommand)) return;
                        process.StartInfo = new ProcessStartInfo()
                        {
                            FileName = "bash",
                            Arguments = "-c \"" + command.UnixCommand + "\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true
                        };
                        break;
                    default:
                        break;
                }

                // Write the output data
                process.OutputDataReceived += (o, ev) =>
                {
                    Console.WriteLine(ev.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            }
        }
    }
}
