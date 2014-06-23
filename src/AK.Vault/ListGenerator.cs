/*******************************************************************************************************************************
 * AK.Vault.ListGenerator
 * Copyright © 2014 Aashish Koirala <http://aashishkoirala.github.io>
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
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace AK.Vault
{
    /// <summary>
    /// Generates a list (or a tree, rather) representing all files/folders stored
    /// as encrypted files in the vault.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IListGenerator
    {
        /// <summary>
        /// Generates the tree and returns the FolderEntry object representing the
        /// root that can be used to traverse down to everything else.
        /// </summary>
        /// <returns></returns>
        FolderEntry Generate();
    }

    [Export(typeof (IListGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    internal class ListGenerator : IListGenerator
    {
        private readonly IFileNameManager fileNameManager;
        private readonly IConfigurationProvider configurationProvider;

        [ImportingConstructor]
        internal ListGenerator(
            [Import] IFileNameManager fileNameManager,
            [Import] IConfigurationProvider configurationProvider)
        {
            this.fileNameManager = fileNameManager;
            this.configurationProvider = configurationProvider;
        }

        public FolderEntry Generate()
        {
            var files = Directory
                .GetFiles(this.configurationProvider.EncryptedFileLocation, "*.vault", SearchOption.TopDirectoryOnly)
                .Select(x => new FileNameInfo {Original = string.Empty, Encrypted = x})
                .ToArray();

            Parallel.ForEach(files, x => x.Original = this.ExtractFileName(x.Encrypted));

            var fileMap = files.ToDictionary(x => x.Original, x => x.Encrypted);

            var folderEntryMap = new Dictionary<string, FolderEntry>();

            foreach (var file in fileMap.Keys.OrderBy(x => x))
            {
                var folder = Path.GetDirectoryName(file);
                if (folder == null)
                {
                    var message = string.Format("Unexpected error - cannot get directory name from file {0}.", file);
                    throw new Exception(message);
                }

                FolderEntry folderEntry;
                if (!folderEntryMap.TryGetValue(folder, out folderEntry))
                {
                    folderEntry = new FolderEntry {FullPath = folder, Name = Path.GetFileName(folder)};
                    if (folder != Path.GetPathRoot(folder))
                        folderEntry.Parent = this.GetOrCreateParent(folderEntry, folderEntryMap);
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

            FolderEntry parentFolderEntry;
            if (!folderEntryMap.TryGetValue(parentFolder, out parentFolderEntry))
            {
                parentFolderEntry = new FolderEntry {FullPath = parentFolder, Name = Path.GetFileName(parentFolder)};
                folderEntryMap[parentFolder] = parentFolderEntry;
                if (parentFolder != Path.GetPathRoot(parentFolder))
                    parentFolderEntry.Parent = this.GetOrCreateParent(parentFolderEntry, folderEntryMap);
            }

            folderEntry.Parent = parentFolderEntry;
            if (parentFolderEntry != null) parentFolderEntry.Folders.Add(folderEntry);

            return parentFolderEntry;
        }

        private string ExtractFileName(string encryptedFile)
        {
            using (var stream = File.OpenRead(encryptedFile))
                return this.fileNameManager.ReadOriginalFileNameFromStream(stream);
        }

        private class FileNameInfo
        {
            public string Encrypted { get; set; }
            public string Original { get; set; }
        }
    }
}