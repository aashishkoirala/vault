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
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AK.Vault
{
    /// <summary>
    /// Handles generation/transformation of filenames during encryption/decryption.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class FileNameManager
    {
        /// <summary>
        /// Generates a name for the encrypted file based on the original file name.
        /// </summary>
        /// <param name="unencryptedFileName">Original file name.</param>
        /// <returns>Encrypted file name.</returns>
        public string GenerateNameForEncryptedFile(string unencryptedFileName)
        {
            byte[] hash;
            using (var sha1 = SHA256.Create())
            {
                var hashSource = Encoding.UTF8.GetBytes(unencryptedFileName.ToLower());
                hash = sha1.ComputeHash(hashSource);
            }
            return $"{BitConverter.ToString(hash).Replace("-", "")}.vault";
        }

        /// <summary>
        /// Encodes and writes the original file name to the given stream (for use
        /// during encryption).
        /// </summary>
        /// <param name="fileName">Original file name.</param>
        /// <param name="stream">Stream to write to.</param>
        public void WriteOriginalFileNameToStream(string fileName, Stream stream)
        {
            var data = Encoding.UTF8.GetBytes(fileName);
            var base64EncodedString = Convert.ToBase64String(data);
            data = Encoding.ASCII.GetBytes(base64EncodedString);

            var lengthData = BitConverter.GetBytes(data.Length);
            stream.Write(lengthData, 0, 4);
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Decodes and reads the original file name from a stream (for use during
        /// decryption or traversal).
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <returns>Original file name.</returns>
        public string ReadOriginalFileNameFromStream(Stream stream)
        {
            var lengthData = new byte[4];
            stream.Read(lengthData, 0, lengthData.Length);

            var data = new byte[BitConverter.ToInt32(lengthData, 0)];
            stream.Read(data, 0, data.Length);

            var base64EncodedString = Encoding.ASCII.GetString(data);
            data = Convert.FromBase64String(base64EncodedString);

            return Encoding.UTF8.GetString(data);
        }
    }
}
