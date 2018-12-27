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
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace AK.Vault
{
    /// <summary>
    /// Performs symmetric encryption/decryption.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class SymmetricEncryptor
    {
        private static readonly IDictionary<AlgorithmType, Func<SymmetricAlgorithm>>
            AlgorithmMap = new Dictionary<AlgorithmType, Func<SymmetricAlgorithm>>
                {
                    {AlgorithmType.Aes, Aes.Create},
                    {AlgorithmType.Des, DES.Create},
                    {AlgorithmType.Rc2, RC2.Create},
                    {AlgorithmType.Rijndael, Rijndael.Create},
                    {AlgorithmType.TripleDes, TripleDES.Create}
                };

        /// <summary>
        /// Reads data from one stream, encrypts it, and writes the encrypted data to another stream.
        /// </summary>
        /// <param name="parameters">Encryption parameters.</param>
        /// <param name="inStream">Stream to read plain data from.</param>
        /// <param name="outStream">Stream to write encrypted data to.</param>
        public void Encrypt(SymmetricEncryptionParameters parameters, Stream inStream, Stream outStream)
        {
            using (var algorithm = AlgorithmMap[parameters.Algorithm]())
            {
                var iv = GenerateIv(outStream, algorithm.IV.Length);

                using (var cryptor = algorithm.CreateEncryptor(parameters.Key, iv))
                using (var cryptoStream = new CryptoStream(outStream, cryptor, CryptoStreamMode.Write))
                    inStream.CopyTo(cryptoStream);
            }
        }

        /// <summary>
        /// Reads data from one stream, decrypts it, and writes the decrypted data to another stream.
        /// </summary>
        /// <param name="parameters">Decryption parameters.</param>
        /// <param name="inStream">Stream to read encrypted data from.</param>
        /// <param name="outStream">Stream to write decrypted data to.</param>
        public void Decrypt(SymmetricEncryptionParameters parameters, Stream inStream, Stream outStream)
        {
            using (var algorithm = AlgorithmMap[parameters.Algorithm]())
            {
                var iv = ExtractIv(inStream, algorithm.IV.Length);

                using (var cryptor = algorithm.CreateDecryptor(parameters.Key, iv))
                using (var cryptoStream = new CryptoStream(outStream, cryptor, CryptoStreamMode.Write))
                    inStream.CopyTo(cryptoStream);
            }
        }

        /// <summary>
        /// Encrypts the given data.
        /// </summary>
        /// <param name="parameters">Encryption parameters.</param>
        /// <param name="inData">Plain data to encrypt.</param>
        /// <returns>Encrypted data.</returns>
        public byte[] Encrypt(SymmetricEncryptionParameters parameters, byte[] inData)
        {
            using (MemoryStream inDataStream = new MemoryStream(inData), outDataStream = new MemoryStream())
            {
                Encrypt(parameters, inDataStream, outDataStream);

                return outDataStream.ToArray();
            }
        }

        /// <summary>
        /// Decrypts the given data.
        /// </summary>
        /// <param name="parameters">Decryption parameters.</param>
        /// <param name="inData">Encrypted data to decrypt.</param>
        /// <returns>Decrypted data.</returns>
        public byte[] Decrypt(SymmetricEncryptionParameters parameters, byte[] inData)
        {
            using (MemoryStream inDataStream = new MemoryStream(inData), outDataStream = new MemoryStream())
            {
                Decrypt(parameters, inDataStream, outDataStream);

                return outDataStream.ToArray();
            }
        }

        private static byte[] GenerateIv(Stream outStream, int length)
        {
            var iv = new byte[length];
            using (var generator = RandomNumberGenerator.Create())
                generator.GetNonZeroBytes(iv);
            outStream.Write(iv, 0, iv.Length);
            return iv;
        }

        private static byte[] ExtractIv(Stream inStream, int length)
        {
            var iv = new byte[length];
            inStream.Read(iv, 0, iv.Length);
            return iv;
        }
    }
}
