/*******************************************************************************************************************************
 * AK.Vault.Console.ICommand
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

namespace AK.Vault.Console
{
    /// <summary>
    /// Represents a vault command.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal interface ICommand
    {
        /// <summary>
        /// The vault name for use by the command.
        /// </summary>
        string VaultName { get; set; }

        /// <summary>
        /// Assigns the encryption key input, if one is needed by the command.
        /// </summary>
        /// <param name="encryptionKeyInput">Encryption key input.</param>
        void AssignEncryptionKeyInput(EncryptionKeyInput encryptionKeyInput);

        /// <summary>
        /// Assigns parameters from the command line - it is up to the command
        /// to interpret them and set own state.
        /// </summary>
        /// <returns>Whether parameters were successfully parsed and assigned.</returns>
        bool ProcessParameters();

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>Whether execution was successful.</returns>
        bool Execute();
    }
}