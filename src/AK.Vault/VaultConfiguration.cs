/*******************************************************************************************************************************
 * AK.Vault.Configuration.VaultConfiguration
 * Copyright © 2016 Aashish Koirala <http://aashishkoirala.github.io>
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

using System.Collections.Generic;

namespace AK.Vault.Configuration
{
    /// <summary>
    /// Contains information on Vault configuration.
    /// </summary>
    public class VaultConfiguration
    {
        /// <summary>
        /// List of configured vaults.
        /// </summary>
        public ICollection<Vault> Vaults { get; } = new List<Vault>();

        public class Vault
        {
            /// <summary>
            /// Name of the vault.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Encrypted file location.
            /// </summary>
            public string EncryptedFileLocation { get; set; }

            /// <summary>
            /// Decrypted file location.
            /// </summary>
            public string DecryptedFileLocation { get; set; }
        }
    }
}