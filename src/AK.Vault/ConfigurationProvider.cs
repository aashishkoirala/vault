/*******************************************************************************************************************************
 * AK.Vault.ConfigurationProvider
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

using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Reflection;

#endregion

namespace AK.Vault
{
    /// <summary>
    /// Provides access to configuration settings.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Folder where files are encrypted to.
        /// </summary>
        string EncryptedFileLocation { get; }

        /// <summary>
        /// Folder where files are decrypted to.
        /// </summary>
        string DecryptedFileLocation { get; }
    }

    [Export(typeof (IConfigurationProvider)), PartCreationPolicy(CreationPolicy.Shared)]
    internal class ConfigurationProvider : IConfigurationProvider
    {
        public ConfigurationProvider()
        {
            this.EncryptedFileLocation = SanitizePath(ConfigurationManager.AppSettings["EncryptedFileLocation"]);
            this.DecryptedFileLocation = SanitizePath(ConfigurationManager.AppSettings["DecryptedFileLocation"]);
        }

        public string EncryptedFileLocation { get; private set; }
        public string DecryptedFileLocation { get; private set; }

        private static string SanitizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path");

            if (!Path.IsPathRooted(path))
            {
                var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (dir == null)
                    throw new Exception("Unexpected error- cannot get executing assembly location directory");

                path = Path.Combine(dir, path);
            }

            path = Path.GetFullPath(path);
            if (!Directory.Exists(path))
                throw new Exception(string.Format("Folder \"{0}\" does not exist.", path));

            return path;
        }
    }
}