﻿/*******************************************************************************************************************************
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
using System;
using System.Collections.Generic;

namespace AK.Vault.Console
{
    /// <summary>
    /// ICommand instance for the "launch" command; opens the interactive UI.
    /// </summary>
    /// <author>Aashish Koirala</author>
    [CommandInfo(CommandName = "launch", RequiresEncryptionKeyInput = true)]
    internal class LaunchCommand : CommandBase
    {
        private Launcher _launcher;
        private readonly ListGenerator _listGenerator;

        public LaunchCommand(ListGenerator listGenerator,
            FileEncryptorFactory fileEncryptorFactory,
            ConsoleWriter console, ILogger<LaunchCommand> logger) :
            base(fileEncryptorFactory, console, logger, true) => _listGenerator = listGenerator;

        public override void AssignEncryptionKeyInput(EncryptionKeyInput encryptionKeyInput)
        {
            base.AssignEncryptionKeyInput(encryptionKeyInput);
            _launcher = new Launcher(_fileEncryptor, _listGenerator, _console, VaultName, _logger);
        }

        protected override bool PromptAfterEnd => false;

        protected override bool PromptBeforeStart => false;

        protected override bool ExecuteCommand(ICollection<Exception> exceptions)
        {
            try
            {
                _console.Blank();
                _console.Info("Launching application...");
                _launcher.Run();
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