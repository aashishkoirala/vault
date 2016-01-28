/*******************************************************************************************************************************
 * AK.Vault.Console.CommandInfoAttribute
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
using System.ComponentModel.Composition;

#endregion

namespace AK.Vault.Console
{
    /// <summary>
    /// MEF metadata view for ICommand.
    /// </summary>
    public interface ICommandInfo
    {
        /// <summary>
        /// Name of the command.
        /// </summary>
        string CommandName { get; }

        /// <summary>
        /// Whether the command requires encryption key input.
        /// </summary>
        bool RequiresEncryptionKeyInput { get; }
    }

    /// <summary>
    /// MEF metadata attribute that implements ICommandInfo and should be applied
    /// to ICommand instances.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [MetadataAttribute]
    internal class CommandInfoAttribute : Attribute, ICommandInfo
    {
        public CommandInfoAttribute(string commandName, bool requiresEncryptionKeyInput)
        {
            this.CommandName = commandName;
            this.RequiresEncryptionKeyInput = requiresEncryptionKeyInput;
        }

        public string CommandName { get; }
        public bool RequiresEncryptionKeyInput { get; }
    }
}