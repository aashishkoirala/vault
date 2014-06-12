/*******************************************************************************************************************************
 * AK.Vault.Console.LaunchCommand
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

#endregion

namespace AK.Vault.Console
{
    /// <summary>
    /// ICommand instance for the "launch" command; opens the interactive UI.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [Export(typeof (ICommand)), PartCreationPolicy(CreationPolicy.NonShared)]
    [CommandInfo("launch", true)]
    internal class LaunchCommand : CommandBase
    {
        private Launcher launcher;

        public override bool AssignParameters(string[] args)
        {
            return true;
        }

        public override void AssignEncryptionKeyInput(EncryptionKeyInput encryptionKeyInput)
        {
            base.AssignEncryptionKeyInput(encryptionKeyInput);
            this.launcher = new Launcher(this.FileEncryptor, Factory.ListGenerator);
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
            try
            {
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