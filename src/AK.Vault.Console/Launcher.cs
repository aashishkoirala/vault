/*******************************************************************************************************************************
 * AK.Vault.Console.Launcher
 * Copyright © 2014-2016 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of VAULT.
 *  
 * VAULT is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * VAULT is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with VAULT.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

#region Namespace Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

#endregion

namespace AK.Vault.Console
{
    /// <summary>
    /// Controls the interactive UI.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class Launcher
    {
        private const int ItemsPerPage = 15;

        private readonly IFileEncryptor fileEncryptor;
        private readonly IDictionary<ConsoleKey, ExecutableMenuItem> executableMenu;
        private static readonly char[] MenuKeyChars = 
            new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E' };

        private IDictionary<char, SelectableMenuItem> selectableMenu;
        private FolderEntry currentFolder;
        private int itemIndex;
        private SelectableMenuItem selectedMenuItem;
        private bool isRunning = true;

        public Launcher(IFileEncryptor fileEncryptor, IListGenerator listGenerator, string vaultName)
        {
            this.fileEncryptor = fileEncryptor;
            this.currentFolder = listGenerator.Generate(vaultName);
            this.executableMenu = this.BuildExecutableMenu();
        }

        public void Run()
        {
            this.BuildSelectableMenu();
            if (!this.selectableMenu.Any())
            {
                Screen.Print("There is nothing in the vault");
                return;
            }

            this.PrintMenu();

            while (this.isRunning)
            {
                var keyInfo = Screen.ReadKey();

                ExecutableMenuItem executableMenuItem;
                if (this.executableMenu.TryGetValue(keyInfo.Key, out executableMenuItem))
                {
                    if (!executableMenuItem.Visible) continue;

                    executableMenuItem.Execute();
                    if (!this.isRunning) break;

                    this.PrintMenu();
                    continue;
                }

                var keyChar = keyInfo.KeyChar.ToString(CultureInfo.InvariantCulture).ToUpper()[0];
                
                SelectableMenuItem selectableMenuItem;
                if (!this.selectableMenu.TryGetValue(keyChar, out selectableMenuItem)) continue;

                this.selectedMenuItem = selectableMenuItem;
                this.PrintMenu();
            }
        }

        private void BuildSelectableMenu()
        {
            this.selectableMenu =
                this.currentFolder.Folders
                    .Concat(this.currentFolder.Files.Cast<object>())
                    .Skip(this.itemIndex)
                    .Take(ItemsPerPage)
                    .Select(x => x is FileEntry
                                     ? new SelectableMenuItem((FileEntry) x)
                                     : new SelectableMenuItem(x as FolderEntry))
                    .Select((x, i) => new {Key = MenuKeyChars[i], Value = x})
                    .ToDictionary(x => x.Key, x => x.Value);

            this.selectedMenuItem = this.selectableMenu.First().Value;
        }

        private IDictionary<ConsoleKey, ExecutableMenuItem> BuildExecutableMenu()
        {
            return new Dictionary<ConsoleKey, ExecutableMenuItem>
                {
                    {
                        ConsoleKey.Enter,
                        new ExecutableMenuItem(
                            "Open Selected Folder", this.OpenSelected, () => this.selectedMenuItem.IsFolder)
                    },

                    {
                        ConsoleKey.F10,
                        new ExecutableMenuItem("Decrypt Selected Item", this.DecryptSelected, () => true)
                    },
                    {
                        ConsoleKey.Backspace,
                        new ExecutableMenuItem(
                            "Go Back To Parent", this.NavigateToParent, () => this.currentFolder.Parent != null)
                    },
                    {
                        ConsoleKey.PageDown,
                        new ExecutableMenuItem(
                            "Go To Next Page", this.GoToNextPage,
                            () =>
                            this.itemIndex + ItemsPerPage <
                            this.currentFolder.Folders.Count + this.currentFolder.Files.Count)
                    },
                    {
                        ConsoleKey.PageUp,
                        new ExecutableMenuItem(
                            "Go To Previous Page", this.GoToPreviousPage, () => this.itemIndex > 0)
                    },
                    {ConsoleKey.Escape, new ExecutableMenuItem("Quit", this.Quit, () => true)}
                };
        }

        private void PrintMenu()
        {
            Screen.Clear();
            Screen.Print(Screen.Colors.Heading, " VAULT by Aashish Koirala (c) 2014");
            Screen.Print();
            Screen.Print(" {0}", new string('─', 70));
            Screen.Print();

            foreach (var key in this.selectableMenu.Keys)
            {
                var item = this.selectableMenu[key];

                var foreColor = item == this.selectedMenuItem
                                    ? Screen.Colors.CurrentBackground
                                    : Screen.Colors.CurrentForeground;

                var backColor = item == this.selectedMenuItem
                                    ? Screen.Colors.CurrentForeground
                                    : Screen.Colors.CurrentBackground;

                System.Console.Write("  ");
                Screen.Print(foreColor, backColor, " {0} - {1} ", key,
                             item.IsFolder ? $"[{item.FolderEntry.Name}]" : item.FileEntry.Name);
            }

            Screen.Print();
            Screen.Print(" {0}", new string('─', 70));
            Screen.Print();

            foreach (var pair in this.executableMenu.Where(pair => pair.Value.Visible))
                Screen.Print("   {0} - {1}", pair.Key.ToString(), pair.Value.Name);
        }

        private void OpenSelected()
        {
            if (!this.selectedMenuItem.IsFolder) return;

            this.currentFolder = this.selectedMenuItem.FolderEntry;
            this.BuildSelectableMenu();
        }

        private void DecryptSelected()
        {
            var path = this.selectedMenuItem.IsFolder ? this.DecryptSelectedFolder() : this.DecryptSelectedFile();
            if (string.IsNullOrWhiteSpace(path)) return;

            Screen.Print("What would you like to do with:{0}\"{1}\"?", Environment.NewLine, path);
            Screen.Print();
            Screen.Print("1 - Launch It");
            if (!this.selectedMenuItem.IsFolder) Screen.Print("2 - Open Containing Folder");
            Screen.Print("Escape - Do Nothing");

            while (true)
            {
                var keyInfo = Screen.ReadKey();
                if (keyInfo.KeyChar == '1')
                {
                    LaunchPath(path);
                    break;
                }
                if (keyInfo.KeyChar == '2' && !this.selectedMenuItem.IsFolder)
                {
                    LaunchPath(Path.GetDirectoryName(path));
                    break;
                }
                if (keyInfo.Key == ConsoleKey.Escape) break;
            }
        }

        private string DecryptSelectedFile()
        {
            Screen.Print("Decrypting {0}...", this.selectedMenuItem.FileEntry.OriginalFullPath);
            Screen.Print();

            Exception exception;
            try
            {
                var results =
                    this.fileEncryptor.Decrypt(new[]
                        {Path.GetFileName(this.selectedMenuItem.FileEntry.EncryptedFullPath)});
                var result = results.Single();

                if (result.IsDone) return result.UnencryptedFilePath;
                exception = result.Exception;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Screen.Print(Screen.Colors.Error, "There was a problem decrypting: {0}", exception.Message);
            Screen.Print();
            Screen.Print("Press any key to continue.");
            Screen.ReadKey();

            return null;
        }

        private string DecryptSelectedFolder()
        {
            Screen.Print("Decrypting {0}...", this.selectedMenuItem.FolderEntry.FullPath);
            Screen.Print();

            var files = new Collection<FileEntry>();
            TraverseFolder(this.selectedMenuItem.FolderEntry, files);

            var filePatternList = files.Select(x => Path.GetFileName(x.EncryptedFullPath));

            var results = this.fileEncryptor.Decrypt(filePatternList).ToArray();

            var doneCount = results.Count(x => x.IsDone);

            Screen.Print("{0} of {1} files decrypted successfully.", doneCount, results.Length);

            return doneCount > 0 ? Path.GetDirectoryName(results.First(x => x.IsDone).UnencryptedFilePath) : null;
        }

        private static void LaunchPath(string path)
        {
            var process = Process.Start(path);
            if (process == null) return;

            process.Exited += (sender, e) => process.Dispose();
        }

        private void NavigateToParent()
        {
            if (this.currentFolder.Parent == null) return;
            this.currentFolder = this.currentFolder.Parent;
            this.BuildSelectableMenu();
        }

        private void GoToNextPage()
        {
            if (this.itemIndex + ItemsPerPage > this.currentFolder.Folders.Count + this.currentFolder.Files.Count)
                return;

            this.itemIndex += ItemsPerPage;
            this.BuildSelectableMenu();
        }

        private void GoToPreviousPage()
        {
            if (this.itemIndex == 0) return;

            this.itemIndex -= ItemsPerPage;
            this.BuildSelectableMenu();
        }

        private void Quit()
        {
            this.isRunning = false;
        }

        private static void TraverseFolder(FolderEntry folder, ICollection<FileEntry> files)
        {
            foreach (var file in folder.Files) files.Add(file);
            foreach (var child in folder.Folders) TraverseFolder(child, files);
        }
    }

    internal class SelectableMenuItem
    {
        public SelectableMenuItem(FolderEntry folderEntry)
        {
            this.FolderEntry = folderEntry;
        }

        public SelectableMenuItem(FileEntry fileEntry)
        {
            this.FileEntry = fileEntry;
        }

        public FileEntry FileEntry { get; }

        public FolderEntry FolderEntry { get; }

        public bool IsFolder => this.FolderEntry != null;
    }

    internal class ExecutableMenuItem
    {
        private readonly Action action;
        private readonly Func<bool> showIf;

        public ExecutableMenuItem(string name, Action action, Func<bool> showIf)
        {
            this.Name = name;
            this.action = action;
            this.showIf = showIf;
        }

        public string Name { get; }

        public bool Visible => this.showIf();

        public void Execute()
        {
            this.action();
        }
    }
}