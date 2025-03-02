﻿using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpecFlow.VisualStudio.ProjectSystem
{
    internal class SpecFlowPackageDetector
    {
        private const string SpecFlowPackageName = "SpecFlow";
        public const string SpecFlowToolsMsBuildGenerationPackageName = "SpecFlow.Tools.MsBuild.Generation";
        public const string SpecFlowXUnitAdapterPackageName = "SpecFlow.xUnitAdapter";
        public const string CucumberExpressionPluginPackageNamePrefix = "CucumberExpressions.SpecFlow";
        public const string SpecFlowPlusRunnerPluginPackageNamePrefix = "SpecRun.SpecFlow";

        private static readonly string[] SpecFlowTestFrameworkPackages =
        {
            "SpecFlow.MsTest",
            "SpecFlow.xUnit",
            "SpecFlow.NUnit",
            "SpecFlow.MsTest"
        };

        private static readonly string[] KnownSpecFlowPackages =
            SpecFlowTestFrameworkPackages
                .Concat(new[]
                {
                    SpecFlowToolsMsBuildGenerationPackageName
                })
                .ToArray();

        private const string SpecRunPackageRe = @"^SpecRun.SpecFlow.(?<sfver>[\d-]+)$";
        private const string SpecSyncPackageRe = @"^SpecSync.AzureDevOps.SpecFlow.(?<sfver>[\d-]+)$";

        private static readonly Regex[] KnownSpecFlowExtensions =
        {
            new Regex(SpecRunPackageRe),
            new Regex(SpecSyncPackageRe),
        };

        private readonly IFileSystem _fileSystem;

        public SpecFlowPackageDetector(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool IsMsBuildGenerationEnabled(IEnumerable<NuGetPackageReference> packageReferences)
        {
            return packageReferences.Any(pr => pr.PackageName == SpecFlowToolsMsBuildGenerationPackageName);
        }

        public bool IsXUnitAdapterEnabled(IEnumerable<NuGetPackageReference> packageReferences)
        {
            return packageReferences.Any(pr => pr.PackageName == SpecFlowXUnitAdapterPackageName);
        }

        public bool IsCucumberExpressionPluginEnabled(IEnumerable<NuGetPackageReference> packageReferences)
        {
            return packageReferences.Any(pr => pr.PackageName != null && pr.PackageName.StartsWith(CucumberExpressionPluginPackageNamePrefix));
        }

        public bool IsSpecFlowTestFrameworkPackagesUsed(NuGetPackageReference[] packageReferences)
        {
            return packageReferences.Any(pr => SpecFlowTestFrameworkPackages.Contains(pr.PackageName) || pr.PackageName.StartsWith(SpecFlowPlusRunnerPluginPackageNamePrefix));
        }

        public NuGetPackageReference GetSpecFlowPackage(IEnumerable<NuGetPackageReference> packageReferences)
        {
            NuGetPackageReference knownSpecFlowPackage = null;
            NuGetPackageReference knownExtensionPackage = null;
            string knownExtensionSpecFlowVersion = null;
            foreach (var packageReference in packageReferences)
            {
                if (packageReference.PackageName == SpecFlowPackageName && !packageReference.Version.IsFloating)
                    return packageReference;

                if (packageReference.InstallPath == null)
                    continue;

                if (knownSpecFlowPackage == null && KnownSpecFlowPackages.Contains(packageReference.PackageName))
                    knownSpecFlowPackage = packageReference;

                if (knownExtensionPackage == null)
                {
                    var match = KnownSpecFlowExtensions.Select(e => e.Match(packageReference.PackageName)).FirstOrDefault(m => m.Success);
                    if (match != null)
                    {
                        knownExtensionPackage = packageReference;
                        knownExtensionSpecFlowVersion = match.Groups["sfver"].Value.Replace("-", ".");
                    }
                }
            }

            if (knownSpecFlowPackage != null)
            {
                var specFlowInstallPath = ReplacePackageNamePart(knownSpecFlowPackage.InstallPath, knownSpecFlowPackage.PackageName, SpecFlowPackageName);
                if (specFlowInstallPath != knownSpecFlowPackage.InstallPath && _fileSystem.Directory.Exists(specFlowInstallPath))
                    return new NuGetPackageReference(SpecFlowPackageName, knownSpecFlowPackage.Version, specFlowInstallPath);
            }

            if (knownExtensionPackage != null)
            {
                while (knownExtensionSpecFlowVersion.Count(c => c == '.') < 2)
                    knownExtensionSpecFlowVersion += ".0";
                var specFlowInstallPath = ReplacePackageNamePart(knownExtensionPackage.InstallPath, knownExtensionPackage.PackageName, SpecFlowPackageName);
                specFlowInstallPath = ReplacePackageVersionPart(specFlowInstallPath, knownExtensionPackage.Version.ToString(), knownExtensionSpecFlowVersion);
                if (specFlowInstallPath != knownExtensionPackage.InstallPath && _fileSystem.Directory.Exists(specFlowInstallPath))
                    return new NuGetPackageReference(SpecFlowPackageName, new NuGetVersion(knownExtensionSpecFlowVersion, knownExtensionSpecFlowVersion), specFlowInstallPath);
            }

            return null;
        }

        private string ReplacePackageNamePart(string path, string oldPart, string newPart)
        {
            return Regex.Replace(path, @"(?<=[\\\/])" + oldPart + @"(?=[\\\/\.])", newPart, RegexOptions.IgnoreCase);
        }

        private string ReplacePackageVersionPart(string path, string oldPart, string newPart)
        {
            return Regex.Replace(path, @"(?<=[\\\/\.])" + oldPart + @"$", newPart, RegexOptions.IgnoreCase);
        }
    }
}
