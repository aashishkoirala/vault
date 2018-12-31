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
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace AK.Vault.Console
{
    /// <summary>
    /// ICommand instance for the "find" command; looks for a file path in the vault.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [CommandInfo(CommandName = "find", RequiresEncryptionKeyInput = false)]
    internal class FindCommand : CommandBase
    {
        private readonly string _fileToFind;
        private readonly FileNameManager _fileNameManager;
        private readonly VaultOptions _vaultOptions;
        private readonly IConfiguration _configuration;

        public Action<string> FileFound { get; set; }

        public FindCommand(IConfiguration configuration, FileNameManager fileNameManager, 
            IOptionsMonitor<VaultOptions> vaultOptionsMonitor, 
            FileEncryptorFactory fileEncryptorFactory) : base(fileEncryptorFactory)
        {
            _configuration = configuration;
            _fileNameManager = fileNameManager;
            _vaultOptions = vaultOptionsMonitor.CurrentValue;
            _fileToFind = _vaultOptions.Target;
            IsValid = !string.IsNullOrWhiteSpace(_fileToFind);
        }

        protected override bool PromptAfterEnd => false;

        protected override bool PromptBeforeStart => false;

        protected override bool ExecuteCommand(ICollection<Exception> exceptions)
        {
            var encryptedFileLocation = _vaultOptions.Vaults
                .Single(x => x.Name == VaultName).EncryptedFileLocation;

            var encryptedName = _fileNameManager.GenerateNameForEncryptedFile(_fileToFind);
            var fullPath = Path.Combine(encryptedFileLocation, encryptedName);

            fullPath = File.Exists(fullPath) ? fullPath : null;
            Screen.Print(fullPath ?? "Not found.");

            if (fullPath != null) FileFound?.Invoke(fullPath);

            return true;
        }
    }
}
