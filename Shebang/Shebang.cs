using System;
using System.Net;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using LibGit2Sharp;

using static Shebang.Serialization;
using static Shebang.Logger;
using static Shebang.Models;
using static Shebang.Utils;
using System.Collections.Generic;

namespace Shebang
{
    public class Shebang
    {
        public static Package GetPackage(string json)
        {
            Package package = JsonConvert.DeserializeObject<Package>(json);
            return package;
        }

        public static void InstallPackage(string repository, string branch = "master")
        {
            string repositoryUrl = $"https://github.com/{repository}";
            string descriptorUrl = $"https://raw.githubusercontent.com/{repository}/{branch}/shebang.json";

            Log("Getting install descriptor...");
            var installStopwatch = Stopwatch.StartNew();
            if (string.IsNullOrEmpty(repository))
            {
                Log("Please specify a repository to install.", LogType.ERROR);
                return;
            }

            WebClient webClient = new WebClient();
            try
            {
                var descriptor = webClient.DownloadString(descriptorUrl);
                Package package = GetPackage(descriptor);
                Log($"Installing '{package.Name}' (v-{package.Version}) ...");

                string packageFolder = GetPackageFolder(package.ID);
                if (Directory.Exists(packageFolder))
                {
                    Log($"'{package.ID}' already exists, reinstalling...");
                    ForceDeleteDirectory(packageFolder);
                }

                Log("Cloning repository...");
                Repository.Clone(repositoryUrl, packageFolder, new CloneOptions()
                {
                    BranchName = branch
                });

                StreamReader reader = new StreamReader(Path.Combine(packageFolder, "shebang.json"));
                var packageJson = reader.ReadToEnd();
                Package offlinePackage = GetPackage(packageJson);

                Log("Successfully cloned.");
                Log("Running preinstall script...");
                offlinePackage.Properties.Commands.Execute(ScriptType.BEFORE_INSTALL);

                Log("Setting permissions...");
                try
                {
                    var files = Directory.GetFiles(packageFolder, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        FileInfo fileInfo = new FileInfo(file)
                        {
                            Attributes = FileAttributes.Normal
                        };
                    }
                }
                catch (Exception)
                {
                    Log("Error while setting permissions; do you have administrator/superuser rights?", LogType.ERROR);
                }

                var executablePath = Path.Combine(packageFolder, package.Properties.ExecutablePath);
                CrossPlatformAction(
                    // Windows-based systems
                    () =>
                    {
                        Log("Creating shortcuts...");
                        Log("Creating shortcuts in Windows is still not a feature.", LogType.WARN);
                        Log($"You can find the main executable here: '{executablePath}'", LogType.WARN);
                    },
                    
                    // Unix-based systems
                    () =>
                    {
                        Log("Creating symlinks...");
                        RunShellCommand($"ln -s {executablePath} /usr/bin/{package.Properties.SymlinkName}", false);
                        RunShellCommand($"sudo chmod +x /usr/bin/{package.Properties.SymlinkName}", false);
                    });

                Log("Running postinstall script...");
                offlinePackage.Properties.Commands.Execute(ScriptType.AFTER_INSTALL);

                installStopwatch.Stop();
                var elapsedSeconds = Math.Round(installStopwatch.Elapsed.TotalSeconds, 2);

                Log($"'{package.ID}' has been installed successfully!");
                Log($"Installation took {elapsedSeconds} seconds.");
            }
            catch (WebException e)
            {
                HttpWebResponse response = (HttpWebResponse)e.Response;
                try
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            Log("The shebang.json file could not be found in this repository or the repository does not exist.", LogType.ERROR);
                            break;
                        case HttpStatusCode.ServiceUnavailable:
                            Log("The GitHub services are currently unavailable or you are sending too much requests.", LogType.ERROR);
                            break;
                        case HttpStatusCode.BadRequest:
                            Log("Error while installing the package; did you specify [username]/[repository]?", LogType.ERROR);
                            break;
                        default:
                            Log($"An unknown error has occured (error {(int)response.StatusCode}).", LogType.ERROR);
                            break;
                    }
                }
                catch (Exception)
                {
                    Log("The HTTP response could not be retrieved. Please make sure you have an internet connection", LogType.ERROR);
                    Log("and make sure Shebang isn't being blocked by a firewall.", LogType.ERROR);
                }
            }
            catch (Exception ex)
            {
                Log("An unknown error has occured. More details written down below:", LogType.ERROR);
                Log($"Error message: {ex.Message}", LogType.ERROR);
                Log($"Error code: {ex.HResult}", LogType.ERROR);
            }
        }

        public static void CreatePackage()
        {
            WriteColoredText(@" // Package creation menu \\ ", ConsoleColor.White, ConsoleColor.DarkCyan);
            Console.WriteLine();

            // Create a package boilerplate
            Package p = new Package()
            {
                Name = "Sample package",
                ID = "sample-package",
                Version = "1.0.0",
                Properties = new PackageProperties()
                {
                    ExecutablePath = "",
                    IsRegistered = true,
                    Commands = new PackageCommands()
                    {
                        AfterInstall = new PackageCommand()
                        {
                            WindowsCommand = "echo 'Enter your commands here'",
                            UnixCommand = "echo 'Enter your commands here'"
                        },

                        BeforeInstall = new PackageCommand()
                        {
                            WindowsCommand = "echo 'Enter your commands here'",
                            UnixCommand = "echo 'Enter your commands here'"
                        }
                    }
                }
            };

            Console.Write($"Package name: [{p.Name}] ");
            var name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name)) p.Name = name;

            Console.Write($"Package id: [{p.ID}] ");
            var id = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(id))
            {
                p.ID = id;
                p.Properties.SymlinkName = id;
            }
            else
                p.Properties.SymlinkName = p.ID;

            Console.Write($"Package version: [{p.Version}] ");
            var version = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(version)) p.Version = version;

            var packageJson = p.ToString();
            var jsonPath = Path.Combine(GetCustomDescriptorsFolder(p.ID), "shebang.json");

            try
            {
                using (StreamWriter writer = new StreamWriter(jsonPath))
                {
                    writer.Write(packageJson);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Log("Unknown exception while writing the JSON descriptor:", LogType.ERROR);
                Log(ex.Message, LogType.ERROR);
            }

            Console.WriteLine();
            WriteColoredText(new List<ColoredString>()
            {
                new ColoredString()
                {
                    Text = "Done! You can find the JSON descriptor at ",
                    ForegroundColor = ConsoleColor.Green
                },

                new ColoredString()
                {
                    Text = $" {jsonPath} ",
                    ForegroundColor = ConsoleColor.Black,
                    BackgroundColor = ConsoleColor.Green
                }
            });

            Console.WriteLine(Environment.NewLine + packageJson);
        }

        public static void RemovePackage(string packageId)
        {
            Log($"Removing package '{packageId}'...");
            string packageFolder = GetPackageFolder(packageId);

            CrossPlatformAction(
                () =>
                {
                    Log("Skipping removal of symlinks...", LogType.WARN);
                },
                
                () =>
                {
                    Log("Removing symlinks...");
                    try
                    {
                        string rawJson;
                        var descriptorPath = Path.Combine(packageFolder, "shebang.json");

                        if (File.Exists(descriptorPath))
                        {
                            StreamReader reader = new StreamReader(descriptorPath);
                            rawJson = reader.ReadToEnd();

                            Package package = GetPackage(rawJson);
                            var symlinkPath = $"/usr/bin/{package.Properties.SymlinkName}";
                            if (File.Exists(symlinkPath))
                                File.Delete(symlinkPath);
                            else
                                Log("Symlink not found, skipping...", LogType.WARN);
                        }
                    }
                    catch (Exception)
                    {
                        Log("Unknown error while removing symlink.", LogType.ERROR);
                    }
                });

            if (Directory.Exists(packageFolder))
            {
                ForceDeleteDirectory(packageFolder);
                Log("Package successfully removed!");
            }
            else
            {
                Log("This package is not installed and will not be removed.", LogType.ERROR);
            }
        }

        public static void Main(string[] args)
        {
            // Print splash screen
            PrintSplash();

            // Command-line arguments parser
            try
            {
                switch (args[0])
                {
                    case "install":
                    case "i":
                    case "get":
                        try
                        {
                            InstallPackage(args[1], args[2]);
                        }
                        catch (Exception)
                        {
                            InstallPackage(args[1]);
                        }
                        break;
                    case "create":
                    case "new":
                    case "make":
                        CreatePackage();
                        break;
                    case "remove":
                    case "delete":
                        RemovePackage(args[1]);
                        break;
                    case "update":
                    case "upgrade":
                        Update();
                        break;
                    case "clean":
                        CleanDescriptors();
                        break;
                    case "help":
                    case "?":
                        PrintSyntax();
                        break;
                    default:
                        Log("This command is not found.", LogType.ERROR);
                        Log("Type [ shebang help ] to view all available commands.", LogType.ERROR);
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                Log("You haven't specified any arguments.", LogType.ERROR);
            }
        }
    }
}
