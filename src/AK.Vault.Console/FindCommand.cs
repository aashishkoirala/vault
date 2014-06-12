/*******************************************************************************************************************************
 * AK.Vault.Console.FindCommand
 * Copyright © 2014 Aashish Koirala <http://aashishkoirala.github.io>
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
    /// ICommand instance for the "find" command; looks for a file path in the vault.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof(ICommand)), PartCreationPolicy(CreationPolicy.NonShared)]
    [CommandInfo("find", false)]
    internal class FindCommand : CommandBase
    {
        private string fileToFind;

        public override bool AssignParameters(string[] args)
        {
            this.fileToFind = args.FirstOrDefault();
            return !string.IsNullOrWhiteSpace(this.fileToFind);
        }

        protected override bool PromptAfterEnd
        {
            get { return false; }
        }

        protected override bool PromptBeforeStart
        {
            get { return false; }
        }

        protected override bool ExecuteCommand(ICollection<Exception> exceptions)
        {
            var encryptedName = Factory.FileNameManager.GenerateNameForEncryptedFile(this.fileToFind);
            var fullPath = Path.Combine(Factory.ConfigurationProvider.EncryptedFileLocation, encryptedName);

            Screen.Print(File.Exists(fullPath) ? fullPath : "Not found.");
            return true;
        }
    }
}