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

namespace AK.Vault.Console
{
    /// <summary>
    /// Promps the user for components of the encryption key input in a protected manner.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal static class EncryptionKeyInputPrompter
    {
        /// <summary>
        /// Prompts the user for all components and creates an encryption key input structure for use
        /// by the application.
        /// </summary>
        /// <returns>Encryption key input structure (or NULL if cancelled).</returns>
        public static EncryptionKeyInput Prompt()
        {
            Screen.Print("Please provide the information you want to use to encrypt or decrypt.");
            Screen.Print("Information that you type will be masked and not visible.");
            Screen.Print("Press ESC to Cancel.");
            Screen.Print();

            bool cancelled;

            var fullName = ProtectedPrompt("Full Name", out cancelled);
            if (cancelled) return null;

            var dateOfBirth = ProtectedDatePrompt("Date of Birth", out cancelled);
            if (cancelled) return null;

            var socialSecurityNumber = ProtectedPrompt("Social Security Number", out cancelled);
            if (cancelled) return null;

            var passPhrase = ProtectedPrompt("Passphrase", out cancelled);
            if (cancelled) return null;

            return new EncryptionKeyInput
                {
                    FullName = fullName,
                    DateOfBirth = dateOfBirth,
                    SocialSecurityNumber = socialSecurityNumber,
                    PassPhrase = passPhrase
                };
        }

        private static DateTime ProtectedDatePrompt(string prompt, out bool cancelled)
        {
            while (true)
            {
                System.Console.Write(prompt + ": ");
                var result = GetProtectedInput(out cancelled);

                if (cancelled) return DateTime.MinValue;
                if (string.IsNullOrWhiteSpace(result))
                {
                    Screen.Print(Screen.Colors.Error, "You can't enter empty/whitespace.");
                    continue;
                }
                DateTime resultDate;
                if (DateTime.TryParse(result, out resultDate)) return resultDate;

                Screen.Print(Screen.Colors.Error, "That is not a valid date.");
            }
        }

        private static string ProtectedPrompt(string prompt, out bool cancelled)
        {
            while (true)
            {
                System.Console.Write(prompt + ": ");
                var result = GetProtectedInput(out cancelled);

                if (cancelled) return null;
                if (!string.IsNullOrWhiteSpace(result)) return result;

                Screen.Print(Screen.Colors.Error, "You can't enter empty/whitespace.");
            }
        }

        private static string GetProtectedInput(out bool cancelled)
        {
            var result = string.Empty;
            cancelled = false;
            while (true)
            {
                var keyInfo = Screen.ReadKey();
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Screen.Print();
                    break;
                }

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    cancelled = true;
                    return null;
                }

                result += keyInfo.KeyChar;
                System.Console.Write('*');
            }
            return result;
        }
    }
}
