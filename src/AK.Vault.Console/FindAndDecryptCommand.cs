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

namespace AK.Vault.Console
{
    /// <summary>
    /// ICommand instance for the "find-and-decrypt" command; performs find-and-decryption.
    /// </summary>
    /// <author>Aashish Koirala</author>
    /// TODO: Fix this command as it doesn't work afer refactoring.
    [CommandInfo(CommandName = "fd", RequiresEncryptionKeyInput = true)]
    internal class FindAndDecryptCommand : CommandBase
    {
        private readonly string[] _filesToFindAndDecrypt;
        private readonly FindCommand _findCommand;
        private readonly DecryptCommand _decryptCommand;
        private EncryptionKeyInput _localEncryptionKeyInput;

        public FindAndDecryptCommand(FileEncryptorFactory fileEncryptorFactory,
            IOptionsMonitor<VaultOptions> vaultOptionsMonitor,
            FindCommand findCommand, DecryptCommand decryptCommand,
            ConsoleWriter console, ILogger<FindAndDecryptCommand> logger) :
            base(fileEncryptorFactory, console, logger)
        {
            _findCommand = findCommand;
            _decryptCommand = decryptCommand;
            var options = vaultOptionsMonitor.CurrentValue;
            if (string.IsNullOrWhiteSpace(options.Target)) return;
            _filesToFindAndDecrypt = options.Target.Split(';');
            IsValid = true;
        }

        public override void AssignEncryptionKeyInput(EncryptionKeyInput encryptionKeyInput)
        {
            base.AssignEncryptionKeyInput(encryptionKeyInput);
            _localEncryptionKeyInput = encryptionKeyInput;
        }

        protected override void WriteHeader()
        {
            _console.Blank();
            _console.Info("Starting find-and-decryption of the following:");
            foreach (var filePattern in _filesToFindAndDecrypt) _console.Info($"{filePattern}\t");
            _console.Blank();
        }

        protected override bool ExecuteCommand(ICollection<Exception> exceptions)
        {
            var filesToDecrypt = new List<string>();

            _findCommand.VaultName = VaultName;
            _findCommand.FileFound = filesToDecrypt.Add;
            foreach (var fileToFind in _filesToFindAndDecrypt)
            {
                //findCommand.AssignParameters(new[] {fileToFind});
                var findResult = _findCommand.Execute();
                if (!findResult) return false;
            }

            if (!filesToDecrypt.Any()) return false;

            filesToDecrypt = filesToDecrypt.Select(Path.GetFileName).ToList();

            _decryptCommand.VaultName = VaultName;
            //decryptCommand.AssignParameters(filesToDecrypt.ToArray());
            _decryptCommand.AssignEncryptionKeyInput(_localEncryptionKeyInput);
            return _decryptCommand.Execute();
        }
    }
}