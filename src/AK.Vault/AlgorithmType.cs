/*******************************************************************************************************************************
 * AK.Vault.AlgorithmType
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

namespace AK.Vault
{
    /// <summary>
    /// Types of encryption algorithms.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal enum AlgorithmType
    {
        /// <summary>
        /// AES
        /// </summary>
        Aes,

        /// <summary>
        /// DES
        /// </summary>
        Des,

        /// <summary>
        /// RC2
        /// </summary>
        Rc2,

        /// <summary>
        /// Rijndael
        /// </summary>
        Rijndael,

        /// <summary>
        /// Triple DES
        /// </summary>
        TripleDes
    }
}