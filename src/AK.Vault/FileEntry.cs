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

namespace AK.Vault
{
    /// <summary>
    /// Represents an encrypted file in the vault tree.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class FileEntry
    {
        /// <summary>
        /// Full path of encrypted file.
        /// </summary>
        public string EncryptedFullPath { get; set; }

        /// <summary>
        /// Full path of original file.
        /// </summary>
        public string OriginalFullPath { get; set; }

        /// <summary>
        /// Display name of the file (i.e. just the name of the original file).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// FolderEntry representing the folder the file belongs to.
        /// </summary>
        public FolderEntry Parent { get; set; }
    }
}
