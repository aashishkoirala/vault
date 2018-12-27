/*******************************************************************************************************************************
 * AK.Vault.FileEncryptorFactory
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

using Microsoft.Extensions.Options;

namespace AK.Vault
{
    /// <summary>
    /// Creates FileEncryptor instances.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class FileEncryptorFactory
    {
        private readonly EncryptionKeyGenerator encryptionKeyGenerator;
        private readonly VaultOptions vaultConfiguration;
        private readonly FileNameManager fileNameManager;
        private readonly SymmetricEncryptor symmetricEncryptor;

        public FileEncryptorFactory(EncryptionKeyGenerator encryptionKeyGenerator,
            IOptionsMonitor<VaultOptions> vaultConfiguration,
            FileNameManager fileNameManager, SymmetricEncryptor symmetricEncryptor)
        {
            this.encryptionKeyGenerator = encryptionKeyGenerator;
            this.vaultConfiguration = vaultConfiguration.CurrentValue;
            this.fileNameManager = fileNameManager;
            this.symmetricEncryptor = symmetricEncryptor;
        }

        /// <summary>
        /// Creates a FileEncryptor based on the given encryption key input structure.
        /// </summary>
        /// <param name="encryptionKeyInput">Encryption key input structure.</param>
        /// <param name="vaultName">Vault name.</param>
        /// <returns>FileEncryptor instance.</returns>
        public FileEncryptor Create(EncryptionKeyInput encryptionKeyInput, string vaultName) =>
            new FileEncryptor(this.symmetricEncryptor, this.encryptionKeyGenerator,
                this.vaultConfiguration, this.fileNameManager, encryptionKeyInput, vaultName);
    }
}