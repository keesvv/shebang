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

        public class PackageCommands
        {
            

            [JsonProperty("before_install")]
            public string BeforeInstall { get; set; }

            [JsonProperty("after_install")]
            public string AfterInstall { get; set; }

            public void Execute(ScriptType type)
            {
                string command = string.Empty;
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

                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        // TO-DO: Add support for Windows
                        Log("Apparently, you seem to use Windows. Shebang currently doesn't support Windows systems.", LogType.WARN);
                        Log("If you'd like, you could still use Shebang with WSL (Linux Subsystem for Windows).", LogType.WARN);
                        break;
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        new Process()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = "bash",
                                Arguments = "-c \"" + command + "\"",
                                UseShellExecute = true,
                                RedirectStandardOutput = true
                            }
                        }.Start();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
