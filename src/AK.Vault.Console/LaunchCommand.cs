﻿/*******************************************************************************************************************************
 * AK.Vault.Console.LaunchCommand
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

#endregion

namespace AK.Vault.Console
{
    /// <summary>
    /// ICommand instance for the "launch" command; opens the interactive UI.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [CommandInfo(CommandName = "launch", RequiresEncryptionKeyInput = true)]
    internal class LaunchCommand : CommandBase
    {
        private Launcher launcher;
        private readonly IListGenerator listGenerator;

        public LaunchCommand(IListGenerator listGenerator, IFileEncryptorFactory fileEncryptorFactory) : 
            base(fileEncryptorFactory)
        {
            this.listGenerator = listGenerator;
        }

        public override bool ProcessParameters() => true;

        public override void AssignEncryptionKeyInput(EncryptionKeyInput encryptionKeyInput)
        {
            base.AssignEncryptionKeyInput(encryptionKeyInput);
            this.launcher = new Launcher(this.FileEncryptor, this.listGenerator, this.VaultName);
        }

        protected override bool PromptAfterEnd => false;

        protected override bool PromptBeforeStart => false;

        protected override bool ExecuteCommand(ICollection<Exception> exceptions)
        {
            try
            {
                Screen.Print();
                Screen.Print("Launching application...");
                this.launcher.Run();
                return true;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                return false;
            }
        }
    }
}