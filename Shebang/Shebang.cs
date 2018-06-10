﻿using System;
using System.Net;
using Newtonsoft.Json;
using static Shebang.Serialization;
using static Shebang.Logger;
using static Shebang.Models;
using System.IO;
using LibGit2Sharp;

namespace Shebang
{
    public class Shebang
    {
        public static void PrintSyntax()
        {
            string[] syntax = new string[]
            {
                "Error: You must specify a GitHub username and repository.",
                "Format: shebang install <username>/<repository> [branch]",
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

        public static void PrintSplash()
        {
            // TO-DO: Print splash screen with colors and ASCII art
        }

        public static Package GetPackage(string json)
        {
            Package package = JsonConvert.DeserializeObject<Package>(json);
            return package;
        }

        public static string GetPackagesFolder()
        {
            string packagesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "shebang", "packages");
            if (!Directory.Exists(packagesFolder))
            {
                Directory.CreateDirectory(packagesFolder);
            }

            return packagesFolder;
        }

        public static void InstallPackage(string repository, string branch = "master")
        {
            // Clear the console window
            Console.Clear();

            string repositoryUrl = $"https://github.com/{repository}";
            string descriptorUrl = $"https://raw.githubusercontent.com/{repository}/{branch}/shebang.json";

            Log("Getting install descriptor...");
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

                string packageFolder = Path.Combine(GetPackagesFolder(), package.ID);
                if (Directory.Exists(packageFolder))
                {
                    Log($"'{package.ID}' already exists, reinstalling...");
                    Utils.ForceDeleteDirectory(packageFolder);
                }

                Log("Cloning repository...");
                Repository.Clone(repositoryUrl, packageFolder, new CloneOptions()
                {
                    BranchName = branch
                });

                Log("Successfully cloned.");
                Log("Running preinstall script...");
                package.Properties.Commands.Execute(ScriptType.BEFORE_INSTALL);

                Log("Setting permissions...");
                try
                {
                    var files = Directory.GetFiles(packageFolder, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        fileInfo.Attributes = FileAttributes.Normal;
                    }
                }
                catch (Exception)
                {
                    Log("Error while setting permissions; do you have administrator/superuser rights?", LogType.ERROR);
                }

                var executablePath = Path.Combine(packageFolder, package.Properties.ExecutablePath);
                Utils.CrossPlatformAction(
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
                        Utils.RunShellCommand($"ln -s {executablePath} /usr/bin/{package.Properties.SymlinkName}", false);
                    });

                Log("Running postinstall script...");
                package.Properties.Commands.Execute(ScriptType.AFTER_INSTALL);
            }
            catch (WebException e)
            {
                HttpWebResponse response = (HttpWebResponse)e.Response;
                try
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            Log("The shebang.json file could not be found in this repository.", LogType.ERROR);
                            break;
                        case HttpStatusCode.ServiceUnavailable:
                            Log("The GitHub services are currently unavailable or you are sending too much requests.", LogType.ERROR);
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
        }

        public static void RemovePackage(string packageId)
        {
        }

        public static void Main(string[] args)
        {
            try
            {
                switch (args[0])
                {
                    case "install":
                    case "i":
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
                        CreatePackage();
                        break;
                    case "remove":
                        RemovePackage(args[1]);
                        break;
                    case "update":
                        Utils.Update();
                        break;
                    default:
                        Log("This command is not found.", LogType.WARN);
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                Log("You haven't specified any arguments.", LogType.ERROR);
            }

            Console.ReadLine();
        }
    }
}
