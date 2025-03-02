﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using SpecFlow.VisualStudio.Diagnostics;
using SpecFlow.VisualStudio.ProjectSystem;

namespace SpecFlow.VisualStudio
{
    public class WindowsFileAssociationDetector
    {
        // ReSharper disable UnusedMember.Local
        [Flags]
        enum AssocF
        {
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200
        }

        enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic,
            InfoTip,
            QuickTip,
            TileInfo,
            ContentType,
            DefaultIcon,
            ShellExtension
        }
        // ReSharper restore UnusedMember.Local

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra,
            [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);

        private const string ExtensionIcon = "gherkin_specflowvs.ico";
        private const string FileExtension = ".feature";
        private const string ProgId = "SpecFlow.GherkinFile";
        private const string FriendlyTypeName = "Gherkin Specification File for SpecFlow";

        private readonly IFileSystem _fileSystem;
        private readonly IIdeScope _ideScope;
        
        public WindowsFileAssociationDetector(IFileSystem fileSystem, IIdeScope ideScope)
        {
            _fileSystem = fileSystem;
            _ideScope = ideScope;
        }

        public bool? IsAssociated()
        {
            try
            {
                uint resultLength = 0;
                const AssocStr assocStr = AssocStr.FriendlyDocName;
                AssocQueryString(AssocF.NoTruncate, assocStr, FileExtension, null, null, ref resultLength);

                if (resultLength == 0)
                    return false;

                // Allocate the output buffer
                StringBuilder pszOut = new StringBuilder((int)resultLength);

                // Get the full pathname to the program in pszOut
                AssocQueryString(AssocF.Verify, assocStr, FileExtension, null, pszOut, ref resultLength);

                string doc = pszOut.ToString();
                
                return doc.IndexOf("SpecFlow", StringComparison.InvariantCultureIgnoreCase) >= 0;
            }
            catch (Exception ex)
            {
                _ideScope.Logger.LogException(_ideScope.MonitoringService, ex, $"IsAssociated failed: {ex.Message}");
                return null;
            }
        }

        private bool EnsureIcon(string iconPath)
        {
            try
            {
                if (!File.Exists(iconPath))
                {
                    var gherkinIcon = Path.Combine(_ideScope.GetExtensionFolder(), "Package", "Resources",
                        ExtensionIcon);
                    _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(iconPath));
                    _fileSystem.File.Copy(gherkinIcon, iconPath, true);
                }
            }
            catch (Exception)
            {
                return false;
            }
            
            return true;
        }

        public bool SetAssociation()
        {
            try
            {
                string appPath = Process.GetCurrentProcess().MainModule.FileName;
                string iconPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SpecFlow", ExtensionIcon);
                const string classesBaseKey = @"Software\Classes\" + ProgId;
                using (var key = Registry.CurrentUser.CreateSubKey(classesBaseKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (key == null)
                        return false;

                    key.SetValue(null, FriendlyTypeName);
                    key.SetValue("FriendlyTypeName", FriendlyTypeName);
                }

                if (EnsureIcon(iconPath))
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(classesBaseKey + @"\DefaultIcon", RegistryKeyPermissionCheck.ReadWriteSubTree))
                    {
                        if (key == null)
                            return false;

                        key.SetValue(null, iconPath);
                    }
                }

                using (var key = Registry.CurrentUser.CreateSubKey(classesBaseKey + @"\shell", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (key == null)
                        return false;

                    key.SetValue(null, "Open");
                }

                using (var key = Registry.CurrentUser.CreateSubKey(classesBaseKey + @"\shell\open", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (key == null)
                        return false;

                    key.SetValue(null, "&Open");
                }

                using (var key = Registry.CurrentUser.CreateSubKey(classesBaseKey + @"\shell\open\command", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (key == null)
                        return false;

                    key.SetValue(null, string.Format(@"""{0}"" /edit ""%1""", appPath));
                }

                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + FileExtension, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (key == null)
                        return false;

                    key.SetValue(null, ProgId);
                    key.SetValue("Content Type", "application/text");
                }
                return true;
            }
            catch (Exception ex)
            {
                _ideScope.Logger.LogException(_ideScope.MonitoringService, ex, $"SetAssociation failed: {ex.Message}");
                return false;
            }
        }
    }
}
