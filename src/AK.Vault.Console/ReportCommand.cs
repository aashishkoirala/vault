/*******************************************************************************************************************************
 * AK.Vault.Console.ReportCommand
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
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;

#endregion

namespace AK.Vault.Console
{
    /// <summary>
    /// ICommand instance for the "report" command; generates a tabular report of all files in the vault.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ICommand))]
    [CommandInfo(CommandName = "report", RequiresEncryptionKeyInput = false)]
    internal class ReportCommand : CommandBase
    {
        protected override bool PromptBeforeStart => false;

        private readonly IConfigurationProvider configurationProvider;
        private readonly IFileNameManager fileNameManager;

        [ImportingConstructor]
        public ReportCommand(IConfigurationProvider configurationProvider, IFileNameManager fileNameManager, 
            IFileEncryptorFactory fileEncryptorFactory) : base(fileEncryptorFactory)
        {
            this.configurationProvider = configurationProvider;
            this.fileNameManager = fileNameManager;
        }

        public override bool AssignParameters(string[] args)
        {
            return true;
        }

        protected override bool ExecuteCommand(ICollection<Exception> exceptions)
        {
            var encryptedFileLocation = this.configurationProvider.Configuration.Vaults
                .Single(x => x.Name == this.VaultName).EncryptedFileLocation;

            Screen.Print("Encrypted Name\tOriginal Name");

            foreach (var file in Directory.GetFiles(encryptedFileLocation, "*.vault"))
            {
                string originalName;
                using (var stream = File.OpenRead(file))
                {
                    originalName = this.fileNameManager.ReadOriginalFileNameFromStream(stream);
                }

                Screen.Print("{0}\t{1}", file, originalName);
            }

            Screen.Print();

            return true;
        }
    }
}