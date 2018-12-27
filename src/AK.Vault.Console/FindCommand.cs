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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#endregion

namespace AK.Vault.Console
{
    /// <summary>
    /// ICommand instance for the "find" command; looks for a file path in the vault.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [CommandInfo(CommandName = "find", RequiresEncryptionKeyInput = false)]
    internal class FindCommand : CommandBase
    {
        private string fileToFind;
        private readonly FileNameManager fileNameManager;
        private readonly VaultOptions vaultConfiguration;
        private readonly IConfiguration configuration;

        public Action<string> FileFound { get; set; }

        public FindCommand(IConfiguration configuration, FileNameManager fileNameManager, 
            IOptionsMonitor<VaultOptions> vaultConfiguration, 
            FileEncryptorFactory fileEncryptorFactory) : base(fileEncryptorFactory)
        {
            this.configuration = configuration;
            this.fileNameManager = fileNameManager;
            this.vaultConfiguration = vaultConfiguration.CurrentValue;
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
            var encryptedFileLocation = this.vaultConfiguration.Vaults
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