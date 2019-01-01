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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AK.Vault.Console
{
    /// <summary>
    /// Parses command line arguments to create the correct ICommand instance
    /// and execute it.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class CommandExecutor : IHostedService
    {
        private readonly ApplicationState _applicationState;
        private readonly VaultSelector _vaultSelector;
        private readonly EncryptionKeyEvaluator _encryptionKeyEvaluator;
        private readonly VaultOptions _vaultOptions;
        private readonly Func<string, ICommand> _commandFunc;
        private readonly ConsoleWriter _console;
        private readonly ILogger _logger;

        public CommandExecutor(ApplicationState applicationState,
            VaultSelector vaultSelector, EncryptionKeyEvaluator encryptionKeyEvaluator,
            IOptionsMonitor<VaultOptions> vaultOptionsMonitor,
            Func<string, ICommand> commandFunc,
            ConsoleWriter console,
            ILogger<CommandExecutor> logger)
        {
            _applicationState = applicationState;
            _vaultSelector = vaultSelector;
            _encryptionKeyEvaluator = encryptionKeyEvaluator;
            _vaultOptions = vaultOptionsMonitor.CurrentValue;
            _commandFunc = commandFunc;
            _console = console;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            try
            {
                _console.Heading("VAULT by Aashish Koirala (c) 2014-2019");
                _console.Blank();
                var command = await ParseCommand();
                if (command == null)
                {
                    PrintUsage();
                    _applicationState.ReturnCode = 1;
                }
                else _applicationState.ReturnCode = (await command.Execute()) ? 0 : 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during command parse/execution.");
                _console.Error("There was an unexpected error, please see the logs for details.");
                _applicationState.ReturnCode = 1;
            }
            _applicationState.CancellationTokenSource.Cancel();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task<ICommand> ParseCommand()
        {
            try
            {
                var commandName = _vaultOptions.Command ?? "launch";
                var command = _commandFunc(commandName);
                if (command == null)
                {
                    _console.Error($"Invalid command: {commandName}");
                    return null;
                }
                if (!command.IsValid)
                {
                    _console.Error($"Invalid parameters for {commandName}.");
                    return null;
                }

                var commandInfo = command.GetType().GetCustomAttribute<CommandInfoAttribute>();

                var vaultName = _vaultSelector.SelectVault();
                if (vaultName == null) return null;
                command.VaultName = vaultName;
                _console.Clear();

                if (!commandInfo.RequiresEncryptionKeyInput) return command;

                var encryptionKeyInput = await _encryptionKeyEvaluator.EvaluateEncryptionKeyInput();
                if (encryptionKeyInput == null) return null;
                command.AssignEncryptionKeyInput(encryptionKeyInput);

                return command;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while parsing command.");
                _console.Error($"Error parsing command, please see logs for details.");
                return null;
            }
        }

        private void PrintUsage()
        {
            _console.Blank();
            _console.Heading("Usage:");
            _console.Info("\tvault [--command=command-name] [--target=target] [--vault=vault-name] [--key=base64-key|--keyinput=key-input|--keyfile=key-file-path]");
            _console.Blank();
            _console.Info("\tcommand-name:");
            _console.Info("\t\tlaunch\tLaunches the interactive application, default if omitted.");
            _console.Info("\t\tencrypt\tEncrypts the files specified by the \"target\" argument.");
            _console.Info("\t\tdecrypt\tDecrypts the files specified by the \"target\" argument.");
            _console.Info("\t\tlist\tLists the content of the vault.");
            _console.Info("\t\tfind\tFinds the file specified in the \"target\" argument.");
            _console.Info("\t\tfd\tFinds and decrypts the file specified in the \"target\" argument.");
            _console.Info("\t\treport\tSpits out a tabular report of all files in the vault with their encrypted names.");
            _console.Blank();
            _console.Info("\ttarget:");
            _console.Info("\t\tOnly required when command = encrypt, decrypt, find or fd");
            _console.Info("\t\tencrypt/decrypt\tfilepattern1;[filepattern2];[filepattern3]...");
            _console.Info("\t\tfind/fd\tfilename");
            _console.Blank();
            _console.Info("\tvault-name:");
            _console.Info("\t\tVault name to use if multiple vaults are configured. Not needed if only 1 vault is configured.");
            _console.Info("\t\tIf multiple are configured and you don't specify this, you will be prompted to pick one.");
            _console.Blank();
            _console.Info("\tbase64-key:");
            _console.Info("\t\tIf directly specifying the raw encryption key, the Base64 encoded value of the key.");
            _console.Blank();
            _console.Info("\tkey-input:");
            _console.Info("\t\tIf not directly specifying the raw encryption key, the passphrase that will be used to generate the key.");
            _console.Info("\t\tIf not provided and --key or--keyfile are not provided, this value will be prompted for.");
            _console.Blank();
            _console.Info("\tkey-file-path:");
            _console.Info("\t\tIf the raw key is stored in a file, the path to the file.");
            _console.Blank();
        }
    }
}