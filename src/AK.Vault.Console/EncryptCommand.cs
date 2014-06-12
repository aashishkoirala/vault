/*******************************************************************************************************************************
 * AK.Vault.Console.EncryptCommand
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
using System.Linq;

#endregion

namespace AK.Vault.Console
{
    /// <summary>
    /// ICommand instance for the "encrypt" command; performs encryption.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof(ICommand)), PartCreationPolicy(CreationPolicy.NonShared)]
    [CommandInfo("encrypt", true)]
    internal class EncryptCommand : CommandBase
    {
        private string[] filePatterns;

        protected override void WriteHeader()
        {
            Screen.Print();
            Screen.Print("Starting encryption of the following:");
            foreach (var filePattern in filePatterns)
                Screen.Print("{0}{1}", '\t', filePattern);
            Screen.Print();
        }

        public override bool AssignParameters(string[] args)
        {
            this.filePatterns = args;
            return true;
        }

        protected override bool ExecuteCommand(ICollection<Exception> exceptions)
        {
            var results = this.FileEncryptor.Encrypt(this.filePatterns).ToArray();

            var totalCount = results.Length;
            var doneCount = results.Count(x => x.IsDone);

            Screen.Print();
            Screen.Print("{0} of {1} files encrypted successfully.", doneCount, totalCount);

            foreach (var result in results.Where(x => !x.IsDone && x.Exception != null))
            {
                var message = string.Format("Error encrypting {0} - {1}.",
                                            result.UnencryptedFilePath, result.Exception.Message);

                exceptions.Add(new Exception(message, result.Exception));
            }

            return totalCount == doneCount;
        }
    }
}