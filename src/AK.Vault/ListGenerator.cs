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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace AK.Vault
{
    /// <summary>
    /// Generates a list (or a tree, rather) representing all files/folders stored
    /// as encrypted files in the vault.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class ListGenerator
    {
        private readonly FileNameManager _fileNameManager;
        private readonly VaultOptions _vaultOptions;

        public ListGenerator(FileNameManager fileNameManager, IOptionsMonitor<VaultOptions> vaultOptionsMonitor)
        {
            _fileNameManager = fileNameManager;
            _vaultOptions = vaultOptionsMonitor.CurrentValue;
        }

        /// <summary>
        /// Generates the tree and returns the FolderEntry object representing the
        /// root that can be used to traverse down to everything else.
        /// </summary>
        /// <param name="vaultName">Vault name.</param>
        /// <returns></returns>
        public async Task<FolderEntry> Generate(string vaultName)
        {
            var encryptedFileLocation = _vaultOptions.Vaults
                .Single(x => x.Name == vaultName).EncryptedFileLocation;

            var files = Directory
                .GetFiles(encryptedFileLocation, "*.vault", SearchOption.TopDirectoryOnly)
                .Select(x => new FileNameInfo {Original = string.Empty, Encrypted = x})
                .ToArray();

            var taskHash = files.ToDictionary(x => x.Encrypted, x => ExtractFileName(x.Encrypted));

            foreach(var file in files)
            {
                file.Original = await taskHash[file.Encrypted];
            }

            var fileMap = files.ToDictionary(x => x.Original, x => x.Encrypted);

            var folderEntryMap = new Dictionary<string, FolderEntry>();

            foreach (var file in fileMap.Keys.OrderBy(x => x))
            {
                var folder = Path.GetDirectoryName(file);
                if (folder == null) throw new Exception($"Unexpected error - cannot get directory name from file {file}.");

                if (!folderEntryMap.TryGetValue(folder, out FolderEntry folderEntry))
                {
                    folderEntry = new FolderEntry { FullPath = folder, Name = Path.GetFileName(folder) };
                    if (folder != Path.GetPathRoot(folder))
                        folderEntry.Parent = GetOrCreateParent(folderEntry, folderEntryMap);
                    folderEntryMap[folder] = folderEntry;
                }

                var fileEntry = new FileEntry
                    {
                        OriginalFullPath = file,
                        EncryptedFullPath = fileMap[file],
                        Name = Path.GetFileName(file),
                        Parent = folderEntry
                    };

                folderEntry.Files.Add(fileEntry);
            }

            var root = folderEntryMap.Values.Single(x => x.FullPath == Path.GetPathRoot(x.FullPath));
            root.Name = "ROOT";

            return root;
        }

        private FolderEntry GetOrCreateParent(FolderEntry folderEntry, IDictionary<string, FolderEntry> folderEntryMap)
        {
            var parentFolder = Path.GetDirectoryName(folderEntry.FullPath);
            Debug.Assert(parentFolder != null);

            if (!folderEntryMap.TryGetValue(parentFolder, out FolderEntry parentFolderEntry))
            {
                parentFolderEntry = new FolderEntry { FullPath = parentFolder, Name = Path.GetFileName(parentFolder) };
                folderEntryMap[parentFolder] = parentFolderEntry;
                if (parentFolder != Path.GetPathRoot(parentFolder))
                    parentFolderEntry.Parent = GetOrCreateParent(parentFolderEntry, folderEntryMap);
            }

            folderEntry.Parent = parentFolderEntry;
            parentFolderEntry?.Folders.Add(folderEntry);

            return parentFolderEntry;
        }

        private async Task<string> ExtractFileName(string encryptedFile)
        {
            using (var stream = File.OpenRead(encryptedFile))
            {
                return await _fileNameManager.ReadOriginalFileNameFromStream(stream);
            }
        }

        private class FileNameInfo
        {
            public string Encrypted { get; set; }
            public string Original { get; set; }
        }
    }
}
