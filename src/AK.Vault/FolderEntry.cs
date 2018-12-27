﻿/*******************************************************************************************************************************
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

using System.Collections.Generic;

namespace AK.Vault
{
    /// <summary>
    /// Represents a folder with encrypted files in the vault tree.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class FolderEntry
    {
        /// <summary>
        /// Full path of the original folder.
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Display name of the folder (i.e. just the folder name).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// FolderEntry instance representing the folder this belongs to,
        /// NULL for the root folder.
        /// </summary>
        public FolderEntry Parent { get; set; }

        /// <summary>
        /// List of FolderEntry objects representing this folder's child folders.
        /// </summary>
        public ICollection<FolderEntry> Folders { get; } = new List<FolderEntry>();

        /// <summary>
        /// List of FileEntry objects representing files in this folder.
        /// </summary>
        public ICollection<FileEntry> Files { get; } = new List<FileEntry>();
    }
}
