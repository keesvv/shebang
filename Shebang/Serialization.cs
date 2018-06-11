using System;
using Newtonsoft.Json;
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

            /// <summary>
            /// Get the JSON string for the package.
            /// </summary>
            /// <returns>JSON string</returns>
            public override string ToString()
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
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

                Utils.CrossPlatformAction(
                    // Windows-based systems
                    () =>
                    {
                        Utils.RunShellCommand(command.WindowsCommand);
                    },

                    // Unix-based systems
                    () =>
                    {
                        Utils.RunShellCommand(command.UnixCommand);
                    });
            }
        }
    }
}
