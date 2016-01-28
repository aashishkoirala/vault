/*******************************************************************************************************************************
 * AK.Vault.SymmetricEncryptionParameters
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

using System.Security.Cryptography;

namespace AK.Vault
{
    /// <summary>
    /// Structure that holds parameters for symmetric encryption.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class SymmetricEncryptionParameters
    {
        /// <summary>
        /// The symmetric algorithm to use.
        /// </summary>
        public AlgorithmType Algorithm { get; set; }

        /// <summary>
        /// The padding mode to use.
        /// </summary>
        public PaddingMode Padding { get; set; }

        /// <summary>
        /// The block-cipher mode to use.
        /// </summary>
        public CipherMode Mode { get; set; }

        /// <summary>
        /// The encryption key.
        /// </summary>
        public byte[] Key { get; set; }
    }
}