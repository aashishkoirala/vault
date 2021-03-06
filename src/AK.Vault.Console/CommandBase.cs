﻿/*******************************************************************************************************************************
 * AK.Vault.Console.CommandBase
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

#endregion

namespace AK.Vault.Console
{
    /// <summary>
    /// Base class with common functionality for all ICommand instances.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal abstract class CommandBase : ICommand
    {
        protected IFileEncryptor FileEncryptor;

        protected virtual bool PromptBeforeStart => true;

        protected virtual bool PromptAfterEnd => true;

        public abstract bool AssignParameters(string[] args);

        public virtual string VaultName { get; set; }

        public virtual void AssignEncryptionKeyInput(EncryptionKeyInput encryptionKeyInput)
        {
            this.FileEncryptor = Factory.CreateFileEncryptor(encryptionKeyInput, this.VaultName);
            this.FileEncryptor.UpdateMessageAction =
                message =>
                Screen.Print(message.IsError ? Screen.Colors.Error : Screen.Colors.Normal, message.Description);
        }

        public bool Execute()
        {
            this.WriteHeader();
            if (this.PromptBeforeStart && !this.PromptContinueOrCancel()) return false;

            var exceptions = new Collection<Exception>();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var result = false;
            try
            {
                result = this.ExecuteCommand(exceptions);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            stopWatch.Stop();

            Screen.Print();
            Screen.Print("Operation took {0}.", stopWatch.Elapsed);

            if (exceptions.Any())
            {
                var errorLog = Path.Combine(Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Error.log");

                var dashes = $"{new string('-', 50)}{Environment.NewLine}";
                File.WriteAllText(errorLog, dashes);
                foreach (var ex in exceptions)
                {
                    File.AppendAllText(errorLog, ex.ToString());
                    File.AppendAllText(errorLog, dashes);
                }

                Screen.Print(Screen.Colors.Error, "There were errors. See {0} for details.", errorLog);
            }

            if (this.PromptAfterEnd) this.PromptContinue();
            return result;
        }

        protected abstract bool ExecuteCommand(ICollection<Exception> exceptions);

        protected virtual void WriteHeader() { }

        protected bool PromptContinueOrCancel(string prompt = "Press ENTER to continue, ESC to Cancel.")
        {
            Screen.Print(prompt);
            while (true)
            {
                var keyInfo = Screen.ReadKey();
                if (keyInfo.Key == ConsoleKey.Escape) return false;
                if (keyInfo.Key == ConsoleKey.Enter) return true;
            }
        }

        protected void PromptContinue(string prompt = "Press ENTER to continue.")
        {
            Screen.Print(prompt);
            while (Screen.ReadKey().Key != ConsoleKey.Enter) {}
        }
    }
}