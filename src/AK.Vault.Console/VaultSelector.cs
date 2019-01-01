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
using System.Linq;

namespace AK.Vault.Console
{
    /// <summary>
    /// Promps the user for which vault to work with.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal class VaultSelector
    {
        private readonly VaultOptions _vaultOptions;
        private readonly ConsoleWriter _console;

        public VaultSelector(IOptionsMonitor<VaultOptions> vaultOptionsMonitor, ConsoleWriter console)
        {
            _vaultOptions = vaultOptionsMonitor.CurrentValue;
            _console = console;
        }

        public string SelectVault()
        {
            var vaults = _vaultOptions.Vaults;
            var vault = _vaultOptions.Vault;
            if (!string.IsNullOrWhiteSpace(vault))
            {
                if (vaults.Any(x => x.Name == vault)) return vault;
                else
                {
                    _console.Error($"Invalid vault- {vault}.");
                    return null;
                }
            }

            if (!vaults.Any())
            {
                _console.Error("Oops - no vaults configured!");
                return null;
            }

            if (vaults.Count == 1) return vaults.Single().Name;

            _console.Info("Please pick a vault to work with:");
            _console.Info("Press ESC to Cancel.");
            _console.Blank();

            var menuItems = vaults
                .OrderBy(x => x.Name)
                .Select((x, i) => new {Key = GetKeyChar(i), Value = x.Name})
                .ToDictionary(x => x.Key, x => x.Value);

            foreach (var key in menuItems.Keys) _console.Info($"{key} - {menuItems[key]}");

            while (true)
            {
                var keyInfo = _console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Escape) return null;

                if (menuItems.TryGetValue(keyInfo.KeyChar, out string vaultName)) return vaultName;
                if (menuItems.TryGetValue(keyInfo.KeyChar.ToString().ToUpper()[0], out vaultName)) return vaultName;
                if (menuItems.TryGetValue(keyInfo.KeyChar.ToString().ToLower()[0], out vaultName)) return vaultName;
            }
        }

        private static char GetKeyChar(int index)
        {
            var number = index + 1;
            if (number <= 9) return number.ToString()[0];
            if (number == 10) return '0';

            return (char) (number - 10 + 'A');
        }
    }
}
