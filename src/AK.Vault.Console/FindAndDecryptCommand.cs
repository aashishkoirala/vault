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
    /// ICommand instance for the "find-and-decrypt" command; performs find-and-decryption.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ICommand))]
    [CommandInfo(CommandName = "fd", RequiresEncryptionKeyInput = true)]
    internal class FindAndDecryptCommand : CommandBase
    {
        private string[] filesToFindAndDecrypt;
        private EncryptionKeyInput localEncryptionKeyInput;
        private readonly IConfiguration configuration;
        private readonly FindCommand findCommand;
        private readonly DecryptCommand decryptCommand;

        [ImportingConstructor]
        public FindAndDecryptCommand(IFileEncryptorFactory fileEncryptorFactory,
            IConfiguration configuration, FindCommand findCommand, DecryptCommand decryptCommand) : 
            base(fileEncryptorFactory)
        {
            this.configuration = configuration;
            this.findCommand = findCommand;
            this.decryptCommand = decryptCommand;
        }

        public override bool ProcessParameters()
        {
            this.filesToFindAndDecrypt = this.configuration["target"].Split(';');
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

            this.findCommand.VaultName = this.VaultName;
            this.findCommand.FileFound = filesToDecrypt.Add;
            foreach (var fileToFind in this.filesToFindAndDecrypt)
            {
                //this.findCommand.AssignParameters(new[] {fileToFind});
                var findResult = this.findCommand.Execute();
                if (!findResult) return false;
            }

            if (!filesToDecrypt.Any()) return false;

            filesToDecrypt = filesToDecrypt.Select(Path.GetFileName).ToList();

            this.decryptCommand.VaultName = this.VaultName;
            //this.decryptCommand.AssignParameters(filesToDecrypt.ToArray());
            this.decryptCommand.AssignEncryptionKeyInput(this.localEncryptionKeyInput);
            return this.decryptCommand.Execute();
        }
    }
}