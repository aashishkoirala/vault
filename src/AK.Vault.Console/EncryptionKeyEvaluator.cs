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

using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Reflection;

namespace AK.Vault.Console
{
    /// <summary>
    /// Promps the user for components of the encryption key input in a protected manner.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class EncryptionKeyEvaluator
    {
        private readonly VaultOptions _vaultOptions;
        private readonly ConsoleWriter _console;

        public EncryptionKeyEvaluator(IOptionsMonitor<VaultOptions> vaultOptionsMonitor,
            ConsoleWriter console)
        {
            _vaultOptions = vaultOptionsMonitor.CurrentValue;
            _console = console;
        }

        /// <summary>
        /// Prompts the user for all components and creates an encryption key input structure for use
        /// by the application.
        /// </summary>
        /// <returns>Encryption key input structure (or NULL if cancelled).</returns>
        public EncryptionKeyInput EvaluateEncryptionKey()
        {
            if (!string.IsNullOrWhiteSpace(_vaultOptions.Key))
            {
                var span = new Span<byte>();
                if (!Convert.TryFromBase64String(_vaultOptions.Key, span, out var bytesWritten))
                {
                    _console.Error("Invalid format for Base64 key.");
                    return null;
                }
                return new EncryptionKeyInput { Key = span.ToArray() };
            }

            if (!string.IsNullOrWhiteSpace(_vaultOptions.KeyFile))
            {
                try
                {
                    var path = _vaultOptions.KeyFile;
                    if (!Path.IsPathRooted(path)) path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path));
                    return new EncryptionKeyInput { Key = File.ReadAllBytes(path) };
                }
                catch (Exception ex)
                {
                    _console.Error($"Invalid key file: {ex.Message}.");
                    return null;
                }
            }

            var keyInput = _vaultOptions.KeyInput;
            if (string.IsNullOrWhiteSpace(keyInput))
            {
                _console.Info("Please provide the information you want to use to encrypt or decrypt.");
                _console.Info("Information that you type will be masked and not visible.");
                _console.Info("Press ESC to Cancel.");
                _console.Blank();
                _console.Info("In order to avoid this prompt, you can directly provide this information");
                _console.Info("or the key itself in the command line.");
                _console.Blank();

                keyInput = ProtectedPrompt("Key Input", out bool cancelled);
                if (cancelled || string.IsNullOrWhiteSpace(keyInput)) return null;
            }

            return new EncryptionKeyInput { KeyInput = keyInput };
        }

        private string ProtectedPrompt(string prompt, out bool cancelled)
        {
            while (true)
            {
                _console.Info(prompt + ": ", true);
                var result = GetProtectedInput(out cancelled);

                if (cancelled) return null;
                if (!string.IsNullOrWhiteSpace(result)) return result;

                _console.Error("You can't enter empty/whitespace.");
            }
        }

        private string GetProtectedInput(out bool cancelled)
        {
            var result = string.Empty;
            cancelled = false;
            while (true)
            {
                var keyInfo = _console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    _console.Blank();
                    break;
                }

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    cancelled = true;
                    return null;
                }

                result += keyInfo.KeyChar;
                _console.Info("*", true);
            }
            return result;
        }
    }
}