﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NuGet;

namespace Microsoft.Framework.PackageManager
{
    public class InstallBuilder
    {
        public const string CommandsFolderName = "app";

        private static readonly string[] NotAllowedExportedCommandNames = new string[]
        {
            "dotnet",
            "dotnetsdk",
            "k",
            "klr",
            "kpm",
            "kvm",
            "nuget"
        };

        private readonly Runtime.Project _project;
        private readonly IPackageBuilder _packageBuilder;
        private readonly Reports _buildReport;

        public InstallBuilder(Runtime.Project project, IPackageBuilder packageBuilder, Reports buildReport)
        {
            _project = project;
            _packageBuilder = packageBuilder;
            _buildReport = buildReport;
            IsApplicationPackage = project.Commands.Any();
        }

        public bool IsApplicationPackage { get; private set; }

        public bool Build(string outputPath)
        {
            if (!IsApplicationPackage)
            {
                // This is not an application package
                return true;
            }

            if (!ValidateExportedCommands())
            {
                return false;
            }

            BuildApplicationFiles(outputPath);

            return true;
        }

        private bool ValidateExportedCommands()
        {
            // Get the commands that would conflict with .net commands
            var invalidExportedCommands = _project.Commands.Keys.Where(exported =>
                NotAllowedExportedCommandNames.Contains(exported));

            if (invalidExportedCommands.Any())
            {
                _buildReport.Error.WriteLine(
                    string.Format(
                        "The following names are not allowed as commands: {0}.",
                        string.Join(", ", invalidExportedCommands))
                    .Red());

                return false;
            }

            return true;
        }

        private void BuildApplicationFiles(string baseOutputPath)
        {
            var applicationFolder = Path.Combine(baseOutputPath, CommandsFolderName);
            Directory.CreateDirectory(applicationFolder);

            WriteApplicationProjectJsonFile(applicationFolder);
            WriteCommandsScripts(applicationFolder);
        }

        private void WriteApplicationProjectJsonFile(string applicationFolder)
        {
            var projectFileName = Path.GetFileName(_project.ProjectFilePath);
            var appScriptProjectFile = Path.Combine(
                applicationFolder,
                projectFileName);
            File.Copy(
                _project.ProjectFilePath,
                appScriptProjectFile,
                overwrite: true);

            Runtime.Project appProject;
            Runtime.Project.TryGetProject(appScriptProjectFile, out appProject);

            ModifyJson(appScriptProjectFile, projectFile =>
            {
                projectFile["entryPoint"] = _project.Name;
                projectFile["loadable"] = false;

                var dependencies = new JObject();
                projectFile[nameof(dependencies)] = dependencies;
                dependencies[_project.Name] = _project.Version.ToString();
            });

            _packageBuilder.Files.Add(new PhysicalPackageFile()
            {
                SourcePath = appScriptProjectFile,
                TargetPath = Path.Combine(CommandsFolderName, projectFileName)
            });
        }

        private void WriteCommandsScripts(string applicationFolder)
        {
            foreach (string command in _project.Commands.Keys.Distinct())
            {
                WriteWindowsScript(applicationFolder, command);
                WriteLinuxScript(applicationFolder, command);

                _buildReport.Information.WriteLine("Exported application command: " + command);
            }
        }

        private void WriteWindowsScript(string applicationFolder, string commandName)
        {
            var scriptFileName = commandName + ".cmd";
            var scriptFilePath = Path.Combine(applicationFolder, scriptFileName);
            var script = string.Format(
                "@" + Runtime.Constants.BootstrapperExeName + @" --appbase ""%~dp0."" Microsoft.Framework.ApplicationHost {0} %*",
                commandName);

            File.WriteAllText(scriptFilePath, script);

            _packageBuilder.Files.Add(new PhysicalPackageFile()
            {
                SourcePath = scriptFilePath,
                TargetPath = Path.Combine(CommandsFolderName, scriptFileName)
            });

            _buildReport.Verbose.WriteLine("Exported command script: " + scriptFileName);
        }

        private void WriteLinuxScript(string applicationFolder, string commandName)
        {
            var scriptFileName = commandName + ".sh";
            var scriptFilePath = Path.Combine(applicationFolder, scriptFileName);
            var script = string.Format(
                Runtime.Constants.BootstrapperExeName + @" --appbase ""$(dirname $0)"" Microsoft.Framework.ApplicationHost {0} $@",
                commandName);

            File.WriteAllText(scriptFilePath, script);

            _packageBuilder.Files.Add(new PhysicalPackageFile()
            {
                SourcePath = scriptFilePath,
                TargetPath = Path.Combine(CommandsFolderName, scriptFileName)
            });

            _buildReport.Verbose.WriteLine("Exported command script: " + scriptFileName);
        }

        private static void ModifyJson(string jsonFile, Action<JObject> modifier)
        {
            var jsonObject = JObject.Parse(File.ReadAllText(jsonFile));
            modifier(jsonObject);
            File.WriteAllText(jsonFile, jsonObject.ToString());
        }
    }
}