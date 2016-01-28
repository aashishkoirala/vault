/*******************************************************************************************************************************
 * AK.Vault.ConfigurationProvider
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

using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

#endregion

namespace AK.Vault.Configuration
{
    /// <summary>
    /// Provides access to configuration settings.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Vault configuration structure.
        /// </summary>
        VaultConfiguration Configuration { get; }
    }

    [Export(typeof (IConfigurationProvider)), PartCreationPolicy(CreationPolicy.Shared)]
    internal class ConfigurationProvider : IConfigurationProvider
    {
        public ConfigurationProvider()
        {
            this.Configuration = LoadConfiguration();
        }

        public VaultConfiguration Configuration { get; }

        private static VaultConfiguration LoadConfiguration()
        {
            var systemConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var vaultSection = (VaultSection) systemConfiguration.GetSection("ak.vault");

            var configuration = new VaultConfiguration();

            var vaults =
                from VaultElement vaultElement in vaultSection.VaultConfiguration.Vaults
                select new VaultConfiguration.Vault
                {
                    Name = vaultElement.Name,
                    EncryptedFileLocation = SanitizePath(vaultElement.EncryptedFileLocation),
                    DecryptedFileLocation = SanitizePath(vaultElement.DecryptedFileLocation)
                };

            foreach (var vault in vaults.OrderBy(x => x.Name).Take(36)) configuration.Vaults.Add(vault);

            return configuration;
        }

        private static string SanitizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            if (!Path.IsPathRooted(path))
            {
                var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (dir == null)
                    throw new Exception("Unexpected error- cannot get executing assembly location directory");

                path = Path.Combine(dir, path);
            }

            path = Path.GetFullPath(path);
            if (!Directory.Exists(path))
                throw new Exception($"Folder \"{path}\" does not exist.");

            return path;
        }
    }
}