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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            FileNameManager fileNameManager, FileEncryptorFactory fileEncryptorFactory,
            ConsoleWriter console, ILogger<ReportCommand> logger) :
            base(fileEncryptorFactory, console, logger, true)
        {
            _vaultOptions = vaultOptionsMonitor.CurrentValue;
            _fileNameManager = fileNameManager;
        }

        protected override async Task<bool> ExecuteCommand(ICollection<Exception> exceptions)
        {
            var encryptedFileLocation = _vaultOptions.Vaults
                .Single(x => x.Name == VaultName).EncryptedFileLocation;

            _console.Info("Encrypted Name\tOriginal Name");

            var tasks = new Dictionary<string, Task<string>>();
            foreach (var file in Directory.GetFiles(encryptedFileLocation, "*.vault"))
            {
                using (var stream = File.OpenRead(file))
                {
                    tasks[file] = _fileNameManager.ReadOriginalFileNameFromStream(stream);
                }
            }

            foreach (var item in tasks)
            {
                var originalName = await item.Value;
                _console.Info($"{item.Key}\t{originalName}");
            }
            _console.Blank();

            return true;
        }
    }
}