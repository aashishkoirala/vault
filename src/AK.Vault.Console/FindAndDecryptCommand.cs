/*******************************************************************************************************************************
 * AK.Vault.Console.FindAndDecryptCommand
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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

#endregion

namespace AK.Vault.Console
{
    /// <summary>
    /// ICommand instance for the "find-and-decrypt" command; performs find-and-decryption.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ICommand)), PartCreationPolicy(CreationPolicy.NonShared)]
    [CommandInfo("fd", true)]
    internal class FindAndDecryptCommand : CommandBase
    {
        private string[] filesToFindAndDecrypt;
        private EncryptionKeyInput localEncryptionKeyInput;

        public override bool AssignParameters(string[] args)
        {
            this.filesToFindAndDecrypt = args;
            return true;
        }

        public override void AssignEncryptionKeyInput(EncryptionKeyInput encryptionKeyInput)
        {
            base.AssignEncryptionKeyInput(encryptionKeyInput);
            this.localEncryptionKeyInput = encryptionKeyInput;
        }

        protected override void WriteHeader()
        {
            Screen.Print();
            Screen.Print("Starting find-and-decryption of the following:");
            foreach (var filePattern in this.filesToFindAndDecrypt)
                Screen.Print("{0}{1}", '\t', filePattern);
            Screen.Print();
        }

        protected override bool ExecuteCommand(ICollection<Exception> exceptions)
        {
            var filesToDecrypt = new List<string>();

            foreach (var fileToFind in this.filesToFindAndDecrypt)
            {
                var findCommand = new FindCommand
                {
                    VaultName = this.VaultName,
                    FileFound = filesToDecrypt.Add
                };

                findCommand.AssignParameters(new[] {fileToFind});
                var findResult = findCommand.Execute();
                if (!findResult) return false;
            }

            if (!filesToDecrypt.Any()) return false;

            filesToDecrypt = filesToDecrypt.Select(Path.GetFileName).ToList();

            var decryptCommand = new DecryptCommand {VaultName = this.VaultName};
            decryptCommand.AssignParameters(filesToDecrypt.ToArray());
            decryptCommand.AssignEncryptionKeyInput(this.localEncryptionKeyInput);
            return decryptCommand.Execute();
        }
    }
}