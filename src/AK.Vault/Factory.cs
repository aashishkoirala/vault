/*******************************************************************************************************************************
 * AK.Vault.Factory
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
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

#endregion

namespace AK.Vault
{
    /// <summary>
    /// Maintains lifecycles of and provides access to various components within
    /// the library.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public static class Factory
    {
        private static readonly CompositionContainer CompositionContainer;
        private static readonly Container AppContainer;

        static Factory()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyCatalog = new AssemblyCatalog(assembly);

            CompositionContainer = new CompositionContainer(assemblyCatalog);
            AppContainer = CompositionContainer.GetExportedValue<Container>();
        }

        /// <summary>
        /// Gets the IListGenerator instance.
        /// </summary>
        public static IListGenerator ListGenerator
        {
            get { return AppContainer.ListGenerator; }
        }

        /// <summary>
        /// Gets thte IFileNameManager instance.
        /// </summary>
        public static IFileNameManager FileNameManager
        {
            get { return AppContainer.FileNameManager; }
        }

        /// <summary>
        /// Gets the IConfigurationProvider instance.
        /// </summary>
        public static IConfigurationProvider ConfigurationProvider
        {
            get { return AppContainer.ConfigurationProvider; }
        }

        /// <summary>
        /// Creates an IFileEncryptor based on the given encryption key input structure.
        /// </summary>
        /// <param name="encryptionKeyInput">Encryption key input structure.</param>
        /// <returns>IFileEncryptor instance.</returns>
        public static IFileEncryptor CreateFileEncryptor(EncryptionKeyInput encryptionKeyInput)
        {
            return new FileEncryptor(
                AppContainer.SymmetricEncryptor,
                AppContainer.EncryptionKeyGenerator,
                AppContainer.ConfigurationProvider,
                AppContainer.FileNameManager,
                encryptionKeyInput);
        }

        [Export, PartCreationPolicy(CreationPolicy.Shared)]
        private class Container
        {
            // ReSharper disable MemberHidesStaticFromOuterClass

            [Import]
            public IEncryptionKeyGenerator EncryptionKeyGenerator { get; private set; }

            [Import]
            public IConfigurationProvider ConfigurationProvider { get; private set; }

            [Import]
            public IFileNameManager FileNameManager { get; private set; }

            [Import]
            public IListGenerator ListGenerator { get; private set; }

            [Import]
            public ISymmetricEncryptor SymmetricEncryptor { get; private set; }

            // ReSharper restore MemberHidesStaticFromOuterClass
        }
    }
}