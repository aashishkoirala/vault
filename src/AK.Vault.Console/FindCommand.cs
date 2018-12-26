/*******************************************************************************************************************************
 * AK.Vault.Console.FindCommand
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

using AK.Vault.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;

#endregion

namespace AK.Vault.Console
{
    /// <summary>
    /// ICommand instance for the "find" command; looks for a file path in the vault.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof(ICommand))]
    [Export]
    [CommandInfo(CommandName = "find", RequiresEncryptionKeyInput = false)]
    internal class FindCommand : CommandBase
    {
        private string fileToFind;
        private readonly IFileNameManager fileNameManager;
        private readonly Configuration.IConfigurationProvider configurationProvider;
        private readonly IConfiguration configuration;

        public Action<string> FileFound { get; set; }

        [ImportingConstructor]
        public FindCommand(IConfiguration configuration, IFileNameManager fileNameManager, 
            Configuration.IConfigurationProvider configurationProvider, 
            IFileEncryptorFactory fileEncryptorFactory) : base(fileEncryptorFactory)
        {
            this.configuration = configuration;
            this.fileNameManager = fileNameManager;
            this.configurationProvider = configurationProvider;
        }

        public override bool ProcessParameters()
        {
            this.fileToFind = this.configuration["target"];
            return !string.IsNullOrWhiteSpace(this.fileToFind);
        }

        protected override bool PromptAfterEnd => false;

        protected override bool PromptBeforeStart => false;

        protected override bool ExecuteCommand(ICollection<Exception> exceptions)
        {
            var encryptedFileLocation = this.configurationProvider.Configuration.Vaults
                .Single(x => x.Name == this.VaultName).EncryptedFileLocation;

            var encryptedName = this.fileNameManager.GenerateNameForEncryptedFile(this.fileToFind);
            var fullPath = Path.Combine(encryptedFileLocation, encryptedName);

            fullPath = File.Exists(fullPath) ? fullPath : null;
            Screen.Print(fullPath ?? "Not found.");

            if (fullPath != null) this.FileFound?.Invoke(fullPath);

            return true;
        }
    }
}