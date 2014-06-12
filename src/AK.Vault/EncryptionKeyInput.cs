/*******************************************************************************************************************************
 * AK.Vault.EncryptionKeyInput
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
    /// Structure that acts as seed for encryption keys.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class EncryptionKeyInput
    {
        /// <summary>
        /// Full name of the encryptor.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Date of birth of the encryptor.
        /// </summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// SSN of the encryptor.
        /// </summary>
        public string SocialSecurityNumber { get; set; }

        /// <summary>
        /// A secret pass-phrase used by the encryptor.
        /// </summary>
        public string PassPhrase { get; set; }
    }
}