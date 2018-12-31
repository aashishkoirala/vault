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
using Microsoft.Extensions.Options;

namespace AK.Vault.Console
{
    /// <summary>
    /// ICommand instance for the "report" command; generates a tabular report of all files in the vault.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [CommandInfo(CommandName = "report", RequiresEncryptionKeyInput = false)]
    internal class ReportCommand : CommandBase
    {
        protected override bool PromptBeforeStart => false;

        private readonly VaultOptions _vaultOptions;
        private readonly FileNameManager _fileNameManager;

        public ReportCommand(IOptionsMonitor<VaultOptions> vaultOptionsMonitor,
            FileNameManager fileNameManager, FileEncryptorFactory fileEncryptorFactory) : base(fileEncryptorFactory, true)
        {
            _vaultOptions = vaultOptionsMonitor.CurrentValue;
            _fileNameManager = fileNameManager;
        }

        protected override bool ExecuteCommand(ICollection<Exception> exceptions)
        {
            var encryptedFileLocation = _vaultOptions.Vaults
                .Single(x => x.Name == VaultName).EncryptedFileLocation;

            Screen.Print("Encrypted Name\tOriginal Name");

            foreach (var file in Directory.GetFiles(encryptedFileLocation, "*.vault"))
            {
                string originalName;
                using (var stream = File.OpenRead(file))
                {
                    originalName = _fileNameManager.ReadOriginalFileNameFromStream(stream);
                }

                Screen.Print("{0}\t{1}", file, originalName);
            }

            Screen.Print();

            return true;
        }
    }
}
