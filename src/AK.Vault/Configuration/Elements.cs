/*******************************************************************************************************************************
 * AK.Vault.Configuration.Elements
 * Copyright © 2016 Aashish Koirala <http://aashishkoirala.github.io>
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

using System.Configuration;

namespace AK.Vault.Configuration
{
    public class VaultSection : ConfigurationSection
    {
        [ConfigurationProperty("vaultConfiguration")]
        public VaultConfigurationElement VaultConfiguration
        {
            get { return (VaultConfigurationElement) this["vaultConfiguration"]; }
            set { this["vaultConfiguration"] = value; }
        }
    }

    public class VaultConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("vaults")]
        [ConfigurationCollection(typeof(VaultElement), AddItemName = "vault")]
        public VaultCollection Vaults
        {
            get { return (VaultCollection) this["vaults"]; }
            set { this["vaults"] = value; }
        }
    }

    public class VaultCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new VaultElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((VaultElement) element).Name;
        }        
    }

    public class VaultElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true)]
        public string Name
        {
            get { return this["name"].ToString(); }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("encryptedFileLocation")]
        public string EncryptedFileLocation
        {
            get { return this["encryptedFileLocation"].ToString(); }
            set { this["encryptedFileLocation"] = value; }
        }

        [ConfigurationProperty("decryptedFileLocation")]
        public string DecryptedFileLocation
        {
            get { return this["decryptedFileLocation"].ToString(); }
            set { this["decryptedFileLocation"] = value; }
        }
    }
}