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

using Microsoft.Extensions.Options;

namespace AK.Vault
{
    /// <summary>
    /// Creates FileEncryptor instances.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class FileEncryptorFactory
    {
        private readonly EncryptionKeyGenerator _encryptionKeyGenerator;
        private readonly VaultOptions _vaultOptions;
        private readonly FileNameManager _fileNameManager;
        private readonly SymmetricEncryptor _symmetricEncryptor;

        public FileEncryptorFactory(EncryptionKeyGenerator encryptionKeyGenerator,
            IOptionsMonitor<VaultOptions> vaultOptionsMonitor,
            FileNameManager fileNameManager, SymmetricEncryptor symmetricEncryptor)
        {
            _encryptionKeyGenerator = encryptionKeyGenerator;
            _vaultOptions = vaultOptionsMonitor.CurrentValue;
            _fileNameManager = fileNameManager;
            _symmetricEncryptor = symmetricEncryptor;
        }

        /// <summary>
        /// Creates a FileEncryptor based on the given encryption key input structure.
        /// </summary>
        /// <param name="encryptionKeyInput">Encryption key input structure.</param>
        /// <param name="vaultName">Vault name.</param>
        /// <returns>FileEncryptor instance.</returns>
        public FileEncryptor Create(EncryptionKeyInput encryptionKeyInput, string vaultName) =>
            new FileEncryptor(_symmetricEncryptor, _encryptionKeyGenerator,
                _vaultOptions, _fileNameManager, encryptionKeyInput, vaultName);
    }
}
