/*******************************************************************************************************************************
 * AK.Vault.FileEncryptionResult
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

using System;

namespace AK.Vault
{
    /// <summary>
    /// Represents the result of encryption or decryption on one file.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class FileEncryptionResult
    {
        /// <summary>
        /// Was the encryption/decryption successful?
        /// </summary>
        public bool IsDone { get; set; }

        /// <summary>
        /// Exception, if one was encountred, NULL otherwise.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// The full file path before encryption (or after decryption).
        /// </summary>
        public string UnencryptedFilePath { get; set; }

        /// <summary>
        /// The full path location of the encrypted file.
        /// </summary>
        public string EncryptedFilePath { get; set; }
    }
}