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

using System;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AK.Vault.Console
{
    /// <summary>
    /// Parses command line arguments to create the correct ICommand instance
    /// and execute it.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class CommandParser
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CommandParser(CancellationTokenSource cancellationTokenSource) => _cancellationTokenSource = cancellationTokenSource;

        public int ReturnCode { get; private set; }

        public void ParseAndExecute(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            try
            {
                Screen.Print(Screen.Colors.Heading, "VAULT by Aashish Koirala (c) 2014-2019");
                Screen.Print();
                var command = Parse(configuration, serviceProvider);
                if (command == null)
                {
                    PrintUsage();
                    ReturnCode = 1;
                }
                else ReturnCode = command.Execute() ? 0 : 1;
            }
            catch (Exception ex)
            {
                Screen.Print(Screen.Colors.Error, "Unexpected error:{0}{1}{0}", Environment.NewLine, ex);
                ReturnCode = 1;
            }
            _cancellationTokenSource.Cancel();
        }

        private static ICommand Parse(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            try
            {
                var commandName = configuration["command"] ?? "launch";
                var command = serviceProvider.GetService<Func<string, ICommand>>()(commandName);
                if (command == null)
                {
                    Screen.Print(Screen.Colors.Error, $"Invalid command: {commandName}");
                    return null;
                }
                if (!command.ProcessParameters())
                {
                    Screen.Print(Screen.Colors.Error, "Invalid parameters for {commandName}.", commandName);
                    return null;
                }

                var commandInfo = command.GetType().GetCustomAttribute<CommandInfoAttribute>();

                var vaultName = VaultPrompter.Prompt(serviceProvider.GetService<IOptionsMonitor<VaultOptions>>().CurrentValue);
                if (vaultName == null) return null;
                command.VaultName = vaultName;
                Screen.Clear();

                if (!commandInfo.RequiresEncryptionKeyInput) return command;

                var encryptionKeyInput = EncryptionKeyInputPrompter.Prompt();
                if (encryptionKeyInput == null) return null;
                command.AssignEncryptionKeyInput(encryptionKeyInput);

                return command;
            }
            catch (Exception ex)
            {
                Screen.Print(Screen.Colors.Error, "Error parsing command: {0}", ex.Message);
                return null;
            }
        }

        private static void PrintUsage()
        {
            Screen.Print();
            Screen.Print(Screen.Colors.Heading, "Usage:");
            Screen.Print("\tvault [launch]");
            Screen.Print("\t\tLaunches the interactive application.");
            Screen.Print();
            Screen.Print("\tvault encrypt filepattern1 [filepattern2] [filepattern3] ...");
            Screen.Print("\t\tEncrypts the given files.");
            Screen.Print();
            Screen.Print("\tvault encrypt filepattern1 [filepattern2] [filepattern3] ...");
            Screen.Print("\t\tDecrypts the given files.");
            Screen.Print();
            Screen.Print("\tvault list");
            Screen.Print("\t\tLists the content of the vault.");
            Screen.Print();
            Screen.Print("\tvault find filename");
            Screen.Print("\t\tChecks to see if the file is in the vault and gives you the encrypted filename if it does.");
            Screen.Print();
            Screen.Print("\tvault fd filename1 [filename2] [filename3] ...");
            Screen.Print("\t\tPerforms a \"find\" on each file, and if found, decrypts it.");
            Screen.Print();
            Screen.Print("\tvault report");
            Screen.Print("\t\tSpits out a tabular report of all files in the vault with their encrypted names.");
            Screen.Print();
        }
    }
}
