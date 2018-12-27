/*******************************************************************************************************************************
 * Copyright © 2014-2019 Aashish Koirala <https://www.aashishkoirala.com>
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
 * along with VAULT.  If not, see <https://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AK.Vault.Console
{
    /// <summary>
    /// Controls the interactive UI.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class Launcher
    {
        private const int ItemsPerPage = 15;

        private readonly FileEncryptor _fileEncryptor;
        private readonly IDictionary<ConsoleKey, ExecutableMenuItem> _executableMenu;
        private static readonly char[] MenuKeyChars = 
            new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E' };

        private IDictionary<char, SelectableMenuItem> _selectableMenu;
        private FolderEntry _currentFolder;
        private int _itemIndex;
        private SelectableMenuItem _selectedMenuItem;
        private bool _isRunning = true;

        public Launcher(FileEncryptor fileEncryptor, ListGenerator listGenerator, string vaultName)
        {
            _fileEncryptor = fileEncryptor;
            _currentFolder = listGenerator.Generate(vaultName);
            _executableMenu = BuildExecutableMenu();
        }

        public void Run()
        {
            BuildSelectableMenu();
            if (!_selectableMenu.Any())
            {
                Screen.Print("There is nothing in the vault");
                return;
            }

            PrintMenu();

            while (_isRunning)
            {
                var keyInfo = Screen.ReadKey();

                ExecutableMenuItem executableMenuItem;
                if (_executableMenu.TryGetValue(keyInfo.Key, out executableMenuItem))
                {
                    if (!executableMenuItem.Visible) continue;

                    executableMenuItem.Execute();
                    if (!_isRunning) break;

                    PrintMenu();
                    continue;
                }

                var keyChar = keyInfo.KeyChar.ToString(CultureInfo.InvariantCulture).ToUpper()[0];
                
                SelectableMenuItem selectableMenuItem;
                if (!_selectableMenu.TryGetValue(keyChar, out selectableMenuItem)) continue;

                _selectedMenuItem = selectableMenuItem;
                PrintMenu();
            }
        }

        private void BuildSelectableMenu()
        {
            _selectableMenu =
                _currentFolder.Folders
                    .Concat(_currentFolder.Files.Cast<object>())
                    .Skip(_itemIndex)
                    .Take(ItemsPerPage)
                    .Select(x => x is FileEntry
                                     ? new SelectableMenuItem((FileEntry) x)
                                     : new SelectableMenuItem(x as FolderEntry))
                    .Select((x, i) => new {Key = MenuKeyChars[i], Value = x})
                    .ToDictionary(x => x.Key, x => x.Value);

            _selectedMenuItem = _selectableMenu.First().Value;
        }

        private IDictionary<ConsoleKey, ExecutableMenuItem> BuildExecutableMenu()
        {
            return new Dictionary<ConsoleKey, ExecutableMenuItem>
                {
                    {
                        ConsoleKey.Enter,
                        new ExecutableMenuItem(
                            "Open Selected Folder", OpenSelected, () => _selectedMenuItem.IsFolder)
                    },

                    {
                        ConsoleKey.F10,
                        new ExecutableMenuItem("Decrypt Selected Item", DecryptSelected, () => true)
                    },
                    {
                        ConsoleKey.Backspace,
                        new ExecutableMenuItem(
                            "Go Back To Parent", NavigateToParent, () => _currentFolder.Parent != null)
                    },
                    {
                        ConsoleKey.PageDown,
                        new ExecutableMenuItem(
                            "Go To Next Page", GoToNextPage,
                            () =>
                            _itemIndex + ItemsPerPage <
                            _currentFolder.Folders.Count + _currentFolder.Files.Count)
                    },
                    {
                        ConsoleKey.PageUp,
                        new ExecutableMenuItem(
                            "Go To Previous Page", GoToPreviousPage, () => _itemIndex > 0)
                    },
                    {ConsoleKey.Escape, new ExecutableMenuItem("Quit", Quit, () => true)}
                };
        }

        private void PrintMenu()
        {
            Screen.Clear();
            Screen.Print(Screen.Colors.Heading, " VAULT by Aashish Koirala (c) 2014");
            Screen.Print();
            Screen.Print(" {0}", new string('─', 70));
            Screen.Print();

            foreach (var key in _selectableMenu.Keys)
            {
                var item = _selectableMenu[key];

                var foreColor = item == _selectedMenuItem
                                    ? Screen.Colors.CurrentBackground
                                    : Screen.Colors.CurrentForeground;

                var backColor = item == _selectedMenuItem
                                    ? Screen.Colors.CurrentForeground
                                    : Screen.Colors.CurrentBackground;

                System.Console.Write("  ");
                Screen.Print(foreColor, backColor, " {0} - {1} ", key,
                             item.IsFolder ? $"[{item.FolderEntry.Name}]" : item.FileEntry.Name);
            }

            Screen.Print();
            Screen.Print(" {0}", new string('─', 70));
            Screen.Print();

            foreach (var pair in _executableMenu.Where(pair => pair.Value.Visible))
                Screen.Print("   {0} - {1}", pair.Key.ToString(), pair.Value.Name);
        }

        private void OpenSelected()
        {
            if (!_selectedMenuItem.IsFolder) return;

            _currentFolder = _selectedMenuItem.FolderEntry;
            BuildSelectableMenu();
        }

        private void DecryptSelected()
        {
            var path = _selectedMenuItem.IsFolder ? DecryptSelectedFolder() : DecryptSelectedFile();
            if (string.IsNullOrWhiteSpace(path)) return;

            Screen.Print("What would you like to do with:{0}\"{1}\"?", Environment.NewLine, path);
            Screen.Print();
            Screen.Print("1 - Launch It");
            if (!_selectedMenuItem.IsFolder) Screen.Print("2 - Open Containing Folder");
            Screen.Print("Escape - Do Nothing");

            while (true)
            {
                var keyInfo = Screen.ReadKey();
                if (keyInfo.KeyChar == '1')
                {
                    LaunchPath(path);
                    break;
                }
                if (keyInfo.KeyChar == '2' && !_selectedMenuItem.IsFolder)
                {
                    LaunchPath(Path.GetDirectoryName(path));
                    break;
                }
                if (keyInfo.Key == ConsoleKey.Escape) break;
            }
        }

        private string DecryptSelectedFile()
        {
            Screen.Print("Decrypting {0}...", _selectedMenuItem.FileEntry.OriginalFullPath);
            Screen.Print();

            Exception exception;
            try
            {
                var results =
                    _fileEncryptor.Decrypt(new[]
                        {Path.GetFileName(_selectedMenuItem.FileEntry.EncryptedFullPath)});
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
            Screen.Print("Decrypting {0}...", _selectedMenuItem.FolderEntry.FullPath);
            Screen.Print();

            var files = new Collection<FileEntry>();
            TraverseFolder(_selectedMenuItem.FolderEntry, files);

            var filePatternList = files.Select(x => Path.GetFileName(x.EncryptedFullPath));

            var results = _fileEncryptor.Decrypt(filePatternList).ToArray();

            var doneCount = results.Count(x => x.IsDone);

            Screen.Print("{0} of {1} files decrypted successfully.", doneCount, results.Length);

            return doneCount > 0 ? Path.GetDirectoryName(results.First(x => x.IsDone).UnencryptedFilePath) : null;
        }

        private static void LaunchPath(string path)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Screen.Print(ConsoleColor.Red, "Sorry launching from here is not permitted on this OS.");
                return;
            }

            var process = path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ?
                Process.Start(path) :
                Process.Start(new ProcessStartInfo("cmd", $"/c start \"\" \"{path}\"") { CreateNoWindow = true });
            if (process == null) return;

            process.Exited += (sender, e) => process.Dispose();
        }

        private void NavigateToParent()
        {
            if (_currentFolder.Parent == null) return;
            _currentFolder = _currentFolder.Parent;
            BuildSelectableMenu();
        }

        private void GoToNextPage()
        {
            if (_itemIndex + ItemsPerPage > _currentFolder.Folders.Count + _currentFolder.Files.Count)
                return;

            _itemIndex += ItemsPerPage;
            BuildSelectableMenu();
        }

        private void GoToPreviousPage()
        {
            if (_itemIndex == 0) return;

            _itemIndex -= ItemsPerPage;
            BuildSelectableMenu();
        }

        private void Quit()
        {
            _isRunning = false;
        }

        private static void TraverseFolder(FolderEntry folder, ICollection<FileEntry> files)
        {
            foreach (var file in folder.Files) files.Add(file);
            foreach (var child in folder.Folders) TraverseFolder(child, files);
        }
    }

    internal class SelectableMenuItem
    {
        public SelectableMenuItem(FolderEntry folderEntry) => FolderEntry = folderEntry;

        public SelectableMenuItem(FileEntry fileEntry) => FileEntry = fileEntry;

        public FileEntry FileEntry { get; }

        public FolderEntry FolderEntry { get; }

        public bool IsFolder => FolderEntry != null;
    }

    internal class ExecutableMenuItem
    {
        private readonly Action _action;
        private readonly Func<bool> _showIf;

        public ExecutableMenuItem(string name, Action action, Func<bool> showIf)
        {
            Name = name;
            _action = action;
            _showIf = showIf;
        }

        public string Name { get; }

        public bool Visible => _showIf();

        public void Execute() => _action();
    }
}
