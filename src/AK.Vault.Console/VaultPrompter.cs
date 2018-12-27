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
using System.Linq;

namespace AK.Vault.Console
{
    /// <summary>
    /// Promps the user for which vault to work with.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal static class VaultPrompter
    {
        /// <summary>
        /// Prompts the user for all components and creates an encryption key input structure for use
        /// by the application.
        /// </summary>
        /// <param name="cancelled">This is set to whether the user cancelled instead of entering.</param>
        /// <param name="configurationProvider">Configuration provider object.</param>
        /// <returns>Encryption key input structure.</returns>
        public static string Prompt(VaultOptions vaultOptions)
        {
            var vaults = vaultOptions.Vaults;
            if (!vaults.Any())
            {
                Screen.Print("Oops - no vaults configured!");
                return null;
            }

            if (vaults.Count == 1) return vaults.Single().Name;

            Screen.Print("Please pick a vault to work with:");
            Screen.Print("Press ESC to Cancel.");
            Screen.Print();

            var menuItems = vaults
                .OrderBy(x => x.Name)
                .Select((x, i) => new {Key = GetKeyChar(i), Value = x.Name})
                .ToDictionary(x => x.Key, x => x.Value);

            foreach (var key in menuItems.Keys) Screen.Print("{0} - {1}", key, menuItems[key]);

            while (true)
            {
                var keyInfo = Screen.ReadKey();
                if (keyInfo.Key == ConsoleKey.Escape) return null;

                string vaultName;
                if (menuItems.TryGetValue(keyInfo.KeyChar, out vaultName)) return vaultName;
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
