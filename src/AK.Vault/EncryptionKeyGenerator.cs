/*******************************************************************************************************************************
 * AK.Vault.EncryptionKeyGenerator
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

using System.ComponentModel.Composition;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace AK.Vault
{
    /// <summary>
    /// Generates symmetric encryption key based on the given input structure.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal interface IEncryptionKeyGenerator
    {
        /// <summary>
        /// Generates a symmetric encryption key.
        /// </summary>
        /// <param name="input">Input structure that acts as source of info or seed for the key.</param>
        /// <returns>Encryption key as byte array.</returns>
        byte[] Generate(EncryptionKeyInput input);
    }

    [Export(typeof (IEncryptionKeyGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    internal class EncryptionKeyGenerator : IEncryptionKeyGenerator
    {
        public byte[] Generate(EncryptionKeyInput input)
        {
            var seed = string.Format(
                "{0}|{1}|{2}|{3}",
                input.FullName, input.DateOfBirth, input.SocialSecurityNumber, input.PassPhrase);

            var seedBinary = Encoding.ASCII.GetBytes(seed);

            using (var sha1 = SHA256.Create())
                return sha1.ComputeHash(seedBinary);
        }
    }
}