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

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AK.Vault.Console
{
    /// <summary>
    /// ICommand instance for the "decrypt" command; performs decryption.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [CommandInfo(CommandName ="decrypt", RequiresEncryptionKeyInput = true)]
    internal class DecryptCommand : CommandBase
    {
        private readonly string[] _filePatterns;

        public DecryptCommand(FileEncryptorFactory fileEncryptorFactory,
            IOptionsMonitor<VaultOptions> vaultOptionsMonitor) : base(fileEncryptorFactory)
        {
            var options = vaultOptionsMonitor.CurrentValue;
            if (string.IsNullOrWhiteSpace(options.Target)) return;
            _filePatterns = options.Target.Split(';');
            IsValid = true;
        }

        protected override void WriteHeader()
        {
            Screen.Print();
            Screen.Print("Starting decryption of the following:");
            foreach (var filePattern in _filePatterns)
                Screen.Print("{0}{1}", '\t', filePattern);
            Screen.Print();
        }

        protected override bool ExecuteCommand(ICollection<Exception> exceptions)
        {
            var results = _fileEncryptor.Decrypt(_filePatterns).ToArray();

            var totalCount = results.Length;
            var doneCount = results.Count(x => x.IsDone);

            Screen.Print();
            Screen.Print("{0} of {1} files decrypted successfully.", doneCount, totalCount);

            foreach (var result in results.Where(x => !x.IsDone && x.Exception != null))
            {
                var message = $"Error decrypting {result.EncryptedFilePath} - {result.Exception.Message}.";

                exceptions.Add(new Exception(message, result.Exception));
            }

            return totalCount == doneCount;
        }
    }
}
