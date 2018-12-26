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

#region Namespace Imports

using System.Composition;
using AK.Vault.Configuration;
using Microsoft.Extensions.Options;

#endregion

namespace AK.Vault
{
    public interface IFileEncryptorFactory
    {
        IFileEncryptor Create(EncryptionKeyInput encryptionKeyInput, string vaultName);
    }

    /// <summary>
    /// Creates FileEncryptor instances.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof(IFileEncryptorFactory))]
    public class FileEncryptorFactory : IFileEncryptorFactory
    {
        private readonly IEncryptionKeyGenerator encryptionKeyGenerator;
        private readonly VaultConfiguration vaultConfiguration;
        private readonly IFileNameManager fileNameManager;
        private readonly ISymmetricEncryptor symmetricEncryptor;

        [ImportingConstructor]
        public FileEncryptorFactory(IEncryptionKeyGenerator encryptionKeyGenerator, 
            IOptionsMonitor<VaultConfiguration> vaultConfiguration,
            IFileNameManager fileNameManager, ISymmetricEncryptor symmetricEncryptor)
        {
            this.encryptionKeyGenerator = encryptionKeyGenerator;
            this.vaultConfiguration = vaultConfiguration.CurrentValue;
            this.fileNameManager = fileNameManager;
            this.symmetricEncryptor = symmetricEncryptor;
        }

        /// <summary>
        /// Creates an IFileEncryptor based on the given encryption key input structure.
        /// </summary>
        /// <param name="encryptionKeyInput">Encryption key input structure.</param>
        /// <param name="vaultName">Vault name.</param>
        /// <returns>IFileEncryptor instance.</returns>
        public IFileEncryptor Create(EncryptionKeyInput encryptionKeyInput, string vaultName) =>
            new FileEncryptor(this.symmetricEncryptor, this.encryptionKeyGenerator,
                this.vaultConfiguration, this.fileNameManager, encryptionKeyInput, vaultName);
    }
}